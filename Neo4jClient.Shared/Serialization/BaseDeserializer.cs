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
    public abstract class BaseDeserializer<TResult, TSerialized, TRecordCollection, TRecord, TField>
    {
        private readonly IGraphClient client;

        protected BaseDeserializer(IGraphClient client, CypherResultMode resultMode)
        {
            this.client = client;
            ResultMode = resultMode;
        }

        protected CypherResultMode ResultMode { get; }

        public IEnumerable<TResult> Deserialize(TSerialized results)
        {
            try
            {
                var records = DeserializeIntoRecordCollections(results);
                var context = GenerateContext(records, ResultMode);
                return Deserialize(records, context);
            }
            catch (Exception ex)
            {
                // we want the NeoException to be thrown
                if (ex is NeoException)
                {
                    throw;
                }

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

        protected abstract TRecordCollection DeserializeIntoRecordCollections(TSerialized results);
        protected abstract string GenerateExceptionDetails(Exception exception, TSerialized results);
        protected abstract string[] GetColumnNames(TRecordCollection results);
        protected abstract IEnumerable<TRecord> GetRecordsFromResults(TRecordCollection results);
        protected abstract TField GetElementForDeserializationInSingleColumn(TRecord record);
        protected abstract void RegisterRecordBeingDeserialized(TRecord record);
        protected abstract IEnumerable<FieldEntry> GetFieldEntries(string[] columnNames, TRecord record);
        protected abstract IEnumerable<FieldEntry> CastIntoDictionaryEntries(Dictionary<string, PropertyInfo> props, TField field);
        protected abstract IEnumerable<TField> CastIntoEnumerable(TField field);
        protected abstract object CastIntoPrimitiveType(Type primitiveType, TField field);
        protected abstract bool TryCastIntoDateTime(TField field, out DateTime? dt);
        protected abstract bool TryCastIntoDateTimeOffset(TField field, out DateTimeOffset? dt);
        protected abstract Dictionary<string, PropertyInfo> GetPropertiesForType(DeserializationContext context,
            Type targetType);
        #endregion

        #region Overridable Methods

        protected virtual DeserializationContext GenerateContext(TRecordCollection results, CypherResultMode resultMode)
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

        protected virtual bool IsNull(PropertyInfo propInfo, TField field)
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

        protected virtual bool TryDeserializeCustomType(DeserializationContext context, Type propertyType, TField field, 
            out object deserialized)
        {
            deserialized = null;
            return false;
        }
        #endregion

        protected IEnumerable<TResult> Deserialize(TRecordCollection results, DeserializationContext context)
        {
            switch (ResultMode)
            {
                case CypherResultMode.Set:
                    return DeserializeInSingleColumnMode(context, results);
                case CypherResultMode.Projection:
                    return DeserializeInProjectionMode(context, results);
                default:
                    throw new NotSupportedException($"Unrecognised result mode of {ResultMode}.");
            }
        }

        public TResult DeserializeObject(DeserializationContext context, TField instance)
        {
            return (TResult)CoerceValue(context, typeof(TResult), instance, 0);
        }

        protected IEnumerable<TResult> DeserializeInSingleColumnMode(DeserializationContext context, TRecordCollection results)
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

            var result = records.Select(record =>
            {
                RegisterRecordBeingDeserialized(record);

                var elementForDeserialization = GetElementForDeserializationInSingleColumn(record);
                var coercedValue = CoerceValue(context, resultType, elementForDeserialization, 0, false);
                return (TResult) (mutationMapping == null ? coercedValue : mutationMapping(coercedValue));
            });

            return result;
        }

        protected IEnumerable<TResult> DeserializeInProjectionMode(DeserializationContext context, TRecordCollection results)
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
                        ReadProjectionRowUsingCtor(context, rec, columnNames, propertiesDictionary, ctor);
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
                mutateRecord = rec => ReadProjectionRowUsingProperties(context, rec, columnNames, propertiesDictionary);
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
            string[] columnNames,
            IDictionary<string, PropertyInfo> propertiesDictionary,
            ConstructorInfo ctor)
        {
            var coercedValues = GetFieldEntries(columnNames, record)
                .Select(entry =>
                {
                    var property = propertiesDictionary[entry.Key];
                    if (IsNull(property, entry.Value))
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
            string[] columnNames,
            IDictionary<string, PropertyInfo> propertiesDictionary)
        {
            var result = Activator.CreateInstance<TResult>();

            foreach (var entry in GetFieldEntries(columnNames, record))
            {
                var property = propertiesDictionary[entry.Key];
                if (IsNull(property, entry.Value))
                {
                    continue;
                }

                SetPropertyValue(context, property, entry.Value, result, 0);
            }

            return result;
        }

        private void SetPropertyValue(DeserializationContext context, PropertyInfo property, TField value, object instance, int nestingLevel)
        {
            if (IsNull(null, value))
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

        private object CoerceValue(DeserializationContext context, Type valueType, TField field, int nestingLevel, bool useTypeMappings = true)
        {
            if (IsNull(null, field))
            {
                return null;
            }

            if (TryDeserializeCustomType(context, valueType, field, out object customDeserialized))
            {
                return customDeserialized;
            }

            var typeInfo = valueType.GetTypeInfo();

            Type genericTypeDef = null;

            if (typeInfo.IsGenericType)
            {
                genericTypeDef = valueType.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(Nullable<>))
                {
                    valueType = valueType.GetGenericArguments()[0];
                    typeInfo = valueType.GetTypeInfo();
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
                var elementType = valueType.GetElementType();
                var list = (ArrayList)BuildList(context, typeof(ArrayList), elementType, typeInfo, value, nestingLevel + 1);
                return list.ToArray(elementType);
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

            if (IsEnumerable(valueType, genericTypeDef))
            {
                var value = CastIntoEnumerable(field);
                var list = BuildList(context, valueType, GetListItemType(valueType, typeInfo), typeInfo, value, nestingLevel + 1);
                return list;
            }

            var mapping = GetTypeMapping(context, valueType, nestingLevel);
            return useTypeMappings && mapping != null
                ? MutateObject(context, mapping, valueType, field, nestingLevel)
                : CreateObject(context, valueType, field, nestingLevel + 1);
        }

        private object MutateObject(DeserializationContext context, TypeMapping mapping, Type propertyType,
            TField field, int nestingLevel)
        {
            var newType = mapping.DetermineTypeToParseJsonIntoBasedOnPropertyType(propertyType);
            var rawItem = CoerceValue(context, newType, field, nestingLevel + 1);
            var item = mapping.MutationCallback(rawItem);
            return item;
        }

        private bool IsEnumerable(Type collectionType, Type genericType)
        {
            return genericType == typeof(IEnumerable<>) || genericType == typeof(IList<>) ||
                   collectionType
                       .GetInterfaces()
                       .Where(i => i.GetTypeInfo().IsGenericType)
                       .Any(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                                 i.GetGenericTypeDefinition() == typeof(IList<>));
        }

        private Type GetListItemType(Type collectionType, TypeInfo ti)
        {
            if (ti.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
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

        private IList BuildList(DeserializationContext context, Type collectionType, Type itemType, TypeInfo collectionTypeInfo,
            IEnumerable<TField> items, int nestingLevel)
        {
            if (items == null)
            {
                return null;
            }

            if (collectionTypeInfo.IsGenericType && 
                collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
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

            foreach (var entry in CastIntoDictionaryEntries(null, field))
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
            var props = GetPropertiesForType(context, targetType);
            var entries = CastIntoDictionaryEntries(props, field)
                .ToDictionary(e => e.Key, e => e.Value);

            foreach (var property in props)
            {
                var propertyName = property.Key;
                var propertyInfo = property.Value;

                if (entries.ContainsKey(propertyName))
                {
                    SetPropertyValue(context, propertyInfo, entries[propertyName], instance, nestingLevel);
                }
            }

            return instance;
        }
    }
}
