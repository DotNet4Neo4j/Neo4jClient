using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Neo4j.Driver.V1;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Serialization;
using Newtonsoft.Json;

namespace Neo4jClient
{
    internal static class StatementResultHelper
    {
        internal static JsonSerializerSettings JsonSettings { get; set; } =
            new JsonSerializerSettings
            {
                Converters = BoltGraphClient.DefaultJsonConverters.Reverse().ToList()
            };

        internal static string ToJsonString(this INode node, bool inSet = false, bool isNested = false, bool isNestedInList = false)
        {
            var props = node
                .Properties
                .Select(p => $"\"{p.Key}\":{JsonConvert.SerializeObject(p.Value, JsonSettings)}");

            if (isNestedInList)
            {
                inSet = false;
                isNested = false;
            }

            if (isNested)
                return $"{{\"data\":{{ {string.Join(",", props)} }}}}";

            if (inSet)
                return $"{string.Join(",", props)}";

            return $"{{ {string.Join(",", props)} }}";
        }

        internal static string ToJsonString(this IRelationship relationship, bool inSet = false, bool isNested = false, bool isNestedInList = false)
        {
            var props = relationship
                .Properties
                .Select(p => $"\"{p.Key}\":{JsonConvert.SerializeObject(p.Value, JsonSettings)}");

            if (isNestedInList)
            {
                inSet = true;
                isNested = false;
            }

            if (isNested)
                return $"{{\"data\":{{ {string.Join(",", props)} }}}}";

            if (inSet)
                return $"{{\"data\": {string.Join(",", props)} }}";

            return $"{{\"data\":{{ {string.Join(",", props)} }}}}";
        }

        internal static string ToJsonString(this object o, bool inSet, bool isNested, bool isNestedInList, bool isClass = false)
        {
            if (o == null)
                return null;

            if (o is INode)
                return ((INode) o).ToJsonString(inSet, true, isNestedInList);

            if (o is IRelationship)
                return ((IRelationship) o).ToJsonString(inSet, true, isNestedInList);

            var oType = o.GetType();

            if (isNested)
            {
                if (isClass)
                {
                    if (o is IDictionary || oType == typeof(IDictionary<string, object>) || oType == typeof(Dictionary<string, object>))
                    {
                        var dict = (IDictionary<string, object>)o;
                        var output = new List<string>();
                        foreach (var keyValuePair in dict)
                        {
                            var s = $"\"{keyValuePair.Key}\":{keyValuePair.ToJsonString(inSet, true, false) ?? "null"}";
                            output.Add(s);
                        }

                        return $"{{\"data\":{{ {string.Join(",", output)} }}}}";
                    }
                }

                if (o is IDictionary || oType == typeof(IDictionary<string, object>) || oType == typeof(Dictionary<string, object>))
                {
                    var dict = (IDictionary<string, object>) o;
                    var output = new List<string>();
                    foreach (var keyValuePair in dict)
                    {
                        var s = $"\"{keyValuePair.Key}\":{keyValuePair.ToJsonString(inSet, true, false) ?? "null"}";
                        output.Add(s);
                    }

                    return string.Join(",", output);
                }

                if (o is KeyValuePair<string, object>)
                {
                    var kvp = (KeyValuePair<string, object>) o;
                    var kvpType = kvp.Value?.GetType();
                    if (kvp.Value is IDictionary || kvpType == typeof(IDictionary<string, object>) || kvpType == typeof(Dictionary<string, object>))
                        return $"{{{kvp.Value.ToJsonString(inSet, true, false) ?? "null"}}}";
                    return $"{kvp.Value.ToJsonString(inSet, true, false) ?? "null"}";
                }

                if (o.GetType().IsList())
                {
                    var output = new List<string>();
                    foreach (var e in (IEnumerable) o)
                        if (IsPrimitive(e.GetType()) || e is INode)
                            output.Add($"{e.ToJsonString(true, true, true) ?? "null"}");
                        else
                            output.Add($"{{{e.ToJsonString(true, true, true) ?? "null"}}}");
                    return $"[{string.Join(",", output)}]";
                }

                return JsonConvert.SerializeObject(o);
            }

            if (o is IDictionary || oType == typeof(IDictionary<string, object>) || oType == typeof(Dictionary<string, object>))
            {
                var dict = (IDictionary<string, object>) o;
                var output = new List<string>();
                foreach (var keyValuePair in dict)
                {
                    var s = $"\"{keyValuePair.Key}\":{keyValuePair.ToJsonString(inSet, true, false) ?? "null"}";
                    output.Add(s);
                }

                return $"{{ {string.Join(",", output)} }}";
            }


            if (oType.IsList())
            {
                var output = new List<string>();
                var wasNested = false;
                var onlyKvp = true;
                foreach (var e in (IEnumerable) o)
                {
                    var eType = e.GetType();
                    if (IsPrimitive(eType))
                    {
                        output.Add($"{e.ToJsonString(inSet, false, true) ?? "null"}");
                        onlyKvp = false;
                    }
                    else if (eType == typeof(KeyValuePair<string, object>))
                    {
                        var kvp = (KeyValuePair<string, object>) e;
                        output.Add($"\"{kvp.Key}\":{e.ToJsonString(inSet, true, true)}");
                        wasNested = true;
                    }
                    else if (eType == typeof(Dictionary<string, object>))
                    {
                        output.Add($"{{{e.ToJsonString(inSet, false, true) ?? "null"}}}");
                        onlyKvp = false;
                    }
                    else
                    {
                        output.Add($"{{\"data\":{e.ToJsonString(inSet, false, true) ?? "null"}}}");
                        onlyKvp = false;
                    }
                }
                if (onlyKvp)
                    return $"{string.Join(",", output)}";
                if (wasNested)
                    return $"[{{{string.Join(",", output)}}}]";
                return $"[{string.Join(",", output)}]";
            }
            return JsonConvert.SerializeObject(o);
        }

        private static string GetColumns(IEnumerable<string> keys)
        {
            return $"\"columns\":[{string.Join(",", keys.Select(k => $"\"{k}\""))}]";
        }

        public static IEnumerable<T> Deserialize<T>(this IRecord record, ICypherJsonDeserializer<T> deserializer, CypherResultMode mode)
        {
            var convertMode = mode;
            var typeT = typeof(T);
            if (!typeT.IsPrimitive() && typeT.GetInterfaces().Contains(typeof(IEnumerable)))
                convertMode = CypherResultMode.Projection;

            if (!typeT.IsAnonymous())
                convertMode = CypherResultMode.Projection;

            var columns = GetColumns(record.Keys);
            //Columns //Data
            var data = new List<string>();
            foreach (var key in record.Keys)
            {
                var o = record[key];
                if (o == null)
                {
                    data.Add(null);
                    continue;
                }

                var property = typeT.GetProperties().FirstOrDefault(p => p.Name == key);
                var isClass = property != null && !property.PropertyType.IsAnonymous() && !property.PropertyType.IsPrimitive() && !property.PropertyType.IsArray;

                data.Add(o.ToJsonString(convertMode == CypherResultMode.Set, record.Keys.Count > 1, false, isClass));
            }

            var format = "{{ {0}, \"data\":[[ {1} ]] }}";
            var dataJoined = string.Join(",", data.Select(d => d ?? "null"));

            string json;

            switch (convertMode)
            {
                case CypherResultMode.Set:
                    if (typeT.IsPrimitive())
                        json = string.Format(format, columns, $"{dataJoined}");
                    else
                        json = string.Format(format, columns, $"{{\"data\":{{ {dataJoined} }} }}");
                    break;
                case CypherResultMode.Projection:
                    json = string.Format(format, columns, $"{dataJoined}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            return deserializer.Deserialize(json);
        }

        public static T Parse<T>(this IRecord record, IGraphClient graphClient)
        {
            if (record.Keys.Count != 1)
                return ParseMultipleKeys<T>(record, graphClient);

            var identifier = record.Keys.Single();
            return record.Parse<T>(identifier, graphClient);
        }

        private static T ParseMultipleKeys<T>(IRecord record, IGraphClient graphClient)
        {
            var t = ConstructNew<T>();
            //Collection or Node --- anything else???
            foreach (var property in typeof(T).GetTypeInfo().DeclaredProperties)
            {
                if (!record.Keys.Contains(property.Name))
                    break;

                var method = GetParsed(property.PropertyType);
                var response = method.Invoke(null, new object[] {new Neo4jClientRecord(record, property.Name), graphClient});
                property.SetValue(t, response);
            }
            return t;
        }

        private static T Parse<T>(this IRecord record, string identifier, IGraphClient graphClient)
        {
            var typeT = typeof(T);
            if (typeT.IsPrimitive())
                return record.ParsePrimitive<T>(identifier);

            if (typeT.GetTypeInfo().ImplementedInterfaces.Any(x => x.Name == nameof(IEnumerable)) && typeT.Name != nameof(ExpandoObject))
                return record.ParseCollection<T>(identifier, graphClient);

            var converters = graphClient.JsonConverters;
            converters.Reverse();
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = converters,
                ContractResolver = graphClient.JsonContractResolver
            };

            foreach (var jsonConverter in converters)
                if (jsonConverter.CanConvert(typeof(T)))
                    return JsonConvert.DeserializeObject<T>(record[identifier].As<INode>().ToJsonString(), serializerSettings);


            var t = ConstructNew<T>();
            var obj = record[identifier];
            if (obj is INode node)
                foreach (var property in t.GetType().GetProperties())
                    if (node.Properties.ContainsKey(property.Name))
                        if (property.PropertyType.IsPrimitive())
                        {
                            property.SetValue(t, Convert.ChangeType(node.Properties[property.Name], property.PropertyType));
                        }
                        else if (property.PropertyType.GetTypeInfo().ImplementedInterfaces.Any(i => i == typeof(IEnumerable)))
                        {
                            var parsed = GetParsed(property.PropertyType);
                            var enumRecord = new Neo4jClientRecord(node.Properties[property.Name], "Enumerable");
                            var list = parsed.Invoke(null, enumRecord.AsParameters(graphClient));

                            property.SetValue(t, list);
                        }
                        else
                        {
                            var res = JsonConvert.DeserializeObject(
                                $"\"{node.Properties[property.Name].As<string>()}\"",
                                property.PropertyType,
                                serializerSettings);
                            property.SetValue(t, res);
                        }
            return t;
        }

        private static T ParseCollection<T>(this IRecord record, string identifier, IGraphClient graphClient)
        {
            var typeT = typeof(T).GetTypeInfo();
            if (!typeT.IsGenericType && !typeT.IsArray)
                throw new InvalidOperationException($"Don't know how to handle {typeof(T).FullName}");

            if (typeT.IsArray)
                return record.ParseArray<T>(identifier, graphClient);

            var genericArgs = typeT.GenericTypeArguments;
            if (genericArgs.Length > 1)
                throw new InvalidOperationException($"Don't know how to handle {typeof(T).FullName}");

            var listType = typeof(List<>).MakeGenericType(genericArgs.Single());
            var list = Activator.CreateInstance(listType);

            foreach (var item in (IEnumerable) record[identifier])
            {
                var internalRecord = new Neo4jClientRecord(item, identifier);
                var method = GetParsed(genericArgs.Single());
                var parsed = method.Invoke(null, internalRecord.AsParameters(graphClient));
                listType.GetMethod("Add")?.Invoke(list, new[] {parsed});
            }

            return (T) list;
        }

        private static T ParseArray<T>(this IRecord record, string identifier, IGraphClient graphClient)
        {
            var typeT = typeof(T).GetTypeInfo();
            if (!typeT.IsArray)
                throw new InvalidOperationException($"Don't know how to handle {typeof(T).FullName}");

            var arrayElementType = typeT.GetElementType();
            var listType = typeof(List<>).MakeGenericType(arrayElementType);

            var method = typeof(StatementResultHelper).GetMethod(nameof(ParseCollection), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                ?.MakeGenericMethod(listType);
            dynamic listVersion = method?.Invoke(null, new object[] {record, identifier, graphClient});

            return listVersion?.ToArray();
        }

        private static MethodInfo GetParsed(Type genericParameter)
        {
            return typeof(StatementResultHelper).GetMethod(nameof(Parse))?.MakeGenericMethod(genericParameter);
        }

        public static string ParseAnonymous(this IRecord record, IGraphClient graphClient, bool onlyReturnData = false)
        {
            return JsonConvert.SerializeObject(ParseAnonymousAsDynamic(record, graphClient, onlyReturnData));
        }

        private static dynamic ParsePathResponse(IPath path)
        {
            var output = new PathsResultBolt(path);
            return output;
        }

        private static dynamic ParseAnonymousAsDynamic(this IRecord record, IGraphClient graphClient, bool onlyReturnData)
        {
            var data = new List<dynamic>();

            var inner = new List<dynamic>();
            foreach (var identifier in record.Keys)
            {
                dynamic expando = new ExpandoObject();
                var t = (IDictionary<string, object>) expando;
                var obj = record[identifier];
                if (obj == null)
                {
                    inner.Add(null);
                }
                else if (obj is IPath path)
                {
                    inner.Add(ParsePathResponse(path));
                }
                else if (obj is INode node)
                {
                    foreach (var property in node.Properties)
                    {
                        t[property.Key] = ParseElementInCollectionForAnonymous(graphClient, property.Value, identifier);
                    }

                    inner.Add(new Dictionary<string, dynamic> {{"data", expando}});
                }
                else if (obj is IRelationship relationship)
                {
                    foreach (var property in relationship.Properties)
                    {
                        t[property.Key] = ParseElementInCollectionForAnonymous(graphClient, property.Value, identifier);
                    }

                    inner.Add(new Dictionary<string, dynamic> {{"data", expando}});
                }
                else if (obj is IEnumerable && !(obj is string))
                {
                    var listObj = ((IEnumerable) obj).Cast<object>().ToList();

                    var first = listObj.FirstOrDefault();
                    if (first is KeyValuePair<string, object>)
                    {
                        var newNode = new Neo4jClientNode(listObj.Cast<KeyValuePair<string, object>>());
                        dynamic expando2 = new ExpandoObject();
                        var t2 = (IDictionary<string, object>) expando2;
                        foreach (var property in newNode.Properties)
                        {
                            var parsedValue = 
                                ParseElementInCollectionForAnonymous(graphClient, property.Value, identifier);
                            t2[property.Key] = parsedValue;
                        }

                        inner.Add(expando2);
                    }
                    else
                    {
                        var parsedItems = new List<dynamic>();
                        foreach (var o in listObj)
                        {
                            parsedItems.Add(ParseElementInCollectionForAnonymous(graphClient, o, identifier));
                        }

                        inner.Add(parsedItems);
                    }
                }
                else
                {
                    var method = typeof(StatementResultHelper).GetMethod(nameof(Parse), BindingFlags.Static | BindingFlags.NonPublic);
                    var generic = method?.MakeGenericMethod(obj.GetType());
                    var res = generic?.Invoke(null, new object[] {record, identifier, graphClient});
                    if(res == null)
                        throw new JsonSerializationException($"Unable to serialize {obj.GetType().FullName} correctly. This is likely an error and should be reported to Neo4jClient's github page.");

                    inner.Add(res);
                }
            }

            data.Add(inner);

            if (onlyReturnData)
                return inner;

            //TODO: Ugh! this is about as hacky as it can get
            dynamic output = new
            {
                columns = new List<string>(record.Keys),
                data
            };

            return output;
        }

        private static dynamic ParseElementInCollectionForAnonymous(IGraphClient graphClient, dynamic item, string identifier)
        {
            var newRecord = new Neo4jClientRecord(item, identifier);
            // item o gets parsed and returned always as a list when onlyReturnData = true
            var p2 = (List<dynamic>)ParseAnonymousAsDynamic(newRecord, graphClient, true);
            // but we only need the first item
            return p2.First();
        }

        public static bool IsAnonymous(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var hasCompilerGenerated = type.GetTypeInfo().GetCustomAttribute<CompilerGeneratedAttribute>() != null;

            // HACK: The only way to detect anonymous types right now.
            return hasCompilerGenerated
                   && type.GetTypeInfo().IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (type.GetTypeInfo().Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        private static T ConstructNew<T>()
        {
            try
            {
                return (T) typeof(T).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 0).Invoke(new object[] { });
            }
            catch (NullReferenceException e)
            {
                throw new InvalidCastException($"Unable to create an instance of {typeof(T).Name} without a parameterless constructor.", e);
            }
        }

        private static bool IsList(this Type type)
        {
            return !type.IsPrimitive() && type.GetInterfaces().Contains(typeof(IEnumerable));
        }

        private static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive || type == typeof(string) || type == typeof(decimal);
        }

        private static T ParsePrimitive<T>(this IRecord record, string identifier)
        {
            return (T) record[identifier];
        }

        private class Neo4jClientNode : INode
        {
            private readonly IDictionary<string, object> properties = new Dictionary<string, object>();

            public Neo4jClientNode(IEnumerable<KeyValuePair<string, object>> properties)
            {
                foreach (var p in properties) this.properties.Add(p.Key, p.Value);
            }

            public object this[string key] => properties[key];

            public IReadOnlyDictionary<string, object> Properties => new ReadOnlyDictionary<string, object>(properties);
            public long Id => int.MinValue;


            public IReadOnlyList<string> Labels => new List<string>();

            #region Equality members

            protected bool Equals(Neo4jClientNode other)
            {
                return Equals(properties, other.properties);
            }

            public bool Equals(INode other)
            {
                return Equals((object) other);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Neo4jClientNode) obj);
            }

            public override int GetHashCode()
            {
                return properties != null ? properties.GetHashCode() : 0;
            }

            #endregion
        }

        private class Neo4jClientRecord : IRecord
        {
            public Neo4jClientRecord(object obj, string identifier)
            {
                Values = new Dictionary<string, object> {{identifier, obj}};
            }

            public Neo4jClientRecord(IRecord record, string identifier)
            {
                Values = record == null 
                    ? new Dictionary<string, object> { { identifier, null } } 
                    : new Dictionary<string, object> {{identifier, record[identifier]}};
            }

            [Obsolete("This should not be called internally.")]
            object IRecord.this[int index] => throw new NotImplementedException("This should not be called.");

            object IRecord.this[string key] => Values[key];

            public IReadOnlyDictionary<string, object> Values { get; }
            public IReadOnlyList<string> Keys => Values.Keys.ToList();

            public object[] AsParameters(IGraphClient graphClient)
            {
                return new object[] {this, graphClient};
            }
        }
    }
}