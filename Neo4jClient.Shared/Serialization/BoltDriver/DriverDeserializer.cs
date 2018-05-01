using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Neo4j.Driver.V1;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;

namespace Neo4jClient.Serialization.BoltDriver
{
    /// <summary>
    /// Deserializes based on Neo4j Bolt Drivers
    /// </summary>
    /// <typeparam name="TResult">The type of the result object</typeparam>
    public class DriverDeserializer<TResult> : IDriverDeserializer<TResult>
    {
        private readonly CypherResultMode resultMode;

        public DriverDeserializer(CypherResultMode resultMode)
        {
            this.resultMode = resultMode;
        }

        public TResult Deserialize(IRecord record)
        {
            try
            {
                return InternalDeserialize(record);
            }
            catch (Exception ex)
            {
                const string messageTemplate =
                    @"Neo4j returned a valid response, however Neo4jClient was unable to deserialize into the object structure you supplied.

First, try and review the exception below to work out what broke.

If it's not obvious, you can ask for help at http://stackoverflow.com/questions/tagged/neo4jclient

Include the full text of this exception, including this message, the stack trace, and all of the inner exception details.

Include the full type definition of {0}.

Include this full record instance, with any sensitive values replaced with non-sensitive equivalents:

Keys: {1}

Corresponding value types: {2}

Corresponding values (serialized through ToString()): {3}";

                var message = string.Format(messageTemplate, typeof(TResult).FullName, string.Join(", ", record.Keys),
                    string.Join(", ", record.Keys.Select(k => record[k]?.GetType().FullName ?? "(NULL)")),
                        record.Keys.Select(k => record[k]?.ToString() ?? "(NULL)"));

                // If it's a specifc scenario that we're blowing up about, put this front and centre in the message
                if (ex is DeserializationException deserializationException)
                {
                    message = $"{deserializationException.Message}{Environment.NewLine}{Environment.NewLine}----{Environment.NewLine}{Environment.NewLine}{message}";
                }

                throw new ArgumentException(message, nameof(record), ex);
            }
        }

        private TResult InternalDeserialize(IRecord record)
        {
            return resultMode == CypherResultMode.Set
                ? DeserializeSetMode(record)
                : DeserializeProjectionMode(record);
        }

        private TResult DeserializeSetMode(IRecord record)
        {
            if (record.Keys.Count != 1)
            {
                throw new InvalidOperationException(
                    "The deserializer is running in single column mode, but the response included multiple columns which indicates a projection instead. If using the fluent Cypher interface, use the overload of Return that takes a lambda or object instead of single string. (The overload with a single string is for an identity, not raw query text: we can't map the columns back out if you just supply raw query text.)");
            }

            var resultType = typeof(TResult);

            var value = record[record.Keys[0]];

//            var rowAsArray = (JArray)row;
//            if (rowAsArray.Count != 1)
//                throw new InvalidOperationException(string.Format("Expected the row to only have a single array value, but it had {0}.", rowAsArray.Count));

//            var elementToParse = row[0];
//            if (elementToParse is JObject)
//            {
//                var propertyNames = ((JObject)elementToParse)
//                    .Properties()
//                    .Select(p => p.Name)
//                    .ToArray();
//                var dataElementLooksLikeANodeOrRelationshipInstance =
//                    new[] { "data", "self", "traverse", "properties" }.All(propertyNames.Contains);
//                if (!isResultTypeANodeOrRelationshipInstance &&
//                    dataElementLooksLikeANodeOrRelationshipInstance)
//                {
//                    elementToParse = elementToParse["data"];
//                }
//            }

            return (TResult) CoerceValue(resultType, value);
        }

        private TResult DeserializeProjectionMode(IRecord record)
        {
            var properties = typeof(TResult).GetProperties();
            var propertiesDictionary = properties
                .ToDictionary(p => p.Name);

            Func<IRecord, TResult> mutateRecord = null;

            var columnNames = record.Keys.ToArray();

            var columnsWhichDontHaveSettableProperties = columnNames
                .Where(c => !propertiesDictionary.ContainsKey(c) || !propertiesDictionary[c].CanWrite)
                .ToArray();
            if (columnsWhichDontHaveSettableProperties.Any())
            {
                // See if there is a constructor that is compatible with all property types,
                // which is the case for anonymous types...
                var ctor = typeof(TResult).GetConstructors().FirstOrDefault(info =>
                {
                    var parameters = info.GetParameters();
                    if (parameters.Length != record.Keys.Count)
                        return false;

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var property = propertiesDictionary[record.Keys[i]];
                        if (!parameters[i].ParameterType.IsAssignableFrom(property.PropertyType))
                            return false;
                    }
                    return true;
                });

                if (ctor != null)
                {
                    mutateRecord = rec =>
                        ReadProjectionRowUsingCtor(rec, propertiesDictionary, ctor);
                }

                if (mutateRecord == null)
                {
                    // wasn't able to build TResult via constructor
                    var columnsWhichDontHaveSettablePropertiesCommaSeparated = string.Join(", ", columnsWhichDontHaveSettableProperties);
                    throw new ArgumentException(string.Format(
                        "The query response contains columns {0} however {1} does not contain publically settable properties to receive this data.",
                        columnsWhichDontHaveSettablePropertiesCommaSeparated,
                        typeof(TResult).FullName),
                        nameof(record));
                }
            }
            else
            {
                mutateRecord = rec => ReadProjectionRowUsingProperties(rec, propertiesDictionary);
            }

            return mutateRecord(record);
        }

        TResult ReadProjectionRowUsingCtor(
            IRecord record,
            IDictionary<string, PropertyInfo> propertiesDictionary,
            ConstructorInfo ctor)
        {
            var coercedValues = record
                .Values
                .Select(entry =>
                {
                    var property = propertiesDictionary[entry.Key];
                    if (entry.Value == null)
                    {
                        return null;
                    }

                    var coercedValue = CoerceValue(property.PropertyType, entry.Value);
                    return coercedValue;
                })
                .ToArray();

            var result = (TResult)ctor.Invoke(coercedValues);

            return result;
        }

        TResult ReadProjectionRowUsingProperties(
            IRecord record,
            IDictionary<string, PropertyInfo> propertiesDictionary)
        {
            var result = Activator.CreateInstance<TResult>();

            foreach (var entry in record.Values)
            {
                var property = propertiesDictionary[entry.Key];

                SetPropertyValue(property, entry.Value, result);
            }

            return result;
        }

        private void SetPropertyValue(PropertyInfo property, object value, object instance)
        {
            if (value == null)
            {
                return;
            }

            var coercedValue = CoerceValue(property.PropertyType, value);
            property.SetValue(instance, coercedValue, null);
        }

        private object CoerceValue(Type valueType, object value)
        {
            if (value == null)
            {
                return null;
            }

            var typeInfo = valueType.GetTypeInfo();

            Type genericTypeDef = null;

            if (typeInfo.IsGenericType)
            {
                genericTypeDef = valueType.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(Nullable<>))
                {
                    valueType = valueType.GetGenericArguments()[0];
                    genericTypeDef = null;
                }
            }

            if (typeInfo.IsPrimitive)
            {
                return Convert.ChangeType(value, valueType);
            }
         
            if (typeInfo.IsEnum)
            {
                var converted = Enum.Parse(valueType, (string)value, false);
                return converted;
            }

            if (valueType == typeof(Uri))
            {
                var uri = new Uri((string)value, UriKind.RelativeOrAbsolute);
                return uri;
            }

            if (valueType == typeof(string))
            {
                return value.ToString();
            }

            if (valueType == typeof(DateTime))
            {
                if (value is string s)
                {
                    return CommonDeserializerMethods.ParseDateTime(s);
                }

                return null;
            }

            if (valueType == typeof(DateTimeOffset))
            {
                if (value is string s)
                {
                    return CommonDeserializerMethods.ParseDateTimeOffset(s);
                }

                return null;
            }

            if (valueType == typeof(Decimal))
            {
                var dec = Convert.ToDecimal(value);
                return dec;
            }

            if (valueType == typeof(TimeSpan))
            {
                var timeSpan = TimeSpan.Parse(value.ToString());
                return timeSpan;
            }

            if (valueType == typeof(Guid))
            {
                var raw = value.ToString();
                var guid = string.IsNullOrEmpty(raw) ? Guid.Empty : new Guid(raw);
                return guid;
            }

            if (typeInfo.IsValueType)
            {
                return Convert.ChangeType(value.ToString(), valueType);
            }

            if (valueType == typeof(byte[]))
            {
                return Convert.FromBase64String((string)value);
            }

            if (typeInfo.BaseType == typeof(Array))
            {
                var list = (ArrayList) BuildList(typeof(ArrayList), valueType.GetElementType(), (IEnumerable) value);
                return list.ToArray();
            }

            if (genericTypeDef == typeof(List<>) || genericTypeDef == typeof(IEnumerable<>))
            {
                var list = BuildList(valueType, GetListItemType(valueType), (IEnumerable) value);
                return list;
            }

            if (genericTypeDef == typeof(Dictionary<,>))
            {
                var keyType = valueType.GetGenericArguments()[0];

                // only supports Dict<string, T>()
                if (keyType != typeof(string))
                {
                    throw new NotSupportedException("Value coersion only supports dictionaries with a key of type System.String");
                }

                var dict = BuildDictionary(valueType, value);
                return dict;
            }

            if (valueType == typeof(PathsResultBolt))
            {
                if (value is IPath path)
                {
                    return new PathsResultBolt(path);
                }

                throw new DeserializationException(
                    $"A path must be returned by the IRecord entry in order to be deserialized into {typeof(PathsResultBolt).Name}");
            }

            if (valueType == typeof(IPath))
            {
                if (value is IPath path)
                {
                    return path;
                }

                throw new DeserializationException(
                    $"A path must be returned by the IRecord entry in order to be deserialized into {typeof(IPath).Name}");
            }

            if (valueType == typeof(INode))
            {
                if (value is INode node)
                {
                    return node;
                }

                throw new DeserializationException(
                    $"A node must be returned by the IRecord entry in order to be deserialized into {typeof(INode).Name}");
            }

            if (valueType == typeof(IRelationship))
            {
                if (value is IRelationship relationship)
                {
                    return relationship;
                }

                throw new DeserializationException(
                    $"A relationship must be returned by the IRecord entry in order to be deserialized into {typeof(IRelationship).Name}");
            }

            return CreateAndMap(valueType, value);
        }

        private Type GetListItemType(Type collectionType)
        {
            if (collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return collectionType.GetGenericArguments().Single();
            }

            var itemType = collectionType
                .GetInterfaces()
                .Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>))
                .Select(i => i.GetGenericArguments().First())
                .Single();

            return itemType;
        }

        private IList BuildList(Type collectionType, Type itemType, IEnumerable items)
        {
            if (items == null)
            {
                return null;
            }

            if (collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                collectionType = typeof(List<>).MakeGenericType(itemType);
            }

            var collection = (IList)Activator.CreateInstance(collectionType);

            foreach (var item in items)
            {
                collection.Add(CoerceValue(itemType, item));
            }

            return collection;
        }

        private IDictionary BuildDictionary(Type type, object value)
        {
            var dict = (IDictionary)Activator.CreateInstance(type);
            var valueType = type.GetGenericArguments()[1];

            foreach (var entry in GetDictionaryEntries(type, value))
            {
                dict.Add(entry.Key, CoerceValue(valueType, entry.Value));
            }

            return dict;
        }

        private IEnumerable<DictionaryEntry> CastDictionaryEntries(IDictionary dict)
        {
            foreach (var entry in dict)
            {
                yield return (DictionaryEntry) entry;
            }
        }

        private IEnumerable<PropertyEntry> GetDictionaryEntries(Type targetType, object value)
        {
            if (value is IDictionary dictionary)
            {
                return CastDictionaryEntries(dictionary).Select(e => new PropertyEntry((string) e.Key, e.Value));
            }

            if (value is INode node)
            {
                return node.Properties.Select(pair => new PropertyEntry(pair.Key, pair.Value));
            }

            if (value is IRelationship relationship)
            {
                return relationship.Properties.Select(pair => new PropertyEntry(pair.Key, pair.Value));
            }

            throw new DeserializationException(
                $"Type {value.GetType().FullName} returned by Neo4j cannot be deserialized into a {targetType.Name}. " +
                $"Only maps, nodes, and relationships can be deserialized into a {targetType.Name}.");
        }

        private object CreateAndMap(Type valueType, object value)
        {
            object instance;
            try
            {
                instance = Activator.CreateInstance(valueType);
            }
            catch (MissingMethodException ex)
            {
                throw new DeserializationException(
                    $"We expected a default public constructor on {valueType.Name} so that we could create instances of it to deserialize data into, however this constructor does not exist or is inaccessible.",
                    ex);
            }

            return Map(valueType, instance, value);
        }

        private object Map(Type targetType, object instance, object valueToBeMapped)
        {
            var entries = GetDictionaryEntries(targetType, valueToBeMapped)
                .ToDictionary(e => e.Key, e => e.Value);
            var props = GetPropertiesForType(targetType);

            foreach (var property in props)
            {
                var propertyName = property.Key;
                var propertyInfo = property.Value;

                if (!entries.ContainsKey(propertyName))
                {
                    throw new InvalidOperationException(
                        $"While trying to map some IRecord entry into an object of type {targetType.FullName}, " +
                        $"we failed to find an expected property ({propertyName}).");
                }

                SetPropertyValue(propertyInfo, entries[propertyName], instance);
            }

            return instance;
        }

        private Dictionary<string, PropertyInfo> GetPropertiesForType(Type targetType)
        {
            return targetType
                .GetProperties()
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name);
        }

        private class PropertyEntry
        {
            public string Key { get; }
            public object Value { get; }

            public PropertyEntry(string key, object value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
