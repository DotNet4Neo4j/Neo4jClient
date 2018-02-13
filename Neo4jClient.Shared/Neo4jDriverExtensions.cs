using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neo4j.Driver.V1;
using Neo4jClient.Cypher;
using Neo4jClient.Serialization;
using Newtonsoft.Json;

namespace Neo4jClient
{
    public static class Neo4jDriverExtensions
    {
        public static IStatementResult Run(this ISession session, CypherQuery query, IGraphClient gc)
        {
            return session.Run(query.QueryText, query.ToNeo4jDriverParameters(gc));
        }

        public static IStatementResult Run(this ITransaction session, CypherQuery query, IGraphClient gc)
        {
            return session.Run(query.QueryText, query.ToNeo4jDriverParameters(gc));
        }

        // ReSharper disable once InconsistentNaming
        public static Dictionary<string, object> ToNeo4jDriverParameters(this CypherQuery query, IGraphClient gc)
        {
            var output = new Dictionary<string, object>();

            foreach (var item in query.QueryParameters)
            {
                var type = item.Value.GetType();
                var typeInfo = type.GetTypeInfo();

                if (typeInfo.IsClass && type != typeof(string))
                {
                    object itemToAdd;
                    if (typeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable)))
                    {
                        if (typeInfo.IsArray)
                        {
                            itemToAdd = item.Value;
                        }
                        else if (typeInfo.IsGenericType && type.GenericTypeArguments.Length == 1)
                        {
                            var genericType = type.GenericTypeArguments[0];
                            var genericTypeInfo = genericType.GetTypeInfo();
                            if (genericTypeInfo.IsValueType || genericType == typeof(string))
                                itemToAdd = item.Value;
                            else
                                itemToAdd = ConvertListToListOfDictionaries((IEnumerable) item.Value, gc);
                        }

                        else itemToAdd = ConvertListToListOfDictionaries((IEnumerable) item.Value, gc);
                    }
                    else
                    {
                        var serialized = JsonConvert.SerializeObject(item.Value, Formatting.None, gc.JsonConverters.ToArray());
                        itemToAdd = JsonConvert.DeserializeObject<Dictionary<string, object>>(serialized, new JsonSerializerSettings{DateParseHandling = DateParseHandling.None} );
                    }

                    output.Add(item.Key, itemToAdd);
                }
                else if (type == typeof(string) || typeInfo.IsPrimitive)
                    output.Add(item.Key, item.Value);
                else
                    output.Add(item.Key, JsonConvert.SerializeObject(item.Value));
            }

            return output;
        }

        private static IEnumerable<IDictionary<string, object>> ConvertListToListOfDictionaries(IEnumerable list, IGraphClient gc)
        {
            var output = new List<IDictionary<string, object>>();
            foreach (var item in list)
                output.Add(JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(item, Formatting.None, gc.JsonConverters.ToArray())));

            return output;
        }
    }
}