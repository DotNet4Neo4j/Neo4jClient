using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Neo4jClient.ApiModels;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Extensions;

namespace Neo4jClient.Deserializer
{
    public class CypherJsonDeserializer<TResult>
        where TResult : new()
    {
        readonly IGraphClient client;
        public CultureInfo Culture { get; set; }

        public CypherJsonDeserializer(IGraphClient client)
        {
            this.client = client;
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

            var properties = typeof (TResult).GetProperties();
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
                    "response");
            }

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

            var dataArray = (JArray)root["data"];
            var rows = dataArray.Children();
            var results = rows.Select(row => ReadRow(row, propertiesDictionary, columnNames, jsonTypeMappings));

            return results;
        }

        TResult ReadRow(
            JToken row,
            IDictionary<string, PropertyInfo> propertiesDictionary,
            string[] columnNames,
            IEnumerable<TypeMapping> jsonTypeMappings)
        {
            var result = new TResult();

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
