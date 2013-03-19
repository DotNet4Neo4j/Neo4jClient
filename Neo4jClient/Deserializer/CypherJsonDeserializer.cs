using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.Deserializer
{
    public class CypherJsonDeserializer<TResult>
    {
        readonly IGraphClient client;
        readonly CypherResultMode resultMode;

        readonly CultureInfo culture = CultureInfo.InvariantCulture;

        public CypherJsonDeserializer(IGraphClient client, CypherResultMode resultMode)
        {
            this.client = client;
            this.resultMode = resultMode;
        }

        public IEnumerable<TResult> Deserialize(string content)
        {
            content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

            var reader = new JsonTextReader(new StringReader(content))
            {
                DateParseHandling = DateParseHandling.DateTimeOffset
            };
            var root = JToken.ReadFrom(reader).Root;

            var columnsArray = (JArray)root["columns"];
            var columnNames = columnsArray
                .Children()
                .Select(c => c.AsString())
                .ToArray();

            var jsonTypeMappings = new List<TypeMapping>
            {
                new TypeMapping
                {
                    ShouldTriggerForPropertyType = (nestingLevel, type) =>
                        type.IsGenericType &&
                        type.GetGenericTypeDefinition() == typeof(Node<>),
                    DetermineTypeToParseJsonIntoBasedOnPropertyType = t =>
                    {
                        var nodeType = t.GetGenericArguments();
                        return typeof (NodeApiResponse<>).MakeGenericType(nodeType);
                    },
                    MutationCallback = n => n.GetType().GetMethod("ToNode").Invoke(n, new object[] { client })
                },
                new TypeMapping
                {
                    ShouldTriggerForPropertyType = (nestingLevel, type) =>
                        type.IsGenericType &&
                        type.GetGenericTypeDefinition() == typeof(RelationshipInstance<>),
                    DetermineTypeToParseJsonIntoBasedOnPropertyType = t =>
                    {
                        var relationshipType = t.GetGenericArguments();
                        return typeof (RelationshipApiResponse<>).MakeGenericType(relationshipType);
                    },
                    MutationCallback = n => n.GetType().GetMethod("ToRelationshipInstance").Invoke(n, new object[] { client })
                }
            };

            switch (resultMode)
            {
                case CypherResultMode.Set:
                    return ParseInSingleColumnMode(root, columnNames, jsonTypeMappings.ToArray());
                case CypherResultMode.Projection:
                    jsonTypeMappings.Add(new TypeMapping
                    {
                        ShouldTriggerForPropertyType = (nestingLevel, type) =>
                            nestingLevel == 0 && type.IsClass,
                        DetermineTypeToParseJsonIntoBasedOnPropertyType = t =>
                            typeof(NodeOrRelationshipApiResponse<>).MakeGenericType(new[] { t }),
                        MutationCallback = n =>
                            n.GetType().GetProperty("Data").GetGetMethod().Invoke(n, new object[0])
                    });
                    return ParseInProjectionMode(root, columnNames, jsonTypeMappings.ToArray());
                default:
                    throw new NotSupportedException(string.Format("Unrecognised result mode of {0}.", resultMode));
            }
        }

// ReSharper disable UnusedParameter.Local
        IEnumerable<TResult> ParseInSingleColumnMode(JToken root, string[] columnNames, TypeMapping[] jsonTypeMappings)
// ReSharper restore UnusedParameter.Local
        {
            if (columnNames.Count() != 1)
                throw new InvalidOperationException("The deserializer is running in single column mode, but the response included multiple columns which indicates a projection instead.");

            var resultType = typeof(TResult);
            var isResultTypeANodeOrRelationshipInstance = resultType.IsGenericType &&
                                       (resultType.GetGenericTypeDefinition() == typeof(Node<>) ||
                                        resultType.GetGenericTypeDefinition() == typeof(RelationshipInstance<>));
            var mapping = jsonTypeMappings.SingleOrDefault(m => m.ShouldTriggerForPropertyType(0, resultType));
            var newType = mapping == null ? resultType : mapping.DetermineTypeToParseJsonIntoBasedOnPropertyType(resultType);

            var dataArray = (JArray)root["data"];
            var rows = dataArray.Children();
            var results = rows.Select(row =>
            {
                if (!(row is JArray))
                    throw new InvalidOperationException("Expected the row to be a JSON array of values, but it wasn't.");

                var rowAsArray = (JArray) row;
                if (rowAsArray.Count != 1)
                    throw new InvalidOperationException(string.Format("Expected the row to only have a single array value, but it had {0}.", rowAsArray.Count));

                var elementToParse = row[0];
                if (elementToParse is JObject)
                {
                    var propertyNames = ((JObject) elementToParse)
                        .Properties()
                        .Select(p => p.Name)
                        .ToArray();
                    var dataElementLooksLikeANodeOrRelationshipInstance =
                        new[] {"data", "self", "traverse", "properties"}.All(propertyNames.Contains);
                    if (!isResultTypeANodeOrRelationshipInstance &&
                        dataElementLooksLikeANodeOrRelationshipInstance)
                    {
                        elementToParse = elementToParse["data"];
                    }
                }

                var parsed = CommonDeserializerMethods.CreateAndMap(newType, elementToParse, culture, jsonTypeMappings, 0);
                return (TResult)(mapping == null ? parsed : mapping.MutationCallback(parsed));
            });

            return results;
        }

        IEnumerable<TResult> ParseInProjectionMode(JToken root, string[] columnNames, TypeMapping[] jsonTypeMappings)
        {
            var properties = typeof(TResult).GetProperties();
            var propertiesDictionary = properties
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name);

            var columnsWhichDontHaveSettableProperties = columnNames.Where(c => !propertiesDictionary.ContainsKey(c)).ToArray();
            if (columnsWhichDontHaveSettableProperties.Any())
            {
                var columnsWhichDontHaveSettablePropertiesCommaSeparated = string.Join(", ", columnsWhichDontHaveSettableProperties);
                throw new ArgumentException(string.Format(
                    "The query response contains columns {0} however {1} does not contain publically settable properties to receive this data.",
                    columnsWhichDontHaveSettablePropertiesCommaSeparated,
                    typeof(TResult).FullName),
                    "columnNames");
            }

            var dataArray = (JArray)root["data"];
            var rows = dataArray.Children();
            var results = rows.Select(row => ReadProjectionRow(row, propertiesDictionary, columnNames, jsonTypeMappings));

            return results;
        }

        TResult ReadProjectionRow(
            JToken row,
            IDictionary<string, PropertyInfo> propertiesDictionary,
            IList<string> columnNames,
            TypeMapping[] jsonTypeMappings)
        {
            var result = Activator.CreateInstance<TResult>();

            var cellIndex = 0;
            foreach(var cell in row.Children())
            {
                var columnName = columnNames[cellIndex];
                cellIndex++;

                var property = propertiesDictionary[columnName];
                var propertyType = property.PropertyType;
                var cellChildren = cell.Children().ToArray();
                if (propertyType.IsGenericType &&
                    propertyType.GetGenericTypeDefinition() == typeof (IEnumerable<>) &&
                    cell.Type == JTokenType.Array &&
                    cellChildren.Count() == 1 &&
                    cellChildren.Single() != null &&
                    cellChildren.Single().Type == JTokenType.Null)
                    continue;

                CommonDeserializerMethods.SetPropertyValue(result, property, cell, culture, jsonTypeMappings, 0);
            }

            return result;
        }
    }
}
