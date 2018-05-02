using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace Neo4jClient.Serialization
{
    /// <summary>
    /// Base class for deserializers
    /// </summary>
    public abstract class BaseDeserializer<TResult, TSerialized, TRecord, TField>
    {
        private readonly IGraphClient client;
        private readonly CypherResultMode resultMode;

        protected BaseDeserializer(IGraphClient client, CypherResultMode resultMode)
        {
            this.client = client;
            this.resultMode = resultMode;
        }

        public IEnumerable<TResult> Deserialize(TSerialized results)
        {
            try
            {
                return InternalDeserialize(results);
            }
            catch (Exception ex)
            {
                const string messageTemplate =
                    @"Neo4j returned a valid response, however Neo4jClient was unable to deserialize into the object structure you supplied.

First, try and review the exception below to work out what broke.

If it's not obvious, you can ask for help at http://stackoverflow.com/questions/tagged/neo4jclient

Include the full text of this exception, including this message, the stack trace, and all of the inner exception details.

Include the full type definition of {0}.

{1}";
                var message = string.Format(messageTemplate, typeof(TResult).FullName,
                    GenerateExceptionDetails(ex, results));

                // If it's a specifc scenario that we're blowing up about, put this front and centre in the message
                if (ex is DeserializationException deserializationException)
                {
                    message =
                        $"{deserializationException.Message}{Environment.NewLine}{Environment.NewLine}----{Environment.NewLine}{Environment.NewLine}{message}";
                }

                throw new ArgumentException(message, nameof(results), ex);
            }
        }

        #region Abstract Methods
        protected abstract string GenerateExceptionDetails(Exception exception, TSerialized results);
        protected abstract string[] GetColumnNames(TSerialized results);
        protected abstract IEnumerable<TRecord> GetRecordsFromResults(TSerialized results);
        protected abstract TField GetElementForDeserializationInSingleColumn(TRecord record);
        protected abstract void RegisterRecordBeingDeserialized(TRecord record);
        protected abstract IEnumerable<FieldEntry> GetFieldEntries(TRecord record);
        protected abstract IEnumerable<FieldEntry> CastIntoDictionaryEntries(TField field);
        protected abstract IEnumerable<TField> CastIntoEnumerable(TField field);
        protected abstract object CastIntoPrimitiveType(Type primitiveType, TField field);
        protected abstract bool TryCastIntoDateTime(TField field, out DateTime? dt);
        protected abstract bool TryCastIntoDateTimeOffset(TField field, out DateTimeOffset? dt);
        #endregion

        #region Overridable Methods

        protected virtual DeserializationContext GenerateContext(TSerialized results, CypherResultMode resultMode)
        {
            return new DeserializationContext
            {
                Culture = CultureInfo.InvariantCulture,
                JsonConverters = Enumerable.Reverse(client.JsonConverters ?? new List<JsonConverter>(0)).ToArray(),
                JsonContractResolver = client.JsonContractResolver
            };
        }

        protected virtual TypeMapping GetTypeMapping(DeserializationContext context, Type type, int nestingLevel)
        {
            return context.TypeMappings.FirstOrDefault(m => m.ShouldTriggerForPropertyType(nestingLevel, type));
        }

        protected virtual bool IsNull(TField field)
        {
            return field == null;
        }

        protected virtual object GetValueFromField(TField field)
        {
            return field;
        }

        protected virtual string GetStringFromField(TField field)
        {
            return field.ToString();
        }

        protected virtual bool TryDeserializeCustomType(Type propertyType, TField field, out object deserialized)
        {
            deserialized = null;
            return false;
        }
        #endregion

        private IEnumerable<TResult> InternalDeserialize(TSerialized results)
        {
            switch (resultMode)
            {
                case CypherResultMode.Set:
                    return DeserializeInSingleColumnMode(GenerateContext(results, resultMode), results);
                case CypherResultMode.Projection:
                    return DeserializeInProjectionMode(GenerateContext(results, resultMode), results);
                default:
                    throw new NotSupportedException($"Unrecognised result mode of {resultMode}.");
            }
        }

        protected IEnumerable<TResult> DeserializeInSingleColumnMode(DeserializationContext context, TSerialized results)
        {
            var columns = GetColumnNames(results);
            if (columns.Length != 1)
            {
                throw new InvalidOperationException(
                    "The deserializer is running in single column mode, but the response included multiple columns which indicates a projection instead. If using the fluent Cypher interface, use the overload of Return that takes a lambda or object instead of single string. (The overload with a single string is for an identity, not raw query text: we can't map the columns back out if you just supply raw query text.)");
            }

            var mapping = GetTypeMapping(context, typeof(TResult), 0);
            var resultType = mapping == null ? typeof(TResult) : mapping.DetermineTypeToParseJsonIntoBasedOnPropertyType(typeof(TResult));
            var records = GetRecordsFromResults(results);
            var mutationMapping = mapping?.MutationCallback;

            return records.Select(record =>
            {
                RegisterRecordBeingDeserialized(record);

                var elementForDeserialization = GetElementForDeserializationInSingleColumn(record);
                var coercedValue = CreateAndMap(context, resultType, elementForDeserialization, 0);
                return (TResult) (mutationMapping == null ? coercedValue : mutationMapping(coercedValue));
            });
        }

        protected IEnumerable<TResult> DeserializeInProjectionMode(DeserializationContext context, TSerialized results)
        {
            var properties = typeof(TResult).GetProperties();
            var propertiesDictionary = properties
                .ToDictionary(p => p.Name);

            Func<TRecord, TResult> mutateRecord = null;

            var columnNames = GetColumnNames(results);

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
                    if (parameters.Length != columnNames.Length)
                        return false;

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var property = propertiesDictionary[columnNames[i]];
                        if (!parameters[i].ParameterType.IsAssignableFrom(property.PropertyType))
                            return false;
                    }
                    return true;
                });

                if (ctor != null)
                {
                    mutateRecord = rec =>
                        ReadProjectionRowUsingCtor(context, rec, propertiesDictionary, ctor);
                }

                if (mutateRecord == null)
                {
                    // wasn't able to build TResult via constructor
                    var columnsWhichDontHaveSettablePropertiesCommaSeparated = string.Join(", ", columnsWhichDontHaveSettableProperties);
                    throw new ArgumentException(string.Format(
                        "The query response contains columns {0} however {1} does not contain publically settable properties to receive this data.",
                        columnsWhichDontHaveSettablePropertiesCommaSeparated,
                        typeof(TResult).FullName),
                        nameof(results));
                }
            }
            else
            {
                mutateRecord = rec => ReadProjectionRowUsingProperties(context, rec, propertiesDictionary);
            }

            var records = GetRecordsFromResults(results);
            return records.Select(record =>
            {
                RegisterRecordBeingDeserialized(record);

                return mutateRecord(record);
            });
        }

        TResult ReadProjectionRowUsingCtor(
            DeserializationContext context,
            TRecord record,
            IDictionary<string, PropertyInfo> propertiesDictionary,
            ConstructorInfo ctor)
        {
            var coercedValues = GetFieldEntries(record)
                .Select(entry =>
                {
                    var property = propertiesDictionary[entry.Key];
                    if (IsNull(entry.Value))
                    {
                        return null;
                    }

                    var coercedValue = CoerceValue(context, property.PropertyType, entry.Value, 0);
                    return coercedValue;
                })
                .ToArray();

            var result = (TResult)ctor.Invoke(coercedValues);

            return result;
        }

        TResult ReadProjectionRowUsingProperties(
            DeserializationContext context,
            TRecord record,
            IDictionary<string, PropertyInfo> propertiesDictionary)
        {
            var result = Activator.CreateInstance<TResult>();

            foreach (var entry in GetFieldEntries(record))
            {
                var property = propertiesDictionary[entry.Key];

                SetPropertyValue(context, property, entry.Value, result, 0);
            }

            return result;
        }

        private void SetPropertyValue(DeserializationContext context, PropertyInfo property, TField value, object instance, int nestingLevel)
        {
            if (value == null)
            {
                return;
            }

            var coercedValue = CoerceValue(context, property.PropertyType, value, nestingLevel);
            property.SetValue(instance, coercedValue, null);
        }

        protected class FieldEntry
        {
            public string Key { get; }
            public TField Value { get; }

            public FieldEntry(string key, TField value)
            {
                Key = key;
                Value = value;
            }
        }

        private object CreateAndMap(DeserializationContext context, Type valueType, TField field, int nestingLevel)
        {
            if (IsNull(field))
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
                return CastIntoPrimitiveType(valueType, field);
            }

            if (typeInfo.IsEnum)
            {
                var converted = Enum.Parse(valueType, GetStringFromField(field), false);
                return converted;
            }

            if (valueType == typeof(Uri))
            {
                var uri = new Uri(GetStringFromField(field), UriKind.RelativeOrAbsolute);
                return uri;
            }

            if (valueType == typeof(string))
            {
                return GetStringFromField(field);
            }

            if (valueType == typeof(DateTime))
            {
                if (TryCastIntoDateTime(field, out DateTime? dt))
                {
                    return dt;
                }

                return null;
            }

            if (valueType == typeof(DateTimeOffset))
            {
                if(TryCastIntoDateTimeOffset(field, out DateTimeOffset? dt))
                {
                    return dt;
                }

                return null;
            }

            if (valueType == typeof(Decimal))
            {
                var dec = Convert.ToDecimal(GetValueFromField(field));
                return dec;
            }

            if (valueType == typeof(TimeSpan))
            {
                var timeSpan = TimeSpan.Parse(GetStringFromField(field));
                return timeSpan;
            }

            if (valueType == typeof(Guid))
            {
                var raw = GetStringFromField(field);
                var guid = string.IsNullOrEmpty(raw) ? Guid.Empty : new Guid(raw);
                return guid;
            }

            if (typeInfo.IsValueType)
            {
                return Convert.ChangeType(GetStringFromField(field), valueType);
            }

            if (valueType == typeof(byte[]))
            {
                return Convert.FromBase64String(GetStringFromField(field));
            }

            if (typeInfo.BaseType == typeof(Array))
            {
                var value = CastIntoEnumerable(field);
                var list = (ArrayList)BuildList(context, typeof(ArrayList), valueType.GetElementType(), value, nestingLevel + 1);
                return list.ToArray();
            }

            if (genericTypeDef == typeof(List<>) || genericTypeDef == typeof(IEnumerable<>))
            {
                var value = CastIntoEnumerable(field);
                var list = BuildList(context, valueType, GetListItemType(valueType), value, nestingLevel + 1);
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

                var dict = BuildDictionary(context, valueType, field, nestingLevel + 1);
                return dict;
            }

            if (TryDeserializeCustomType(valueType, field, out object customDeserialized))
            {
                return customDeserialized;
            }

            return CreateObject(context, valueType, field, nestingLevel + 1);
        }

        private object CoerceValue(DeserializationContext context, Type valueType, TField field, int nestingLevel)
        {
            var mapping = GetTypeMapping(context, valueType, nestingLevel);
            var item = mapping != null
                ? MutateObject(context, mapping, valueType, field, nestingLevel)
                : CreateAndMap(context, valueType, field, nestingLevel);
            return item;
        }

        private object MutateObject(DeserializationContext context, TypeMapping mapping, Type propertyType,
            TField field, int nestingLevel)
        {
            var newType = mapping.DetermineTypeToParseJsonIntoBasedOnPropertyType(propertyType);
            var rawItem = CreateAndMap(context, newType, field, nestingLevel + 1);
            var item = mapping.MutationCallback(rawItem);
            return item;
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

        private IList BuildList(DeserializationContext context, Type collectionType, Type itemType, IEnumerable<TField> items, int nestingLevel)
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
                collection.Add(CoerceValue(context, itemType, item, nestingLevel));
            }

            return collection;
        }

        private IDictionary BuildDictionary(DeserializationContext context, Type type, TField field, int nestingLevel)
        {
            var dict = (IDictionary)Activator.CreateInstance(type);
            var valueType = type.GetGenericArguments()[1];

            foreach (var entry in CastIntoDictionaryEntries(field))
            {
                dict.Add(entry.Key, CoerceValue(context, valueType, entry.Value, nestingLevel));
            }

            return dict;
        }

        private object CreateObject(DeserializationContext context, Type valueType, TField field, int nestingLevel)
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

            return Map(context, valueType, instance, field, nestingLevel);
        }

        private object Map(DeserializationContext context, Type targetType, object instance, TField field, int nestingLevel)
        {
            var entries = CastIntoDictionaryEntries(field)
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

                SetPropertyValue(context, propertyInfo, entries[propertyName], instance, nestingLevel);
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

    }
}
