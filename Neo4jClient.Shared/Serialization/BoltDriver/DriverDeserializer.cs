using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
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
    public class DriverDeserializer<TResult> : BaseDeserializer<TResult, IStatementResult, IStatementResult, IRecord, object>,
        IDriverDeserializer<TResult>
    {
        private IRecord currentRecord;

        public DriverDeserializer(IGraphClient client, CypherResultMode resultMode) : base(client, resultMode)
        {
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

        protected override DeserializationContext GenerateContext(IStatementResult results, CypherResultMode resultMode)
        {
            var context = base.GenerateContext(results, resultMode);
            context.TypeMappings = new TypeMapping[] { };
            return context;
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
                dt = CommonDeserializerMethods.ParseDateTime(s);
                return true;
            }

            return false;
        }

        protected override bool TryCastIntoDateTimeOffset(object value, out DateTimeOffset? dt)
        {
            dt = null;
            if (value is string s)
            {
                dt = CommonDeserializerMethods.ParseDateTimeOffset(s);
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

        protected override Dictionary<string, PropertyInfo> GetPropertiesForType(DeserializationContext context, Type targetType)
        {
            return targetType
                .GetProperties()
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name);
        }

        protected override TypeMapping GetTypeMapping(DeserializationContext context, Type type, int nestingLevel)
        {
            return null;
        }

        protected override bool TryDeserializeCustomType(Type propertyType, object field, out object deserialized)
        {
            deserialized = null;
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
