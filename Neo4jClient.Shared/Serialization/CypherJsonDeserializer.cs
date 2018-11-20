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

namespace Neo4jClient.Serialization
{
    public class CypherJsonDeserializer<TResult> : ICypherJsonDeserializer<TResult>
    {
        readonly IGraphClient client;
        readonly CypherResultMode resultMode;
        private readonly CypherResultFormat resultFormat;
        private readonly bool inTransaction;
        private readonly bool inBolt;

        readonly CultureInfo culture = CultureInfo.InvariantCulture;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public CypherJsonDeserializer() { }

        public CypherJsonDeserializer(IGraphClient client, CypherResultMode resultMode, CypherResultFormat resultFormat)
            : this(client, resultMode, resultFormat, false, false)
        {
        }

        public CypherJsonDeserializer(IGraphClient client, CypherResultMode resultMode, CypherResultFormat resultFormat, bool inTransaction)
            : this(client, resultMode, resultFormat, inTransaction, false)
        {
        }

        public CypherJsonDeserializer(IGraphClient client, CypherResultMode resultMode, CypherResultFormat resultFormat, bool inTransaction, bool inBolt)
        {
            this.client = client;
            this.resultMode = resultMode;
            this.inTransaction = inTransaction;
            this.inBolt = inBolt;
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

        public IEnumerable<TResult> Deserialize(string content)
        {
            try
            {
                var context = new DeserializationContext
                {
                    Culture = culture,
                    JsonConverters = Enumerable.Reverse(client.JsonConverters ?? new List<JsonConverter>(0)).ToArray(),
                    JsonContractResolver = client.JsonContractResolver
                };
                content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

                var reader = new JsonTextReader(new StringReader(content))
                {
                    DateParseHandling = DateParseHandling.None
                };

                // Force the deserialization to happen now, not later, as there's
                // not much value to deferred execution here and we'd like to know
                // about any errors now
                return inTransaction
                    ? FullDeserializationFromTransactionResponse(reader, context).ToArray()
                    : DeserializeFromRoot(content, reader, context).ToArray();
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

Include this raw JSON, with any sensitive values replaced with non-sensitive equivalents:

{1}";
                var message = string.Format(messageTemplate, typeof(TResult).FullName, content);

                // If it's a specifc scenario that we're blowing up about, put this front and centre in the message
                if (ex is DeserializationException deserializationException)
                {
                    message = $"{deserializationException.Message}{Environment.NewLine}{Environment.NewLine}----{Environment.NewLine}{Environment.NewLine}{message}";
                }

                throw new ArgumentException(message, nameof(content), ex);
            }
        }

        IEnumerable<TResult> DeserializeInternal(string content)
        {
            var context = new DeserializationContext
            {
                Culture = culture,
                JsonConverters = Enumerable.Reverse(client.JsonConverters ?? new List<JsonConverter>(0)).ToArray(),
                JsonContractResolver = client.JsonContractResolver
            };
            content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

            var reader = new JsonTextReader(new StringReader(content))
            {
                DateParseHandling = DateParseHandling.None
            };
            var root = JToken.ReadFrom(reader).Root;

            var columnsArray = (JArray)root["columns"];
            var columnNames = columnsArray
                .Children()
                .Select(c => c.AsString())
                .ToArray();

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

            switch (resultMode)
            {
                case CypherResultMode.Set:
                    return ParseInSingleColumnMode(context, root, columnNames, jsonTypeMappings.ToArray());
                case CypherResultMode.Projection:
                    jsonTypeMappings.Add(new TypeMapping
                    {
                        ShouldTriggerForPropertyType = (nestingLevel, type) =>
                            nestingLevel == 0 && type.GetTypeInfo().IsClass,
                        DetermineTypeToParseJsonIntoBasedOnPropertyType = t =>
                            typeof(NodeOrRelationshipApiResponse<>).MakeGenericType(new[] { t }),
                        MutationCallback = n =>
                            n.GetType().GetProperty("Data").GetGetMethod().Invoke(n, new object[0])
                    });
                    return ParseInProjectionMode(context, root, columnNames, jsonTypeMappings.ToArray());
                default:
                    throw new NotSupportedException(string.Format("Unrecognised result mode of {0}.", resultMode));
            }
        }

        IEnumerable<TResult> DeserializeResultSet(JToken resultRoot, DeserializationContext context)
        {
            var columnsArray = (JArray)resultRoot["columns"];
            var columnNames = columnsArray
                .Children()
                .Select(c => c.AsString())
                .ToArray();

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

            switch (resultMode)
            {
                case CypherResultMode.Set:
                    return ParseInSingleColumnMode(context, resultRoot, columnNames, jsonTypeMappings.ToArray());
                case CypherResultMode.Projection:
                    // if we are in transaction and we have an object we dont need a mutation
                    if (!inTransaction && !inBolt)
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
                    return ParseInProjectionMode(context, resultRoot, columnNames, jsonTypeMappings.ToArray());
                default:
                    throw new NotSupportedException(string.Format("Unrecognised result mode of {0}.", resultMode));
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

            var context = new DeserializationContext
            {
                Culture = culture,
                JsonConverters = Enumerable.Reverse(client.JsonConverters ?? new List<JsonConverter>(0)).ToArray()
            };
            content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

            var reader = new JsonTextReader(new StringReader(content))
            {
                DateParseHandling =DateParseHandling.None// DateParseHandling.DateTimeOffset
            };

            var root = JToken.ReadFrom(reader).Root as JObject;

            return new PartialDeserializationContext
            {
                RootResult = GetRootResultInTransaction(root),
                DeserializationContext = context
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
            return DeserializeResultSet(context.RootResult, context.DeserializationContext);
        }

        private IEnumerable<TResult> FullDeserializationFromTransactionResponse(JsonTextReader reader, DeserializationContext context)
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
            return DeserializeResultSet(resultSet, context);
        }

        IEnumerable<TResult> DeserializeFromRoot(string content, JsonTextReader reader, DeserializationContext context)
        {
            var root = JToken.ReadFrom(reader).Root;
            if (!(root is JObject))
            {
                throw new InvalidOperationException("Root expected to be a JSON object.");
            }
            return DeserializeResultSet(root, context);
        }

        // ReSharper disable UnusedParameter.Local
        IEnumerable<TResult> ParseInSingleColumnMode(DeserializationContext context, JToken root, string[] columnNames, TypeMapping[] jsonTypeMappings)
        // ReSharper restore UnusedParameter.Local
        {
            if (columnNames.Count() != 1)
                throw new InvalidOperationException("The deserializer is running in single column mode, but the response included multiple columns which indicates a projection instead. If using the fluent Cypher interface, use the overload of Return that takes a lambda or object instead of single string. (The overload with a single string is for an identity, not raw query text: we can't map the columns back out if you just supply raw query text.)");

            var resultType = typeof(TResult);
            var isResultTypeANodeOrRelationshipInstance = resultType.GetTypeInfo().IsGenericType &&
                                       (resultType.GetGenericTypeDefinition() == typeof(Node<>) ||
                                        resultType.GetGenericTypeDefinition() == typeof(RelationshipInstance<>));
            var mapping = jsonTypeMappings.SingleOrDefault(m => m.ShouldTriggerForPropertyType(0, resultType));
            var newType = mapping == null ? resultType : mapping.DetermineTypeToParseJsonIntoBasedOnPropertyType(resultType);

            var dataArray = (JArray)root["data"];
            var rows = dataArray.Children();

            var dataPropertyNameInTransaction = resultFormat == CypherResultFormat.Rest ? "rest" : "row";
            var results = rows.Select(row =>
                    {
                        if (inTransaction)
                        {
                            var rowObject = row as JObject;
                            if (rowObject == null)
                            {
                                throw new InvalidOperationException(
                                    "Expected the row to be a JSON object, but it wasn't.");
                            }

                            JToken rowProperty;
                            if (!rowObject.TryGetValue(dataPropertyNameInTransaction, out rowProperty))
                            {
                                throw new InvalidOperationException("There is no row property in the JSON object.");
                            }

                            row = rowProperty;

                        }

                        if (!(row is JArray))
                        {
                            // no transaction mode and the row is not an array
                            throw new InvalidOperationException(
                                "Expected the row to be a JSON array of values, but it wasn't.");
                        }

                        var rowAsArray = (JArray) row;
                        if (rowAsArray.Count > 1)
                            throw new InvalidOperationException(string.Format("Expected the row to only have a single array value, but it had {0}.", rowAsArray.Count));

                        return rowAsArray;
                    }
                )
                .Where(row => row.Count != 0) // No elements to parse
                .Select(row =>
                {
                    var elementToParse = row[0];
                    if (elementToParse is JObject)
                    {
                        var propertyNames = ((JObject) elementToParse)
                            .Properties()
                            .Select(p => p.Name)
                            .ToArray();
                        var dataElementLooksLikeANodeOrRelationshipInstance =
                            new[] {"data", "self", "traverse", "properties"}.All(propertyNames.Contains);
                        if (!isResultTypeANodeOrRelationshipInstance &&
                            dataElementLooksLikeANodeOrRelationshipInstance)
                        {
                            elementToParse = elementToParse["data"];
                        }
                    }

                    var parsed = CommonDeserializerMethods.CreateAndMap(context, newType, elementToParse, jsonTypeMappings, 0);
                    return (TResult) (mapping == null ? parsed : mapping.MutationCallback(parsed));
                });

            return results;
        }

        IEnumerable<TResult> ParseInProjectionMode(DeserializationContext context, JToken root, string[] columnNames, TypeMapping[] jsonTypeMappings)
        {
            var properties = typeof(TResult).GetProperties();
            var propertiesDictionary = properties
                .ToDictionary(p => p.Name);


            Func<JToken, TResult> getRow = null;

            var columnsWhichDontHaveSettableProperties = columnNames.Where(c => !propertiesDictionary.ContainsKey(c) || !propertiesDictionary[c].CanWrite).ToArray();
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
                    getRow = token => ReadProjectionRowUsingCtor(context, token, propertiesDictionary, columnNames, jsonTypeMappings, ctor);
                }

                if (getRow == null)
                {
                    // wasn't able to build TResult via constructor
                    var columnsWhichDontHaveSettablePropertiesCommaSeparated = string.Join(", ", columnsWhichDontHaveSettableProperties);
                    throw new ArgumentException(string.Format(
                        "The query response contains columns {0} however {1} does not contain publically settable properties to receive this data.",
                        columnsWhichDontHaveSettablePropertiesCommaSeparated,
                        typeof(TResult).FullName),
                        "columnNames");
                }
            }
            else
            {
                getRow = token => ReadProjectionRowUsingProperties(context, token, propertiesDictionary, columnNames, jsonTypeMappings);
            }

            var dataArray = (JArray)root["data"];
            var rows = dataArray.Children();

            var dataPropertyNameInTransaction = resultFormat == CypherResultFormat.Rest ? "rest" : "row";
            return inTransaction ? rows.Select(row => row[dataPropertyNameInTransaction]).Select(getRow) : rows.Select(getRow);
        }

        TResult ReadProjectionRowUsingCtor(
            DeserializationContext context,
            JToken row,
            IDictionary<string, PropertyInfo> propertiesDictionary,
            IList<string> columnNames,
            IEnumerable<TypeMapping> jsonTypeMappings,
            ConstructorInfo ctor)
        {
            var coercedValues = row
                .Children()
                .Select((cell, cellIndex) =>
                {
                    var columnName = columnNames[cellIndex];
                    var property = propertiesDictionary[columnName];
                    if (IsNullArray(property, cell)) return null;

                    var coercedValue = CommonDeserializerMethods.CoerceValue(context, property, cell, jsonTypeMappings, 0);
                    return coercedValue;
                })
                .ToArray();

            var result = (TResult)ctor.Invoke(coercedValues);

            return result;
        }

        TResult ReadProjectionRowUsingProperties(
            DeserializationContext context,
            JToken row,
            IDictionary<string, PropertyInfo> propertiesDictionary,
            IList<string> columnNames,
            TypeMapping[] jsonTypeMappings)
        {
            var result = Activator.CreateInstance<TResult>();

            var cellIndex = 0;
            foreach (var cell in row.Children())
            {
                var columnName = columnNames[cellIndex];
                cellIndex++;

                var property = propertiesDictionary[columnName];

                var isNullArray = IsNullArray(property, cell);
                if (isNullArray) continue;

                CommonDeserializerMethods.SetPropertyValue(context, result, property, cell, jsonTypeMappings, 0);
            }

            return result;
        }

        static bool IsNullArray(PropertyInfo property, JToken cell)
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
                return false;

            if (cell.Type != JTokenType.Array)
                return false;

            var cellChildren = cell.Children().ToArray();
            var hasOneOrMoreChildrenAndAllAreNull =
                cellChildren.Any() &&
                cellChildren.All(c => c.Type == JTokenType.Null);

            return hasOneOrMoreChildrenAndAllAreNull;
        }
    }
}
