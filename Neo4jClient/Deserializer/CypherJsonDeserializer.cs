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
        readonly CypherMode mode;

        public CultureInfo Culture { get; set; }

        public CypherJsonDeserializer(IGraphClient client, CypherMode mode)
        {
            this.client = client;
            this.mode = mode;
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

            var jsonTypeMappings = new[]
            {
                new TypeMapping
                {
                    PropertyTypeToTriggerMapping = typeof(Node<>),
                    DetermineTypeToParseJsonIntoBasedOnPropertyType = t =>
                    {
                        var nodeType = t.GetGenericArguments();
                        return typeof (NodeApiResponse<>).MakeGenericType(nodeType);
                    },
                    MutationCallback = n => n.GetType().GetMethod("ToNode").Invoke(n, new object[] { client })
                },
                new TypeMapping
                {
                    PropertyTypeToTriggerMapping = typeof(RelationshipInstance<>),
                    DetermineTypeToParseJsonIntoBasedOnPropertyType = t =>
                    {
                        var relationshipType = t.GetGenericArguments();
                        return typeof (RelationshipApiResponse<>).MakeGenericType(relationshipType);
                    },
                    MutationCallback = n => n.GetType().GetMethod("ToRelationshipInstance").Invoke(n, new object[] { client })
                }
            };

            switch (mode)
            {
                case CypherMode.SingleColumn:
                    return ParseInSingleColumnMode(root, columnNames, jsonTypeMappings);
                case CypherMode.Projection:
                    return ParseInProjectionMode(root, columnNames, jsonTypeMappings);
                default:
                    throw new ArgumentException("Unrecognised mode.", "mode");
            }
        }

        IEnumerable<TResult> ParseInSingleColumnMode(JToken root, string[] columnNames, TypeMapping[] jsonTypeMappings)
        {
            if (columnNames.Count() != 1)
                throw new InvalidOperationException("The deserializer is running in single column mode, but the response included multiple columns which indicates a projection instead.");

            var resultType = typeof (TResult);
            var genericTypeDefinition = resultType.IsGenericType ? resultType.GetGenericTypeDefinition() : null;
            var mapping = jsonTypeMappings.SingleOrDefault(m =>
                m.PropertyTypeToTriggerMapping == resultType ||
                m.PropertyTypeToTriggerMapping == genericTypeDefinition);
            var newType = mapping == null ? resultType : mapping.DetermineTypeToParseJsonIntoBasedOnPropertyType(resultType);

            var dataArray = (JArray)root["data"];
            var rows = dataArray.Children();
            var results = rows.Select(row =>
            {
                var parsed = CommonDeserializerMethods.CreateAndMap(newType, row[0], Culture, jsonTypeMappings);
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
            string[] columnNames,
            TypeMapping[] jsonTypeMappings)
        {
            var result = Activator.CreateInstance<TResult>();

            var cellIndex = 0;
            foreach(var cell in row.Children())
            {
                var columnName = columnNames[cellIndex];
                var property = propertiesDictionary[columnName];
                CommonDeserializerMethods.SetPropertyValue(result, property, cell, Culture, jsonTypeMappings);
                cellIndex++;
            }

            return result;
        }
    }
}
