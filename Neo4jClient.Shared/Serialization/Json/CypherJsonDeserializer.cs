using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Serialization.Json
{
    public class CypherJsonDeserializer<TResult> : BaseDeserializer<TResult, string, JToken, JToken, JToken>, ICypherJsonDeserializer<TResult>
    {
        readonly IGraphClient client;
        private readonly CypherResultFormat resultFormat;
        private readonly bool inTransaction;

        readonly CultureInfo culture = CultureInfo.InvariantCulture;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public CypherJsonDeserializer() : base(null, CypherResultMode.Projection) { }

        public CypherJsonDeserializer(IGraphClient client, CypherResultMode resultMode, CypherResultFormat resultFormat)
            : this(client, resultMode, resultFormat, false)
        {
        }

        public CypherJsonDeserializer(IGraphClient client, CypherResultMode resultMode, CypherResultFormat resultFormat, bool inTransaction) :
            base(client, resultMode)
        {
            this.client = client;
            this.inTransaction = inTransaction;
            // here is where we decide if we should deserialize as transactional or REST endpoint data format.
            if (resultFormat == CypherResultFormat.DependsOnEnvironment)
            {
                this.resultFormat = inTransaction ? CypherResultFormat.Transactional : CypherResultFormat.Rest;
            }
            else
            {
                this.resultFormat = resultFormat;
            }
        }

        protected override JToken DeserializeIntoRecordCollections(string content)
        {
            content = DateDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

            var reader = new JsonTextReader(new StringReader(content))
            {
                DateParseHandling = DateParseHandling.None
            };

            // Force the deserialization to happen now, not later, as there's
            // not much value to deferred execution here and we'd like to know
            // about any errors now
            return inTransaction
                ? GetSerializedRootFromFullTransaction(reader)
                : GetSerializedRootFromNonTransaction(reader);
        }

        protected override string GenerateExceptionDetails(Exception exception, string results)
        {
            return
                $@"Include this raw JSON, with any sensitive values replaced with non-sensitive equivalents:

{results}";
        }

        protected override string[] GetColumnNames(JToken resultRoot)
        {
            var columnsArray = (JArray)resultRoot["columns"];
            return columnsArray
                .Children()
                .Select(c => c.AsString())
                .ToArray();
        }

        protected override IEnumerable<JToken> GetRecordsFromResults(JToken root)
        {
            var dataArray = (JArray)root["data"];
            var rows = dataArray.Children();

            var dataPropertyNameInTransaction = resultFormat == CypherResultFormat.Rest ? "rest" : "row";
            return inTransaction ? rows.Select(row => row[dataPropertyNameInTransaction]) : rows;
        }

        protected override JToken GetElementForDeserializationInSingleColumn(JToken record)
        {
            var resultType = typeof(TResult);
            var isResultTypeANodeOrRelationshipInstance = resultType.GetTypeInfo().IsGenericType &&
                                                          (resultType.GetGenericTypeDefinition() == typeof(Node<>) ||
                                                           resultType.GetGenericTypeDefinition() == typeof(RelationshipInstance<>));
            if (!(record is JArray))
            {
                // no transaction mode and the row is not an array
                throw new InvalidOperationException("Expected the row to be a JSON array of values, but it wasn't.");
            }
            var rowAsArray = (JArray)record;
            if (rowAsArray.Count != 1)
            {
                throw new InvalidOperationException(string.Format(
                    "Expected the row to only have a single array value, but it had {0}.", rowAsArray.Count));
            }

            var elementToParse = record[0];
            if (elementToParse is JObject)
            {
                var propertyNames = ((JObject)elementToParse)
                    .Properties()
                    .Select(p => p.Name)
                    .ToArray();
                var dataElementLooksLikeANodeOrRelationshipInstance =
                    new[] { "data", "self", "traverse", "properties" }.All(propertyNames.Contains);
                if (!isResultTypeANodeOrRelationshipInstance &&
                    dataElementLooksLikeANodeOrRelationshipInstance)
                {
                    elementToParse = elementToParse["data"];
                }
            }

            return elementToParse;
        }

        protected override void RegisterRecordBeingDeserialized(JToken record)
        {
            // do nothing
        }

        protected override IEnumerable<FieldEntry> GetFieldEntries(string[] columnNames, JToken row)
        {
            return row
                .Children()
                .Select((field, cellIndex) =>
                    new FieldEntry(columnNames[cellIndex], field));
        }

        protected override IEnumerable<FieldEntry> CastIntoDictionaryEntries(Dictionary<string, PropertyInfo> props, JToken field)
        {
            IDictionary<string, JToken> dictionary = field as JObject;
            if (props != null && dictionary != null && !props.Keys.All(dictionary.ContainsKey) && dictionary.ContainsKey("data"))
            {
                field = field["data"];
            }

            return field
                .Children()
                .Cast<JProperty>()
                .Select(prop => new FieldEntry(prop.Name, prop.Value));
        }

        protected override IEnumerable<JToken> CastIntoEnumerable(JToken field)
        {
            return field.Children();
        }

        protected override object CastIntoPrimitiveType(Type primitiveType, JToken value)
        {
            object tmpVal = value.AsString().Replace("\"", string.Empty);
            tmpVal = Convert.ChangeType(tmpVal, primitiveType);
            return tmpVal;
        }

        protected override bool TryCastIntoDateTime(JToken value, out DateTime? dt)
        {
            dt = DateDeserializerMethods.ParseDateTime(value);
            return true;
        }

        protected override bool TryCastIntoDateTimeOffset(JToken value, out DateTimeOffset? dt)
        {
            dt = DateDeserializerMethods.ParseDateTimeOffset(value);
            return true;
        }

        protected override Dictionary<string, PropertyInfo> GetPropertiesForType(DeserializationContext context, Type targetType)
        {
            var camelCase = (context.JsonContractResolver is CamelCasePropertyNamesContractResolver);
            var camel = new Func<string, string>(name => string.Format("{0}{1}",
                name.Substring(0, 1).ToLowerInvariant(),
                name.Length > 1 ? name.Substring(1, name.Length - 1) : string.Empty));

            var properties = targetType
                .GetProperties()
                .Where(p => p.CanWrite)
                .Select(p =>
                {
                    var attributes =
                        (JsonPropertyAttribute[])p.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                    return new
                    {
                        Name = attributes.Any() && attributes.Single().PropertyName != null ? attributes.Single().PropertyName : camelCase ? camel(p.Name) : p.Name, //only camelcase if json property doesn't exist
                        Property = p
                    };
                });

            return properties.ToDictionary(p => p.Name, p => p.Property);
        }

        protected override bool IsNull(PropertyInfo propertyInfo, JToken value)
        {
            if (value == null)
            {
                return true;
            }

            if (value.Type == JTokenType.Null)
            {
                return true;
            }

            return propertyInfo != null && IsNullArray(propertyInfo, value);
        }

        protected override object GetValueFromField(JToken field)
        {
            return ((JValue) field).Value;
        }

        protected override string GetStringFromField(JToken field)
        {
            return field.AsString();
        }

        protected override DeserializationContext GenerateContext(JToken results, CypherResultMode resultMode)
        {
            var context = base.GenerateContext(results, resultMode);
            var jsonTypeMappings = new List<TypeMapping>
            {
                new TypeMapping
                {
                    ShouldTriggerForPropertyType = (nestingLevel, type) =>
                        type.GetTypeInfo().IsGenericType &&
                        type.GetGenericTypeDefinition() == typeof(Node<>),
                    DetermineTypeToParseJsonIntoBasedOnPropertyType = t =>
                    {
                        var nodeType = t.GetGenericArguments();
                        return typeof (NodeApiResponse<>).MakeGenericType(nodeType);
                    },
                    MutationCallback = n => n.GetType().GetMethod("ToNode").Invoke(n, new object[] { client })
                },
                new TypeMapping
                {
                    ShouldTriggerForPropertyType = (nestingLevel, type) =>
                        type.GetTypeInfo().IsGenericType &&
                        type.GetGenericTypeDefinition() == typeof(RelationshipInstance<>),
                    DetermineTypeToParseJsonIntoBasedOnPropertyType = t =>
                    {
                        var relationshipType = t.GetGenericArguments();
                        return typeof (RelationshipApiResponse<>).MakeGenericType(relationshipType);
                    },
                    MutationCallback = n => n.GetType().GetMethod("ToRelationshipInstance").Invoke(n, new object[] { client })
                }
            };

            // if we are in transaction and we have an object we dont need a mutation
            if (resultMode == CypherResultMode.Projection && !inTransaction)
            {
                jsonTypeMappings.Add(new TypeMapping
                {
                    ShouldTriggerForPropertyType = (nestingLevel, type) =>
                        nestingLevel == 0 && type.GetTypeInfo().IsClass,
                    DetermineTypeToParseJsonIntoBasedOnPropertyType = t =>
                        typeof(NodeOrRelationshipApiResponse<>).MakeGenericType(new[] { t }),
                    MutationCallback = n =>
                        n.GetType().GetProperty("Data").GetGetMethod().Invoke(n, new object[0])
                });
            }

            context.TypeMappings = jsonTypeMappings.ToArray();
            return context;
        }

        protected override bool TryDeserializeCustomType(DeserializationContext context, Type propertyType, JToken field,
            out object deserialized)
        {
            deserialized = null;
            var converter = context.JsonConverters?.FirstOrDefault(c => c.CanConvert(propertyType));
            if (converter == null)
            {
                return false;
            }

            using (var reader = field.CreateReader())
            {
                reader.Read();
                deserialized = converter.ReadJson(reader, propertyType, null, null);
                return true;
            }
        }

        private string GetStringPropertyFromObject(JObject obj, string propertyName)
        {
            JToken propValue;
            if (obj.TryGetValue(propertyName, out propValue))
            {
                return (string)(propValue as JValue);
            }
            return null;
        }

        private NeoException BuildNeoException(JToken error)
        {
            var errorObject = error as JObject;
            var code = GetStringPropertyFromObject(errorObject, "code");
            if (code == null)
            {
                throw new InvalidOperationException("Expected 'code' property on error message");
            }

            var message = GetStringPropertyFromObject(errorObject, "message");
            if (message == null)
            {
                throw new InvalidOperationException("Expected 'message' property on error message");
            }

            var lastCodePart = code.Substring(code.LastIndexOf('.') + 1);

            return new NeoException(new ExceptionResponse
            {
                // there is no stack trace in transaction error response
                StackTrace = new string[] { },
                Exception = lastCodePart,
                FullName = code,
                Message = message
            });
        }

        public PartialDeserializationContext CheckForErrorsInTransactionResponse(string content)
        {
            if (!inTransaction)
            {
                throw new InvalidOperationException("Deserialization of this type must be done inside of a transaction scope.");
            }

            content = DateDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

            var reader = new JsonTextReader(new StringReader(content))
            {
                DateParseHandling = DateParseHandling.None// DateParseHandling.DateTimeOffset
            };

            var root = JToken.ReadFrom(reader).Root as JObject;

            var rootResults = GetRootResultInTransaction(root);

            return new PartialDeserializationContext
            {
                RootResult = rootResults,
                DeserializationContext = GenerateContext(rootResults, ResultMode)
            };
        }

        private JToken GetRootResultInTransaction(JObject root)
        {
            if (root == null)
            {
                throw new InvalidOperationException("Root expected to be a JSON object.");
            }

            JToken rawErrors;
            if (root.TryGetValue("errors", out rawErrors))
            {
                var errors = rawErrors as JArray;
                if (errors == null)
                {
                    throw new InvalidOperationException("`errors` property expected to a JSON array.");
                }

                if (errors.Count > 0)
                {
                    throw BuildNeoException(errors.First());
                }
            }

            JToken rawResults;
            if (!root.TryGetValue("results", out rawResults))
            {
                throw new InvalidOperationException("Expected `results` property on JSON root object");
            }

            var results = rawResults as JArray;
            if (results == null)
            {
                throw new InvalidOperationException("`results` property expected to a JSON array.");
            }

            return results.FirstOrDefault();
        }

        public IEnumerable<TResult> DeserializeFromTransactionPartialContext(PartialDeserializationContext context)
        {
            if (context.RootResult == null)
            {
                throw new InvalidOperationException(
                    @"`results` array should have one result set.
This means no query was emitted, so a method that doesn't care about getting results should have been called."
                    );
            }

            return Deserialize(context.RootResult, context.DeserializationContext);
        }

        private JToken GetSerializedRootFromFullTransaction(JsonTextReader reader)
        {
            var root = JToken.ReadFrom(reader).Root as JObject;

            // discarding all the results but the first
            // (this won't affect the library because as of now there is no way of executing
            // multiple statements in the same batch within a transaction and returning the results)
            var resultSet = GetRootResultInTransaction(root);
            if (resultSet == null)
            {
                throw new InvalidOperationException(
                    @"`results` array should have one result set.
This means no query was emitted, so a method that doesn't care about getting results should have been called."
                    );
            }

            return resultSet;
        }

        private JToken GetSerializedRootFromNonTransaction(JsonTextReader reader)
        {
            var root = JToken.ReadFrom(reader).Root;
            if (!(root is JObject))
            {
                throw new InvalidOperationException("Root expected to be a JSON object.");
            }

            return root;
        }

        private bool IsNullArray(PropertyInfo property, JToken cell)
        {
            // Empty arrays in Cypher tables come back as things like [null] or [null,null]
            // instead of just [] or null. We detect these scenarios and convert them to just
            // null.

            var propertyType = property.PropertyType;

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

            if (cell.Type != JTokenType.Array)
            {
                return false;
            }

            var cellChildren = cell.Children().ToArray();
            var hasOneOrMoreChildrenAndAllAreNull =
                cellChildren.Any() &&
                cellChildren.All(c => c.Type == JTokenType.Null);

            return hasOneOrMoreChildrenAndAllAreNull;
        }
    }
}
