using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Neo4j.Driver.V1;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace Neo4jClient.Serialization.BoltDriver
{
    /// <summary>
    /// Deserializes based on Neo4j Binary Drivers (PackStream)
    /// </summary>
    /// <typeparam name="TResult">The type of the result object</typeparam>
    public class DriverDeserializer<TResult> : BaseDeserializer<TResult, IStatementResult, IStatementResult, IRecord, object, ITypeSerializer>,
        IDriverDeserializer<TResult>
    {
        private IRecord currentRecord;
        private readonly IGraphClient client;

        public DriverDeserializer(IGraphClient client, CypherResultMode resultMode) : base(client, resultMode)
        {
            this.client = client;
        }

        protected override string GenerateExceptionDetails(Exception exception, IStatementResult results)
        {
            const string exceptionDetailsFormat = @"Include this full record instance, with any sensitive values replaced with non-sensitive equivalents:

Keys: {0}

Current Record Object Graph (in JSON): {1}

Unconsumed Results Object Graph (in JSON, max 100 records): {2}";

            return string.Format(exceptionDetailsFormat,
                string.Join(", ", results.Keys),
                GenerateObjectGraph(currentRecord),
                GenerateObjectGraph(results));
        }

        private string GenerateObjectGraph(object instance)
        {
            var typeRecordSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(instance, Formatting.Indented, typeRecordSettings);
        }


        private string GenerateObjectGraph(IStatementResult results)
        {
            return GenerateObjectGraph(results.Select(record => record).Take(100));
        }

        protected override IStatementResult DeserializeIntoRecordCollections(IStatementResult results)
        {
            return results;
        }

        protected override bool IsNullArray(PropertyInfo propInfo, object field)
        {
            // Empty arrays in Cypher tables come back as things like [null] or [null,null]
            // instead of just [] or null. We detect these scenarios and convert them to just
            // null.

            var propertyType = propInfo.PropertyType;

            var isEnumerable =
                propertyType.GetTypeInfo().IsGenericType &&
                propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>);

            var isArrayOrEnumerable =
                isEnumerable ||
                propertyType.IsArray;

            if (!isArrayOrEnumerable)
            {
                return false;
            }

            if (!(field is IEnumerable))
            {
                return false;
            }

            var items = ((IEnumerable) field).Cast<object>().ToArray();
            var hasOneOrMoreChildrenAndAllAreNull =
                items.Any() &&
                items.All(item => item == null);

            return hasOneOrMoreChildrenAndAllAreNull;
        }

        protected override DeserializationContext<ITypeSerializer> GenerateContext(IStatementResult results, CypherResultMode resultMode)
        {
            return new DeserializationContext<ITypeSerializer>
            {
                Culture = CultureInfo.InvariantCulture,
                Converters = Enumerable.Reverse((client as IBoltGraphClient)?.TypeSerializers ?? new List<ITypeSerializer>()).ToArray(),
                JsonContractResolver = client.JsonContractResolver,
                TypeMappings = new TypeMapping[] { }
            };
        }

        protected override string[] GetColumnNames(IStatementResult results)
        {
            return results.Keys.ToArray();
        }

        protected override IEnumerable<IRecord> GetRecordsFromResults(IStatementResult results)
        {
            return results;
        }

        protected override object GetElementForDeserializationInSingleColumn(IRecord record)
        {
            return record[record.Keys[0]];
        }

        protected override void RegisterRecordBeingDeserialized(IRecord record)
        {
            currentRecord = record;
        }

        protected override IEnumerable<FieldEntry> GetFieldEntries(string[] columnNames, IRecord record)
        {
            return record.Values.Select(pair => new FieldEntry(pair.Key, pair.Value));
        }

        protected override IEnumerable<object> CastIntoEnumerable(object value)
        {
            if (value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    yield return item;
                }

                yield break;
            }

            throw new DeserializationException(
                $"Type {value.GetType().FullName} returned by Neo4j cannot be cast into an enumeration. " +
                $"Only Neo4j enumerable objects, like lists, can be used in this fashion.");
        }

        protected override object CastIntoPrimitiveType(Type primitiveType, object value)
        {
            return Convert.ChangeType(value, primitiveType);
        }

        protected override bool TryCastIntoDateTime(object value, out DateTime? dt)
        {
            dt = null;
            if (value is string s)
            {
                dt = DateDeserializerMethods.ParseDateTime(s);
                return true;
            }

            return false;
        }

        protected override bool TryCastIntoDateTimeOffset(object value, out DateTimeOffset? dt)
        {
            dt = null;
            if (value is string s)
            {
                dt = DateDeserializerMethods.ParseDateTimeOffset(s);
                return true;
            }

            return false;
        }

        private IEnumerable<DictionaryEntry> CastDictionaryEntries(IDictionary dict)
        {
            foreach (var entry in dict)
            {
                yield return (DictionaryEntry)entry;
            }
        }

        protected override IEnumerable<FieldEntry> CastIntoDictionaryEntries(Dictionary<string, PropertyInfo> props, object value)
        {
            if (value is IDictionary dictionary)
            {
                return CastDictionaryEntries(dictionary).Select(e => new FieldEntry((string)e.Key, e.Value));
            }

            if (value is INode node)
            {
                return node.Properties.Select(pair => new FieldEntry(pair.Key, pair.Value));
            }

            if (value is IRelationship relationship)
            {
                return relationship.Properties.Select(pair => new FieldEntry(pair.Key, pair.Value));
            }

            throw new DeserializationException(
                $"Type {value.GetType().FullName} returned by Neo4j cannot be deserialized into dictionary entries. " +
                $"Only maps, nodes, and relationships can be deserialized into dictionary entries.");
        }

        protected override Dictionary<string, PropertyInfo> GetPropertiesForType(DeserializationContext<ITypeSerializer> context, Type targetType)
        {
            return targetType
                .GetProperties()
                .Where(p => p.CanWrite)
                .Select(p =>
                {
                    var propertyNameFromAttribute = ((DataMemberAttribute)p.GetCustomAttributes(typeof(DataMemberAttribute), true)
                        .SingleOrDefault())
                        ?.Name;

                    return new
                    {
                        Name = propertyNameFromAttribute ?? p.Name,
                        Property = p
                    };
                })
                .ToDictionary(p => p.Name, p => p.Property);
        }

        protected override TypeMapping GetTypeMapping(DeserializationContext<ITypeSerializer> context, Type type, int nestingLevel)
        {
            return null;
        }

        protected override bool TryDeserializeCustomType(DeserializationContext<ITypeSerializer> context, Type propertyType, object field,
            out object deserialized)
        {
            deserialized = null;

            var converter = context.Converters?.FirstOrDefault(c => c.CanConvert(propertyType));
            if (converter != null)
            {
                deserialized = converter.Deserialize(propertyType, field);
                return true;
            }

            var typeInfo = propertyType.GetTypeInfo();
            var genericTypeDefinition = typeInfo.IsGenericType ? typeInfo.GetGenericTypeDefinition() : null;
            if (genericTypeDefinition == typeof(Node<>))
            {
                if (field is INode node)
                {
                    var nodeType = propertyType.GetGenericArguments()[0];
                    var obj = DeserializeObject(context, nodeType, field);
                    var nodeId = node.Id;

                    var nodeConcreteClass = typeof(Node<>).MakeGenericType(nodeType);
                    var nodeClassConstructor = nodeConcreteClass.GetConstructor(new[] {
                        nodeType, typeof(long), typeof(IGraphClient)});
                    deserialized = nodeClassConstructor?.Invoke(new[] {obj, nodeId, null});
                    
                    return deserialized != null;
                }

                throw new DeserializationException(
                    $"A node must be returned by the IRecord entry in order to be deserialized into {propertyType.Name}");
            }
            if (genericTypeDefinition == typeof(RelationshipInstance<>))
            {
                if (field is IRelationship relationship)
                {
                    var dataType = propertyType.GetGenericArguments()[0];
                    var obj = DeserializeObject(context, dataType, field);
                    var relationshipId = relationship.Id;

                    var nodeConcreteClass = typeof(RelationshipInstance<>).MakeGenericType(dataType);
                    var nodeClassConstructor = nodeConcreteClass.GetConstructor(new[] {
                        typeof(long), typeof(long), typeof(long), typeof(string), dataType});
                    deserialized = nodeClassConstructor?.Invoke(new[]
                    {
                        relationshipId,
                        relationship.StartNodeId,
                        relationship.EndNodeId,
                        relationship.Type,
                        obj
                    });

                    return deserialized != null;
                }

                throw new DeserializationException(
                    $"A relationship must be returned by the IRecord entry in order to be deserialized into {propertyType.Name}");
            }

            if (propertyType == typeof(PathsResultBolt))
            {
                if (field is IPath path)
                {
                    deserialized = new PathsResultBolt(path);
                    return true;
                }

                throw new DeserializationException(
                    $"A path must be returned by the IRecord entry in order to be deserialized into {typeof(PathsResultBolt).Name}");
            }

            if (propertyType == typeof(IPath))
            {
                if (field is IPath path)
                {
                    deserialized = path;
                    return true;
                }

                throw new DeserializationException(
                    $"A path must be returned by the IRecord entry in order to be deserialized into {typeof(IPath).Name}");
            }

            if (propertyType == typeof(INode))
            {
                if (field is INode node)
                {
                    deserialized = node;
                    return true;
                }

                throw new DeserializationException(
                    $"A node must be returned by the IRecord entry in order to be deserialized into {typeof(INode).Name}");
            }

            if (propertyType == typeof(IRelationship))
            {
                if (field is IRelationship relationship)
                {
                    deserialized = relationship;
                    return true;
                }

                throw new DeserializationException(
                    $"A relationship must be returned by the IRecord entry in order to be deserialized into {typeof(IRelationship).Name}");
            }

            return false;
        }
    }
}
