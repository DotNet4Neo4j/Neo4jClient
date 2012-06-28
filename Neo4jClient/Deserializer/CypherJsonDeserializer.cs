using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Extensions;

namespace Neo4jClient.Deserializer
{
    public class CypherJsonDeserializer<TResult>
    {
        readonly IGraphClient client;
        readonly CypherResultMode resultMode;

        public CultureInfo Culture { get; set; }

        public CypherJsonDeserializer(IGraphClient client, CypherResultMode resultMode)
        {
            this.client = client;
            this.resultMode = resultMode;
            Culture = CultureInfo.InvariantCulture;
        }

        public IEnumerable<TResult> Deserialize(RestResponse response)
        {
            response.Content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(response.Content);
            var content = response.Content;
            var root = JObject.Parse(content).Root;

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
                    throw new ArgumentException("Unrecognised mode.", "mode");
            }
        }

        IEnumerable<TResult> ParseInSingleColumnMode(JToken root, string[] columnNames, TypeMapping[] jsonTypeMappings)
        {
            if (columnNames.Count() != 1)
                throw new InvalidOperationException("The deserializer is running in single column mode, but the response included multiple columns which indicates a projection instead.");

            var resultType = typeof (TResult);
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

                var parsed = CommonDeserializerMethods.CreateAndMap(newType, row[0], Culture, jsonTypeMappings, 0);
                if (mapping == null)
                    if (parsed is IConvertible)
                        return (TResult) Convert.ChangeType(parsed, typeof (TResult));
                    else return (TResult) parsed;

                return (TResult) mapping.MutationCallback(parsed);
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
            string[] columnNames,
            TypeMapping[] jsonTypeMappings)
        {
            var result = Activator.CreateInstance<TResult>();

            var cellIndex = 0;
            foreach(var cell in row.Children())
            {
                var columnName = columnNames[cellIndex];
                var property = propertiesDictionary[columnName];
                CommonDeserializerMethods.SetPropertyValue(result, property, cell, Culture, jsonTypeMappings, 0);
                cellIndex++;
            }

            return result;
        }
    }
}
