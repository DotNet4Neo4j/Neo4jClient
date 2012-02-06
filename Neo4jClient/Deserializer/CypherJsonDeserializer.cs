using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Extensions;

namespace Neo4jClient.Deserializer
{
    public class CypherJsonDeserializer<TResult>
        where TResult : new()
    {
        public CultureInfo Culture { get; set; }

        public CypherJsonDeserializer()
        {
            Culture = CultureInfo.InvariantCulture;
        }

        public IEnumerable<TResult> Deserialize(RestResponse response)
        {
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

            var dataArray = (JArray)root["data"];
            var rows = dataArray.Children();
            var results = rows.Select(row => ReadRow(row, propertiesDictionary, columnNames));

            return results;
        }

        TResult ReadRow(JToken row, Dictionary<string, PropertyInfo> propertiesDictionary, string[] columnNames)
        {
            var result = new TResult();

            var cellIndex = 0;
            foreach(var cell in row.Children())
            {
                var columnName = columnNames[cellIndex];
                var property = propertiesDictionary[columnName];
                CommonDeserializerMethods.SetPropertyValue(result, property, cell, Culture);
                cellIndex++;
            }

            return result;
        }
    }
}
