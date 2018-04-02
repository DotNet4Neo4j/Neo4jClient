using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using Neo4j.Driver.V1;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace Neo4jClient
{
    public static class Neo4jDriverExtensions
    {
        private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

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
            return query.QueryParameters.ToDictionary(item => item.Key, item => Serialize(item.Value));
        }

        private static object Serialize(object value)
        {
            if (value == null)
            {
                return null;
            }

            var type = value.GetType();
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return SerializeDictionary(type, value);
            }

            if (typeInfo.IsClass && type != typeof(string))
            {
                if (typeInfo.IsArray || typeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable)))
                {
                    return SerializeCollection((IEnumerable)value);
                }

                return SerializeObject(type, value);
            }

            return SerializePrimitive(type, typeInfo, value);
        }
        
        private static object SerializeObject(Type type, object value)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => !(pi.GetIndexParameters().Any() || pi.IsDefined(typeof(JsonIgnoreAttribute))))
                .ToDictionary(pi => pi.Name, pi => Serialize(pi.GetValue(value)));
        }

        private static object SerializeCollection(IEnumerable value)
        {
            return value.Cast<object>().Select(Serialize).ToArray();
        }

        private static object SerializePrimitive(Type type, TypeInfo typeInfo, object instance)
        {
            if (type == typeof(DateTime))
            {
                return SerializeDateTime((DateTime) instance);
            }

            if (type == typeof(DateTimeOffset))
            {
                return SerializeDateTimeOffset((DateTimeOffset) instance);
            }

            if (type == typeof(string) || typeInfo.IsPrimitive || type == typeof(decimal))
            {
                return instance;
            }
            
            if (type == typeof(Guid))
            {
                return $"{instance}";
            }

            // last case scenario serialize it as JSON
            return JsonConvert.SerializeObject(instance);
        }

        private static string SerializeDateTime(DateTime dateTime)
        {
             return dateTime.ToString(DefaultDateTimeFormat, CultureInfo.CurrentCulture);
        }

        private static string SerializeDateTimeOffset(DateTimeOffset dateTime)
        {
            return dateTime.ToString(DefaultDateTimeFormat, CultureInfo.CurrentCulture);
        }


        private static object SerializeDictionary(Type type, object value)
        {
            var keyType = type.GetGenericArguments()[0];
            if (keyType != typeof(string))
            {
                throw new NotSupportedException(
                    $"Dictionary had keys with type '{keyType.Name}'. Only dictionaries with type '{nameof(String)}' are supported.");
            }

            var serialized = new Dictionary<string, object>();
            foreach (var item in (dynamic) value)
            {
                string key = item.Key;
                object entry = item.Value;

                serialized[key] = Serialize(entry);
            }

            return serialized;
        }
    }
}