using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Neo4jClient.Serialization;
using Newtonsoft.Json;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{Query.DebugQueryText}")]
    public partial class CypherFluentQuery :
        ICypherFluentQuery,
        IAttachedReference
    {
        private readonly Version minimumCypherParserVersion = new Version(1, 9);
        protected readonly IRawGraphClient Client;
        protected readonly QueryWriter QueryWriter;

        public CypherFluentQuery(IGraphClient client)
        {
            if (!(client is IRawGraphClient))
                throw new ArgumentException("The supplied graph client also needs to implement IRawGraphClient", "client");

            Client = (IRawGraphClient)client;
            QueryWriter = new QueryWriter();
        }

        protected CypherFluentQuery(IGraphClient client, QueryWriter queryWriter)
        {
            if (!(client is IRawGraphClient))
                throw new ArgumentException("The supplied graph client also needs to implement IRawGraphClient", "client");

            Client = (IRawGraphClient)client;
            QueryWriter = queryWriter;
        }

        ICypherFluentQuery Mutate(Action<QueryWriter> callback)
        {
            var newWriter = QueryWriter.Clone();
            callback(newWriter);
            return new CypherFluentQuery(Client, newWriter);
        }

        protected ICypherFluentQuery<TResult> Mutate<TResult>(Action<QueryWriter> callback)
        {
            var newWriter = QueryWriter.Clone();
            callback(newWriter);
            return new CypherFluentQuery<TResult>(Client, newWriter);
        }

        public ICypherFluentQuery WithParam(string key, object value)
        {
            if (QueryWriter.ContainsParameterWithKey(key))
                throw new ArgumentException("A parameter with the given key is already defined in the query.", "key");
            return Mutate(w => w.CreateParameter(key, value));
        }

        public ICypherFluentQuery WithParams(IDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0) return this;

            if (parameters.Keys.Any(key => QueryWriter.ContainsParameterWithKey(key)))
                throw new ArgumentException("A parameter with the given key is already defined in the query.", "parameters");

            return Mutate(w => w.CreateParameters(parameters));
        }

        public ICypherFluentQuery WithParams(object parameters)
        {
            if (parameters == null) return this;
            var keyValuePairs = new Dictionary<string, object>();
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(parameters))
            {
                var key = propertyDescriptor.Name;
                var value = propertyDescriptor.GetValue(parameters);
                keyValuePairs.Add(key, value);
            }
            return WithParams(keyValuePairs);
        }

        [Obsolete("Use Start(new { identity = startText }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
        public ICypherFluentQuery Start(string identity, string startText)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("START {0}={1}", identity, startText)));
        }

        public ICypherFluentQuery Start(object startBits)
        {
            return Mutate(w =>
            {
                var startBitsText = StartBitFormatter.FormatAsCypherText(startBits, w.CreateParameter);
                var startText = "START " + startBitsText;
                w.AppendClause(startText);
            });
        }

        public ICypherFluentQuery Start(IDictionary<string, object> startBits)
        {
            return Mutate(w =>
            {
                var startBitsText = StartBitFormatter.FormatAsCypherText(startBits, w.CreateParameter);
                var startText = "START " + startBitsText;
                w.AppendClause(startText);
            });
        }

        [Obsolete("Use Start(new { foo = nodeRef1, bar = All.Nodes }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
        public ICypherFluentQuery Start(params ICypherStartBit[] startBits)
        {
            return Mutate(w =>
            {
                var startBitsText = startBits
                    .Select(b => b.ToCypherText(w.CreateParameter))
                    .ToArray();
                var startText = "START " + string.Join(", ", startBitsText);

                w.AppendClause(startText);
            });
        }

        public ICypherFluentQuery Start(string identity, params NodeReference[] nodeReferences)
        {
            return Start(new Dictionary<string, object>
            {
                { identity, nodeReferences }
            });
        }

        public ICypherFluentQuery Start(string identity, params RelationshipReference[] relationshipReferences)
        {
            return Start(new Dictionary<string, object>
            {
                { identity, relationshipReferences }
            });
        }

        [Obsolete("Use Start(new { foo = Node.ByIndexLookup(…) }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
        public ICypherFluentQuery StartWithNodeIndexLookup(string identity, string indexName, string key, object value)
        {
            return Start(new Dictionary<string, object>
            {
                {identity, Node.ByIndexLookup(indexName, key, value)}
            });
        }

        [Obsolete("Use Start(new { foo = Node.ByIndexQuery(…) }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
        public ICypherFluentQuery StartWithNodeIndexLookup(string identity, string indexName, string parameter)
        {
            return Start(new Dictionary<string, object>
            {
                {identity, Node.ByIndexQuery(indexName, parameter)}
            });
        }

        public ICypherFluentQuery Match(params string[] matchText)
        {
            return Mutate(w =>
                w.AppendClause("MATCH " + string.Join(", ", matchText)));
        }

        public ICypherFluentQuery Merge(string mergeText)
        {
            return Mutate(w => w.AppendClause("MERGE " + mergeText));
        }

        public ICypherFluentQuery OnCreate(string identity)
        {
            return Mutate(w => w.AppendClause("ON CREATE " + identity));
        }

        public ICypherFluentQuery OnMatch(string identity)
        {
            return Mutate(w => w.AppendClause("ON MATCH " + identity));
        }

        public ICypherFluentQuery CreateUnique(string createUniqueText)
        {
            return Mutate(w => w.AppendClause("CREATE UNIQUE " + createUniqueText));
        }

        public ICypherFluentQuery Create(string createText, params object[] objects)
        {
            objects
                .Select((o, i) =>
                {
                    if (o == null)
                        throw new ArgumentException("Array includes a null entry", "objects");

                    var objectType = o.GetType();
                    if (objectType.IsGenericType &&
                        objectType.GetGenericTypeDefinition() == typeof(Node<>))
                    {
                        throw new ArgumentException(string.Format(
                            "You're trying to pass in a Node<{0}> instance. Just pass the {0} instance instead.",
                            objectType.GetGenericArguments()[0].Name),
                            "objects");
                    }

                    var validationContext = new ValidationContext(o, null, null);
                    Validator.ValidateObject(o, validationContext);

                    var objectText = GetSerializer().Serialize(o);
                    return new KeyValuePair<string, string>("{" + i + "}", objectText);
                })
                .ToList()
                .ForEach(kv => createText = createText.Replace(kv.Key, kv.Value));

            return Mutate(w =>
                w.AppendClause("CREATE " + createText));
        }

        CustomJsonSerializer GetSerializer()
        {
            return new CustomJsonSerializer
                {
                    NullHandling = NullValueHandling.Ignore,
                    QuoteName = false,
                    JsonConverters = Client.JsonConverters
                };
        }

        public ICypherFluentQuery Create<TNode>(string identity, TNode node)
            where TNode : class 
        {
            if (typeof(TNode).IsGenericType &&
                typeof(TNode).GetGenericTypeDefinition() == typeof(Node<>)) {
                throw new ArgumentException(string.Format(
                   "You're trying to pass in a Node<{0}> instance. Just pass the {0} instance instead.",
                   typeof(TNode).GetGenericArguments()[0].Name),
                   "node");
            }
            
            if (node == null)
                throw new ArgumentNullException("node");
            
            var validationContext = new ValidationContext(node, null, null);
            Validator.ValidateObject(node, validationContext);
            
            var serializer = GetSerializer();
            return Mutate(w =>
                w.AppendClause(string.Format("CREATE ({0} {1})", identity, serializer.Serialize(node))));
        }

        public ICypherFluentQuery Delete(string identities)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("DELETE {0}", identities)));
        }

        public ICypherFluentQuery Set(string setText)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("SET {0}", setText)));
        }

        public ICypherFluentQuery Union()
        {
            return Mutate(w => w.AppendClause("UNION"));
        }

        public ICypherFluentQuery UnionAll()
        {
            return Mutate(w => w.AppendClause("UNION ALL"));
        }

        public ICypherFluentQuery Limit(int? limit)
        {
            return limit.HasValue
                ? Mutate(w => w.AppendClause("LIMIT {0}", limit))
                : this;
        }

        public ICypherFluentQuery Skip(int? skip)
        {
            return skip.HasValue
                ? Mutate(w => w.AppendClause("SKIP {0}", skip))
                : this;
        }

        public ICypherFluentQuery OrderBy(params string[] properties)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("ORDER BY {0}", string.Join(", ", properties))));
        }

        public ICypherFluentQuery OrderByDescending(params string[] properties)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("ORDER BY {0} DESC", string.Join(", ", properties))));
        }

        public CypherQuery Query
        {
            get { return QueryWriter.ToCypherQuery(); }
        }

        public void ExecuteWithoutResults()
        {
            Client.ExecuteCypher(Query);
        }

        IGraphClient IAttachedReference.Client
        {
            get { return Client; }
        }

        public ICypherFluentQuery ParserVersion(Version version)
        {
            if (version < minimumCypherParserVersion)
                return Mutate(w => w.AppendClause("CYPHER LEGACY"));
            
            return Mutate(w => w.AppendClause(string.Format("CYPHER {0}.{1}", version.Major, version.Minor)));
        }

        public ICypherFluentQuery ParserVersion(int major, int minor)
        {
            return ParserVersion(new Version(major, minor));
        }
    }
}
