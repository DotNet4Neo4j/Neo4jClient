using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Gremlin;
using Neo4jClient.Serialization;
using Neo4jClient.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient
{
    public partial class GraphClient : IRawGraphClient, IInternalTransactionalGraphClient<HttpResponseMessage>, IDisposable
    {
        internal const string GremlinPluginUnavailable =
            "You're attempting to execute a Gremlin query, however the server instance you are connected to does not have the Gremlin plugin loaded. If you've recently upgraded to Neo4j 2.0, you'll need to be aware that Gremlin no longer ships as part of the normal Neo4j distribution.  Please move to equivalent (but much more powerful and readable!) Cypher.";
        internal const string MaxExecutionTimeHeaderKey = "max-execution-time";

        public static readonly JsonConverter[] DefaultJsonConverters =
        {
            new TypeConverterBasedJsonConverter(),
            new NullableEnumValueConverter(),
            new TimeZoneInfoConverter(),
            new EnumValueConverter()
        };

        public static readonly DefaultContractResolver DefaultJsonContractResolver = new DefaultContractResolver();

        private ITransactionManager<HttpResponseMessage> transactionManager;
        private readonly IExecutionPolicyFactory policyFactory;

        public ExecutionConfiguration ExecutionConfiguration { get; private set; }

        internal readonly Uri RootUri;
        internal RootApiResponse RootApiResponse;
        private RootNode rootNode;

        private CypherCapabilities cypherCapabilities = CypherCapabilities.Default;


        public bool UseJsonStreamingIfAvailable { get; set; }

        //        public GraphClient(Uri rootUri, string username = null, string password = null)
        //            : this(rootUri, new HttpClientWrapper(username, password))
        //        {
        //            ServicePointManager.Expect100Continue = true;
        //            ServicePointManager.UseNagleAlgorithm = false;
        //        }
        //
        //        public GraphClient(Uri rootUri, bool expect100Continue, bool useNagleAlgorithm, string username = null, string password = null)
        //            : this(rootUri, new HttpClientWrapper(username, password))
        //        {
        //            ServicePointManager.Expect100Continue = expect100Continue;
        //            ServicePointManager.UseNagleAlgorithm = useNagleAlgorithm;
        //        }

        public GraphClient(Uri rootUri, IHttpClient httpClient)
        {
            RootUri = rootUri;
            JsonConverters = new List<JsonConverter>();
            JsonConverters.AddRange(DefaultJsonConverters);
            JsonContractResolver = DefaultJsonContractResolver;
            ExecutionConfiguration = new ExecutionConfiguration
            {
                HttpClient = httpClient,
                UserAgent = $"Neo4jClient/{GetType().GetTypeInfo().Assembly.GetName().Version}",
                UseJsonStreaming = true,
                JsonConverters = JsonConverters,
                Username = httpClient?.Username,
                Password = httpClient?.Password
            };
            UseJsonStreamingIfAvailable = true;
            policyFactory = new ExecutionPolicyFactory(this);
        }

        private Uri BuildUri(string relativeUri)
        {
            var baseUri = RootUri;
            if (!RootUri.AbsoluteUri.EndsWith("/"))
                baseUri = new Uri(RootUri.AbsoluteUri + "/");

            if (relativeUri.StartsWith("/"))
                relativeUri = relativeUri.Substring(1);

            return new Uri(baseUri, relativeUri);
        }

        private string SerializeAsJson(object contents)
        {
            return Serializer.Serialize(contents);
        }

        public virtual bool IsConnected => RootApiResponse != null;

        public virtual void Connect(NeoServerConfiguration configuration = null)
        {
            var task = ConnectAsync(configuration);
            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException ex)
            {
                var operationCompleteArgs = new OperationCompletedEventArgs();

                Exception unwrappedException;
                var wasUnwrapped = ex.TryUnwrap(out unwrappedException);
                operationCompleteArgs.Exception = wasUnwrapped ? unwrappedException : ex;

                OperationCompleted?.Invoke(this, operationCompleteArgs);

                if (wasUnwrapped)
                    throw unwrappedException;

                throw;
            }
            catch (Exception ex)
            {
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs { Exception = ex });
                throw;
            }
        }

        [Obsolete(
            "The concept of a single root node has being dropped in Neo4j 2.0. Use an alternate strategy for having known reference points in the graph, such as labels."
            )]
        public virtual RootNode RootNode
        {
            get
            {
                CheckRoot();
                return rootNode;
            }
        }

        public virtual NodeReference<TNode> Create<TNode>(
            TNode node,
            IEnumerable<IRelationshipAllowingParticipantNode<TNode>> relationships,
            IEnumerable<IndexEntry> indexEntries)
            where TNode : class
        {
            if (typeof (TNode).GetTypeInfo().IsGenericType &&
                typeof (TNode).GetGenericTypeDefinition() == typeof (Node<>))
            {
                throw new ArgumentException(string.Format(
                    "You're trying to pass in a Node<{0}> instance. Just pass the {0} instance instead.",
                    typeof (TNode).GetGenericArguments()[0].Name),
                    "node");
            }

            if (node == null)
                throw new ArgumentNullException("node");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            relationships = relationships ?? Enumerable.Empty<IRelationshipAllowingParticipantNode<TNode>>();
            indexEntries = (indexEntries ?? Enumerable.Empty<IndexEntry>()).ToArray();

            var validationContext = new ValidationContext(node, null, null);
            Validator.ValidateObject(node, validationContext);

            var calculatedRelationships = relationships
                .Cast<Relationship>()
                .Select(r => new
                {
                    CalculatedDirection = Relationship.DetermineRelationshipDirection(typeof(TNode), r),
                    Relationship = r
                })
                .ToArray();

            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Batch);
            CheckTransactionEnvironmentWithPolicy(policy);

            var batchSteps = new List<BatchStep>();

            var createNodeStep = batchSteps.Add(HttpMethod.Post, "/node", node);

            foreach (var relationship in calculatedRelationships)
            {
                var participants = new[]
                {
                    string.Format("{{{0}}}", createNodeStep.Id),
                    string.Format("/node/{0}", relationship.Relationship.OtherNode.Id)
                };
                string sourceNode, targetNode;
                switch (relationship.CalculatedDirection)
                {
                    case RelationshipDirection.Outgoing:
                        sourceNode = participants[0];
                        targetNode = participants[1];
                        break;
                    case RelationshipDirection.Incoming:
                        sourceNode = participants[1];
                        targetNode = participants[0];
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            "The specified relationship direction is not supported: {0}",
                            relationship.CalculatedDirection));
                }

                var relationshipTemplate = new RelationshipTemplate
                {
                    To = targetNode,
                    Data = relationship.Relationship.Data,
                    Type = relationship.Relationship.RelationshipTypeKey
                };
                batchSteps.Add(HttpMethod.Post, sourceNode + "/relationships", relationshipTemplate);
            }

            var entries = indexEntries
                .SelectMany(i => i
                    .KeyValues
                    .Select(kv => new
                    {
                        IndexAddress = BuildRelativeIndexAddress(i.Name, IndexFor.Node),
                        kv.Key,
                        Value = EncodeIndexValue(kv.Value)
                    })
                    .Where(e => !string.IsNullOrEmpty(e.Value)));
            foreach (var indexEntry in entries)
            {
                batchSteps.Add(HttpMethod.Post, indexEntry.IndexAddress, new
                {
                    key = indexEntry.Key,
                    value = indexEntry.Value,
                    uri = "{0}"
                });
            }

            var batchResponse = ExecuteBatch(batchSteps, policy);
            var createResponse = batchResponse[createNodeStep];
            EnsureNodeWasCreated(createResponse);
            var nodeId = long.Parse(GetLastPathSegment(createResponse.Location));
            var nodeReference = new NodeReference<TNode>(nodeId, this);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = string.Format("Create<{0}>", typeof(TNode).Name),
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });

            return nodeReference;
        }

        private BatchResponse ExecuteBatch(List<BatchStep> batchSteps, IExecutionPolicy policy)
        {
            return Request.With(ExecutionConfiguration)
                .Post(policy.BaseEndpoint)
                .WithJsonContent(SerializeAsJson(batchSteps))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<BatchResponse>()
                .Execute();
        }

        public virtual RelationshipReference CreateRelationship<TSourceNode, TRelationship>(
            NodeReference<TSourceNode> sourceNodeReference,
            TRelationship relationship)
            where TRelationship :
                Relationship,
                IRelationshipAllowingSourceNode<TSourceNode>
        {
            if (sourceNodeReference == null)
                throw new ArgumentNullException("sourceNodeReference");

            if (relationship.Direction == RelationshipDirection.Incoming)
                throw new NotSupportedException("Incoming relationships are not yet supported by this method.");

            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            return CreateRelationship(
                sourceNodeReference,
                relationship.OtherNode,
                relationship.RelationshipTypeKey,
                relationship.Data,
                policy);
        }

        private RelationshipReference CreateRelationship(NodeReference sourceNode, NodeReference targetNode,
            string relationshipTypeKey, object data, IExecutionPolicy policy)
        {
            var relationship = new RelationshipTemplate
            {
                To = policy.BaseEndpoint.AddPath(targetNode, policy).ToString(),
                Data = data,
                Type = relationshipTypeKey
            };

            var sourceNodeEndpoint = policy.BaseEndpoint
                .AddPath(sourceNode, policy)
                .AddPath("relationships");

            return Request.With(ExecutionConfiguration)
                .Post(sourceNodeEndpoint)
                .WithJsonContent(SerializeAsJson(relationship))
                .WithExpectedStatusCodes(HttpStatusCode.Created, HttpStatusCode.NotFound)
                .ParseAs<RelationshipApiResponse<object>>()
                .FailOnCondition(responseMessage => responseMessage.StatusCode == HttpStatusCode.NotFound)
                .WithError(responseMessage => new Exception(string.Format(
                    "One of the nodes referenced in the relationship could not be found. Referenced nodes were {0} and {1}.",
                    sourceNode.Id,
                    targetNode.Id))
                )
                .Execute()
                //.ReadAsJson<RelationshipApiResponse<object>>(JsonConverters,JsonContractResolver)
                .ToRelationshipReference(this);
        }

        CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer { JsonConverters = JsonConverters, JsonContractResolver = JsonContractResolver };
        }

        public ISerializer Serializer
        {
            get { return new CustomJsonSerializer { JsonConverters = JsonConverters, JsonContractResolver = JsonContractResolver }; }
        }

        public void DeleteRelationship(RelationshipReference reference)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Request.With(ExecutionConfiguration)
                .Delete(policy.BaseEndpoint.AddPath(reference, policy))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent, HttpStatusCode.NotFound)
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.NotFound)
                .WithError(response => new Exception(string.Format(
                    "Unable to delete the relationship. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.ReasonPhrase)))
                .Execute();

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = "Delete Relationship " + reference.Id,
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        public virtual Node<TNode> Get<TNode>(NodeReference reference)
        {
            var task = GetAsync<TNode>(reference);
            if (task.Exception != null)
            {
                Exception unwrappedException;
                if (task.Exception.TryUnwrap(out unwrappedException))
                    throw unwrappedException;
                throw task.Exception;
            }
            Task.WaitAll(task);
            return task.Result;
        }

        public virtual Task<Node<TNode>> GetAsync<TNode>(NodeReference reference)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            return Request.With(ExecutionConfiguration)
                .Get(policy.BaseEndpoint.AddPath(reference, policy))
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NotFound)
                .ParseAs<NodeApiResponse<TNode>>()
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.NotFound)
                .WithDefault()
                .ExecuteAsync(nodeMessage => nodeMessage.Result != null ? nodeMessage.Result.ToNode(this) : null);
        }

        public virtual Node<TNode> Get<TNode>(NodeReference<TNode> reference)
        {
            return Get<TNode>((NodeReference)reference);
        }

        public virtual RelationshipInstance<TData> Get<TData>(RelationshipReference<TData> reference)
            where TData : class, new()
        {
            return Get<TData>((RelationshipReference)reference);
        }

        public virtual RelationshipInstance<TData> Get<TData>(RelationshipReference reference)
            where TData : class, new()
        {
            var task = GetAsync<TData>(reference);
            if (task.Exception != null)
            {
                Exception unwrappedException;
                if (task.Exception.TryUnwrap(out unwrappedException))
                    throw unwrappedException;
                throw task.Exception;
            }

            Task.WaitAll(task);
            return task.Result;
        }

        public virtual Task<RelationshipInstance<TData>> GetAsync<TData>(RelationshipReference reference) where TData : class, new()
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            return Request.With(ExecutionConfiguration)
                .Get(policy.BaseEndpoint.AddPath(reference, policy))
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NotFound)
                .ParseAs<RelationshipApiResponse<TData>>()
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.NotFound)
                .WithDefault()
                .ExecuteAsync(
                    responseTask =>
                        responseTask.Result != null ? responseTask.Result.ToRelationshipInstance(this) : null);
        }

        public void Update<TNode>(NodeReference<TNode> nodeReference, TNode replacementData,
            IEnumerable<IndexEntry> indexEntries = null)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var allIndexEntries = indexEntries == null
                ? new IndexEntry[0]
                : indexEntries.ToArray();

            Request.With(ExecutionConfiguration)
                .Put(policy.BaseEndpoint.AddPath(nodeReference, policy).AddPath("properties"))
                .WithJsonContent(SerializeAsJson(replacementData))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .Execute();

            if (allIndexEntries.Any())
                ReIndex(nodeReference, allIndexEntries);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = string.Format("Update<{0}> {1}", typeof(TNode).Name, nodeReference.Id),
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        public Node<TNode> Update<TNode>(NodeReference<TNode> nodeReference, Action<TNode> updateCallback,
            Func<TNode, IEnumerable<IndexEntry>> indexEntriesCallback = null,
            Action<IEnumerable<FieldChange>> changeCallback = null)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var node = Get(nodeReference);

            var indexEntries = new IndexEntry[] { };

            if (indexEntriesCallback != null)
            {
                indexEntries = indexEntriesCallback(node.Data).ToArray();
            }

            var serializer = Serializer;

            var originalValuesString = changeCallback == null ? null : serializer.Serialize(node.Data);

            updateCallback(node.Data);

            if (changeCallback != null)
            {
                var originalValuesDictionary =
                    new CustomJsonDeserializer(JsonConverters, resolver: JsonContractResolver).Deserialize<Dictionary<string, string>>(
                        originalValuesString);
                var newValuesString = serializer.Serialize(node.Data);
                var newValuesDictionary =
                    new CustomJsonDeserializer(JsonConverters, resolver: JsonContractResolver).Deserialize<Dictionary<string, string>>(newValuesString);
                var differences = Utilities.GetDifferencesBetweenDictionaries(originalValuesDictionary,
                    newValuesDictionary);
                changeCallback(differences);
            }

            Request.With(ExecutionConfiguration)
                .Put(policy.BaseEndpoint.AddPath(nodeReference, policy).AddPath("properties"))
                .WithJsonContent(serializer.Serialize(node.Data))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .Execute();

            if (indexEntriesCallback != null)
            {
                ReIndex(node.Reference, indexEntries);
            }

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = string.Format("Update<{0}> {1}", typeof(TNode).Name, nodeReference.Id),
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });

            return node;
        }

        public void Update<TRelationshipData>(RelationshipReference<TRelationshipData> relationshipReference,
            Action<TRelationshipData> updateCallback)
            where TRelationshipData : class, new()
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var propertiesEndpoint = policy.BaseEndpoint.AddPath(relationshipReference, policy).AddPath("properties");
            var currentData = Request.With(ExecutionConfiguration)
                .Get(propertiesEndpoint)
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NoContent)
                .ParseAs<TRelationshipData>()
                .Execute();

            var payload = currentData ?? new TRelationshipData();
            updateCallback(payload);

            Request.With(ExecutionConfiguration)
                .Put(propertiesEndpoint)
                .WithJsonContent(SerializeAsJson(payload))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .Execute();

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = string.Format("Update<{0}> {1}", typeof(TRelationshipData).Name, relationshipReference.Id),
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        public virtual void Delete(NodeReference reference, DeleteMode mode)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (mode == DeleteMode.NodeAndRelationships)
            {
                DeleteAllRelationships(reference, policy);
            }

            Request.With(ExecutionConfiguration)
                .Delete(policy.BaseEndpoint.AddPath(reference, policy))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent, HttpStatusCode.Conflict)
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.Conflict)
                .WithError(response => new Exception(string.Format(
                    "Unable to delete the node. The node may still have relationships. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.ReasonPhrase)))
                .Execute();

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = "Delete " + reference.Id,
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        private void DeleteAllRelationships(NodeReference reference, IExecutionPolicy policy)
        {
            //TODO: Make this a dynamic endpoint resolution
            var relationshipEndpoint = policy.BaseEndpoint
                .AddPath(reference, policy)
                .AddPath("relationships")
                .AddPath("all");
            var result = Request.With(ExecutionConfiguration)
                .Get(relationshipEndpoint)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<List<RelationshipApiResponse<object>>>()
                .Execute();

            var relationshipResources = result.Select(r => r.Self);
            foreach (var relationshipResource in relationshipResources)
            {
                Request.With(ExecutionConfiguration)
                    .Delete(new Uri(relationshipResource))
                    .WithExpectedStatusCodes(HttpStatusCode.NoContent, HttpStatusCode.NotFound)
                    .Execute();
            }
        }

        private static string GetLastPathSegment(string uri)
        {
            var path = new Uri(uri).AbsolutePath;
            return path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();
        }

        public ICypherFluentQuery Cypher
        {
            get { return new CypherFluentQuery(this); }
        }

        [Obsolete(
            "Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher."
            )]
        public IGremlinClient Gremlin
        {
            get { return new GremlinClient(this); }
        }

        public Version ServerVersion
        {
            get
            {
                CheckRoot();
                return RootApiResponse.Version;
            }
        }

        public Uri RootEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri("");
            }
        }

        public Uri TransactionEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.Transaction);
            }
        }

        public Uri BatchEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.Batch);
            }
        }

        public Uri CypherEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.Cypher);
            }
        }

        public Uri RelationshipIndexEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.RelationshipIndex);
            }
        }

        public Uri NodeIndexEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.NodeIndex);
            }
        }

        public Uri GremlinEndpoint
        {
            get
            {
                CheckRoot();
                if (RootApiResponse.Extensions.GremlinPlugin == null ||
                    string.IsNullOrEmpty(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript))
                {
                    return null;
                }
                return BuildUri(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript);
            }
        }

        public List<JsonConverter> JsonConverters { get; }

        private void CheckTransactionEnvironmentWithPolicy(IExecutionPolicy policy)
        {
            bool inTransaction = InTransaction;

            if (transactionManager != null)
            {
                transactionManager.RegisterToTransactionIfNeeded();
            }

            if (inTransaction && policy.TransactionExecutionPolicy == TransactionExecutionPolicy.Denied)
            {
                throw new InvalidOperationException("Cannot be done inside a transaction scope.");
            }

            if (!inTransaction && policy.TransactionExecutionPolicy == TransactionExecutionPolicy.Required)
            {
                throw new InvalidOperationException("Cannot be done outside a transaction scope.");
            }
        }

        public ITransaction BeginTransaction()
        {
            return BeginTransaction(TransactionScopeOption.Join);
        }

        public ITransaction BeginTransaction(TransactionScopeOption scopeOption)
        {
            CheckRoot();
            if (transactionManager == null)
            {
                throw new NotSupportedException("HTTP Transactions are only supported on Neo4j 2.0 and newer.");
            }

            return transactionManager.BeginTransaction(scopeOption);
        }

        public ITransaction Transaction => transactionManager?.CurrentNonDtcTransaction;

        public bool InTransaction => transactionManager != null && transactionManager.InTransaction;

        public void EndTransaction()
        {
            if (transactionManager == null)
            {
                throw new NotSupportedException("HTTP Transactions are only supported on Neo4j 2.0 and newer.");
            }
            transactionManager.EndTransaction();
        }

        [Obsolete(
            "Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher."
            )]
        public virtual string ExecuteScalarGremlin(string query, IDictionary<string, object> parameters)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Gremlin);
            CheckTransactionEnvironmentWithPolicy(policy);

            if (RootApiResponse.Extensions.GremlinPlugin == null ||
                RootApiResponse.Extensions.GremlinPlugin.ExecuteScript == null)
                throw new Exception(GremlinPluginUnavailable);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = Request.With(ExecutionConfiguration)
                .Post(policy.BaseEndpoint)
                .WithJsonContent(Serializer.Serialize(new GremlinApiQuery(query, parameters)))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .Execute(string.Format("The query was: {0}", query));

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query,
                ResourcesReturned = 1,
                TimeTaken = stopwatch.Elapsed
            });

            return response.Content.ReadAsString();
        }

        [Obsolete(
            "Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher."
            )]
        public virtual IEnumerable<TResult> ExecuteGetAllProjectionsGremlin<TResult>(IGremlinQuery query)
            where TResult : new()
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Gremlin);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = Request.With(ExecutionConfiguration)
                .Post(policy.BaseEndpoint)
                .WithJsonContent(SerializeAsJson(new GremlinApiQuery(query.QueryText, query.QueryParameters)))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<List<List<GremlinTableCapResponse>>>()
                .Execute(string.Format("The query was: {0}", query.QueryText));

            var responses = response ?? new List<List<GremlinTableCapResponse>> { new List<GremlinTableCapResponse>() };

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query.ToDebugQueryText(),
                ResourcesReturned = responses.Count(),
                TimeTaken = stopwatch.Elapsed
            });

            var results = GremlinTableCapResponse.TransferResponseToResult<TResult>(responses, JsonConverters);

            return results;
        }

        public CypherCapabilities CypherCapabilities => cypherCapabilities;

        [Obsolete(
            "This method is for use by the framework internally. Use IGraphClient.Cypher instead, and read the documentation at https://bitbucket.org/Readify/neo4jclient/wiki/cypher. If you really really want to call this method directly, and you accept the fact that YOU WILL LIKELY INTRODUCE A RUNTIME SECURITY RISK if you do so, then it shouldn't take you too long to find the correct explicit interface implementation that you have to call. This hurdle is for your own protection. You really really should not do it. This signature may be removed or renamed at any time.",
            true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IEnumerable<TResult> ExecuteGetCypherResults<TResult>(CypherQuery query)
        {
            throw new NotImplementedException();
        }

        private Task<CypherPartialResult> PrepareCypherRequest<TResult>(CypherQuery query, IExecutionPolicy policy)
        {
            if (InTransaction)
            {
                return transactionManager
                    .EnqueueCypherRequest(string.Format("The query was: {0}", query.QueryText), this, query)
                    .ContinueWith(responseTask =>
                    {
                        // we need to check for errors returned by the transaction. The difference with a normal REST cypher
                        // query is that the errors are embedded within the result object, instead of having a 400 bad request
                        // status code.
                        var response = responseTask.Result;
                        var deserializer = new CypherJsonDeserializer<TResult>(this, query.ResultMode, query.ResultFormat, true);
                        return new CypherPartialResult
                        {
                            DeserializationContext =
                                deserializer.CheckForErrorsInTransactionResponse(response.Content.ReadAsString()),
                            ResponseObject = response
                        };
                    });
            }

            int? maxExecutionTime = null;
            NameValueCollection customHeaders = null;
            if (query != null)
            {
                maxExecutionTime = query.MaxExecutionTime;
                customHeaders = query.CustomHeaders;
            }

            return Request.With(ExecutionConfiguration, customHeaders, maxExecutionTime)
                .Post(policy.BaseEndpoint)
                .WithJsonContent(policy.SerializeRequest(query))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ExecuteAsync(response => new CypherPartialResult
                {
                    ResponseObject = response.Result
                });
        }

        IEnumerable<TResult> IRawGraphClient.ExecuteGetCypherResults<TResult>(CypherQuery query)
        {
            var task = ((IRawGraphClient)this).ExecuteGetCypherResultsAsync<TResult>(query);
            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException ex)
            {
                Exception unwrappedException;
                if (ex.TryUnwrap(out unwrappedException))
                    throw unwrappedException;
                throw;
            }

            return task.Result;
        }

        async Task<IEnumerable<TResult>> IRawGraphClient.ExecuteGetCypherResultsAsync<TResult>(CypherQuery query)
        {
            var context = ExecutionContext.Begin(this);
            List<TResult> results;
            try
            {
                // the transaction handling is handled by a thread-local variable (ThreadStatic) so we need
                // to know if we are in a transaction right now because our deserializer will run in another thread
                bool inTransaction = InTransaction;

                var response = await PrepareCypherRequest<TResult>(query, context.Policy).ConfigureAwait(false);
                var deserializer = new CypherJsonDeserializer<TResult>(this, query.ResultMode, query.ResultFormat,
                    inTransaction);
                if (inTransaction)
                {
                    response.DeserializationContext.DeserializationContext.JsonContractResolver =
                        query.JsonContractResolver;
                    results =
                        deserializer.DeserializeFromTransactionPartialContext(response.DeserializationContext).ToList();
                }
                else
                {
                    results = deserializer.Deserialize(response.ResponseObject.Content.ReadAsString()).ToList();
                }

            }
            catch (AggregateException aggregateException)
            {
                Exception unwrappedException;
                if (aggregateException.TryUnwrap(out unwrappedException))
                {
                    context.Complete(query, unwrappedException);
                }
                else
                {
                    context.Complete(query, aggregateException);
                }
                throw;
            }
            catch (Exception e)
            {
                context.Complete(query, e);
                throw;
            }

            context.Complete(query, results.Count());

            return results;
        }

        void IRawGraphClient.ExecuteCypher(CypherQuery query)
        {
            var context = ExecutionContext.Begin(this);

            var task = PrepareCypherRequest<object>(query, context.Policy);
            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException ex)
            {
                if (InTransaction)
                    ExecutionConfiguration.HasErrors = true;

                Exception unwrappedException;
                if (ex.TryUnwrap(out unwrappedException))
                {
                    context.Complete(query, unwrappedException);
                    throw unwrappedException;
                }

                context.Complete(query, ex);
                throw;
            }

            context.Policy.AfterExecution(TransactionHttpUtils.GetMetadataFromResponse(task.Result.ResponseObject), null);

            context.Complete(query);
        }

        async Task IRawGraphClient.ExecuteCypherAsync(CypherQuery query)
        {
            var context = ExecutionContext.Begin(this);

            var response = await PrepareCypherRequest<object>(query, context.Policy).ConfigureAwait(false);
            context.Policy.AfterExecution(TransactionHttpUtils.GetMetadataFromResponse(response.ResponseObject), null);

            context.Complete(query);
        }

        void IRawGraphClient.ExecuteMultipleCypherQueriesInTransaction(IEnumerable<CypherQuery> queries, NameValueCollection customHeaders)
        {
            var context = ExecutionContext.Begin(this);

            var queryList = queries.ToList();
            string queriesInText = string.Join(", ", queryList.Select(query => query.QueryText));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = Request.With(ExecutionConfiguration, customHeaders)
                .Post(context.Policy.BaseEndpoint)
                .WithJsonContent(SerializeAsJson(new CypherStatementList(queryList)))
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.Created)
                .Execute("Executing multiple queries: " + queriesInText);

            var transactionObject = transactionManager.CurrentNonDtcTransaction ??
                                    transactionManager.CurrentDtcTransaction;

            if (customHeaders != null && customHeaders.Count > 0)
            {
                transactionObject.CustomHeaders = customHeaders;
            }

            context.Policy.AfterExecution(TransactionHttpUtils.GetMetadataFromResponse(response), transactionObject);
            context.Complete(OperationCompleted != null ? string.Join(", ", queryList.Select(query => query.DebugQueryText)) : string.Empty);
            context.Policy.AfterExecution(TransactionHttpUtils.GetMetadataFromResponse(response), transactionObject);
        }

        [Obsolete(
            "Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher."
            )]
        public virtual IEnumerable<RelationshipInstance> ExecuteGetAllRelationshipsGremlin(string query,
            IDictionary<string, object> parameters)
        {
            return ExecuteGetAllRelationshipsGremlin<object>(query, parameters);
        }

        [Obsolete(
            "Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher."
            )]
        public virtual IEnumerable<RelationshipInstance<TData>> ExecuteGetAllRelationshipsGremlin<TData>(string query,
            IDictionary<string, object> parameters)
            where TData : class, new()
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Gremlin);
            CheckTransactionEnvironmentWithPolicy(policy);

            if (RootApiResponse.Extensions.GremlinPlugin == null ||
                RootApiResponse.Extensions.GremlinPlugin.ExecuteScript == null)
                throw new Exception(GremlinPluginUnavailable);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = Request.With(ExecutionConfiguration)
                .Post(policy.BaseEndpoint)
                .WithJsonContent(SerializeAsJson(new GremlinApiQuery(query, parameters)))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<List<RelationshipApiResponse<TData>>>()
                .Execute(string.Format("The query was: {0}", query));

            var relationships = response == null
                ? new RelationshipInstance<TData>[0]
                : response.Select(r => r.ToRelationshipInstance(this)).ToArray();

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query,
                ResourcesReturned = relationships.Count(),
                TimeTaken = stopwatch.Elapsed
            });

            return relationships;
        }

        [Obsolete(
            "Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher."
            )]
        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query,
            IDictionary<string, object> parameters)
        {
            return ExecuteGetAllNodesGremlin<TNode>(new GremlinQuery(this, query, parameters, new List<string>()));
        }

        [Obsolete(
            "Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher."
            )]
        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query,
            IDictionary<string, object> parameters, IList<string> declarations)
        {
            return ExecuteGetAllNodesGremlin<TNode>(new GremlinQuery(this, query, parameters, declarations));
        }

        [Obsolete(
            "Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher."
            )]
        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(IGremlinQuery query)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Gremlin);
            CheckTransactionEnvironmentWithPolicy(policy);

            if (RootApiResponse.Extensions.GremlinPlugin == null ||
                RootApiResponse.Extensions.GremlinPlugin.ExecuteScript == null)
                throw new Exception(GremlinPluginUnavailable);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = Request.With(ExecutionConfiguration)
                .Post(policy.BaseEndpoint)
                .WithJsonContent(SerializeAsJson(new GremlinApiQuery(query.QueryText, query.QueryParameters)))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<List<NodeApiResponse<TNode>>>()
                .Execute(string.Format("The query was: {0}", query.QueryText));

            var nodes = response == null
                ? new Node<TNode>[0]
                : response.Select(r => r.ToNode(this)).ToArray();

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query.ToDebugQueryText(),
                ResourcesReturned = nodes.Count(),
                TimeTaken = stopwatch.Elapsed
            });

            return nodes;
        }

        private IExecutionPolicy GetPolicyForIndex(IndexFor indexFor)
        {
            switch (indexFor)
            {
                case IndexFor.Node:
                    return policyFactory.GetPolicy(PolicyType.NodeIndex);
                case IndexFor.Relationship:
                    return policyFactory.GetPolicy(PolicyType.RelationshipIndex);
                default:
                    throw new NotSupportedException(string.Format("GetIndexes does not support indexfor {0}", indexFor));
            }
        }

        private Uri GetUriForIndexType(IndexFor indexFor)
        {
            var policy = GetPolicyForIndex(indexFor);
            CheckTransactionEnvironmentWithPolicy(policy);
            return policy.BaseEndpoint;
        }

        public Dictionary<string, IndexMetaData> GetIndexes(IndexFor indexFor)
        {
            CheckRoot();

            var result = Request.With(ExecutionConfiguration)
                .Get(GetUriForIndexType(indexFor))
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NoContent)
                .ParseAs<Dictionary<string, IndexMetaData>>()
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.NoContent)
                .WithDefault()
                .Execute();

            return result ?? new Dictionary<string, IndexMetaData>();
        }

        public bool CheckIndexExists(string indexName, IndexFor indexFor)
        {
            CheckRoot();

            var baseEndpoint = GetUriForIndexType(indexFor);
            var response = Request.With(ExecutionConfiguration)
                .Get(baseEndpoint.AddPath(indexName))
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NotFound)
                .Execute();

            return response.StatusCode == HttpStatusCode.OK;
        }

        private void CheckRoot()
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException(
                    "The graph client is not connected to the server. Call the Connect method first.");
        }

        public void CreateIndex(string indexName, IndexConfiguration config, IndexFor indexFor)
        {
            CheckRoot();

            var baseEndpoint = GetUriForIndexType(indexFor);
            var createIndexApiRequest = new
            {
                name = indexName,
                config
            };

            Request.With(ExecutionConfiguration)
                .Post(baseEndpoint)
                .WithJsonContent(SerializeAsJson(createIndexApiRequest))
                .WithExpectedStatusCodes(HttpStatusCode.Created)
                .Execute();
        }

        public void ReIndex(NodeReference node, IEnumerable<IndexEntry> indexEntries)
        {
            var restPolicy = policyFactory.GetPolicy(PolicyType.Rest);
            var entityUri = restPolicy.BaseEndpoint.AddPath(node, restPolicy);
            var entityId = node.Id;
            ReIndex(entityUri.ToString(), entityId, IndexFor.Node, indexEntries);
        }

        public void ReIndex(RelationshipReference relationship, IEnumerable<IndexEntry> indexEntries)
        {
            var restPolicy = policyFactory.GetPolicy(PolicyType.Rest);
            var entityUri = restPolicy.BaseEndpoint.AddPath(relationship, restPolicy);
            var entityId = relationship.Id;
            ReIndex(entityUri.ToString(), entityId, IndexFor.Relationship, indexEntries);
        }

        private void ReIndex(string entityUri, long entityId, IndexFor indexFor, IEnumerable<IndexEntry> indexEntries,
            IExecutionPolicy policy)
        {
            if (indexEntries == null)
                throw new ArgumentNullException("indexEntries");

            CheckRoot();
            CheckTransactionEnvironmentWithPolicy(policy);

            var updates = indexEntries
                .SelectMany(
                    i => i.KeyValues,
                    (i, kv) => new { IndexName = i.Name, kv.Key, kv.Value })
                .Where(update => update.Value != null)
                .ToList();

            foreach (var indexName in updates.Select(u => u.IndexName).Distinct())
                DeleteIndexEntries(indexName, entityId, GetUriForIndexType(indexFor));

            foreach (var update in updates)
                AddIndexEntry(update.IndexName, update.Key, update.Value, entityUri, indexFor);
        }

        public void ReIndex(string entityUri, long entityId, IndexFor indexFor, IEnumerable<IndexEntry> indexEntries)
        {
            ReIndex(entityUri, entityId, indexFor, indexEntries, GetPolicyForIndex(indexFor));
        }

        public void DeleteIndex(string indexName, IndexFor indexFor)
        {
            CheckRoot();
            var policy = GetPolicyForIndex(indexFor);
            CheckTransactionEnvironmentWithPolicy(policy);

            Request.With(ExecutionConfiguration)
                .Delete(policy.BaseEndpoint.AddPath(indexName))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .Execute();
        }

        public void DeleteIndexEntries(string indexName, NodeReference nodeReference)
        {
            DeleteIndexEntries(indexName, nodeReference.Id, GetUriForIndexType(IndexFor.Node));
        }

        public void DeleteIndexEntries(string indexName, RelationshipReference relationshipReference)
        {
            DeleteIndexEntries(indexName, relationshipReference.Id, GetUriForIndexType(IndexFor.Relationship));
        }

        private void DeleteIndexEntries(string indexName, long id, Uri indexUri)
        {
            var indexAddress = indexUri
                .AddPath(Uri.EscapeDataString(indexName))
                .AddPath(Uri.EscapeDataString(id.ToString(CultureInfo.InvariantCulture)));

            Request.With(ExecutionConfiguration)
                .Delete(indexAddress)
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .Execute(
                    string.Format("Deleting entries from index {0} for node {1}", indexName, id)
                );
        }

        private void AddIndexEntry(string indexName, string indexKey, object indexValue, string address,
            IndexFor indexFor)
        {
            var encodedIndexValue = EncodeIndexValue(indexValue);
            if (string.IsNullOrWhiteSpace(encodedIndexValue))
                return;

            var indexAddress = BuildIndexAddress(indexName, indexFor);

            var indexEntry = new
            {
                key = indexKey,
                value = encodedIndexValue,
                uri = address
            };

            Request.With(ExecutionConfiguration)
                .Post(indexAddress)
                .WithJsonContent(SerializeAsJson(indexEntry))
                .WithExpectedStatusCodes(HttpStatusCode.Created)
                .Execute(string.Format("Adding '{0}'='{1}' to index {2} for {3}", indexKey, indexValue, indexName,
                    address));
        }

        private string BuildRelativeIndexAddress(string indexName, IndexFor indexFor)
        {
            var baseUri = indexFor == IndexFor.Node
                ? new UriBuilder() { Path = RootApiResponse.NodeIndex }
                : new UriBuilder() { Path = RootApiResponse.RelationshipIndex };
            return baseUri.Uri.AddPath(Uri.EscapeDataString(indexName)).LocalPath;
        }

        private Uri BuildIndexAddress(string indexName, IndexFor indexFor)
        {
            return GetUriForIndexType(indexFor).AddPath(Uri.EscapeDataString(indexName));
        }

        private static string EncodeIndexValue(object value)
        {
            string indexValue;
            if (value is DateTimeOffset)
            {
                indexValue = ((DateTimeOffset)value).UtcTicks.ToString(CultureInfo.InvariantCulture);
            }
            else if (value is DateTime)
            {
                indexValue = ((DateTime)value).Ticks.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                indexValue = value.ToString();
            }

            if (string.IsNullOrWhiteSpace(indexValue) ||
                !indexValue.Any(char.IsLetterOrDigit))
                return string.Empty;

            return indexValue;
        }

        //ToDo Check status of https://github.com/neo4j/community/issues/249 for limiting query result sets
        [Obsolete(
            "There are encoding issues with this method. You should use the newer Cypher approach instead. See https://bitbucket.org/Readify/neo4jclient/issue/54/spaces-in-search-text-while-searching-for for an explanation of the problem, and https://bitbucket.org/Readify/neo4jclient/wiki/cypher for documentation about doing index queries with Cypher."
            )]
        public IEnumerable<Node<TNode>> QueryIndex<TNode>(string indexName, IndexFor indexFor, string query)
        {
            CheckRoot();
            var indexEndpoint = GetUriForIndexType(indexFor)
                .AddPath(indexName)
                .AddQuery("query=" + Uri.EscapeDataString(query));

            return Request.With(ExecutionConfiguration)
                .Get(indexEndpoint)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<List<NodeApiResponse<TNode>>>()
                .Execute()
                .Select(nodeResponse => nodeResponse.ToNode(this));
        }

        public IEnumerable<Node<TNode>> LookupIndex<TNode>(string exactIndexName, IndexFor indexFor, string indexKey,
            long id)
        {
            return BuildLookupIndex<TNode>(exactIndexName, indexFor, indexKey, id.ToString(CultureInfo.InvariantCulture));
        }

        public IEnumerable<Node<TNode>> LookupIndex<TNode>(string exactIndexName, IndexFor indexFor, string indexKey,
            int id)
        {
            return BuildLookupIndex<TNode>(exactIndexName, indexFor, indexKey, id.ToString(CultureInfo.InvariantCulture));
        }

        private IEnumerable<Node<TNode>> BuildLookupIndex<TNode>(string exactIndexName, IndexFor indexFor,
            string indexKey, string id)
        {
            CheckRoot();
            var indexResource = GetUriForIndexType(indexFor)
                .AddPath(exactIndexName)
                .AddPath(indexKey)
                .AddPath(id.ToString());

            return Request.With(ExecutionConfiguration)
                .Get(indexResource)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<List<NodeApiResponse<TNode>>>()
                .Execute()
                .Select(query => query.ToNode(this));
        }

        [Obsolete(
            "This method depends on Gremlin, which is being dropped in Neo4j 2.0. Find an alternate strategy for server lifetime management."
            )]
        public void ShutdownServer()
        {
            ExecuteScalarGremlin("g.getRawGraph().shutdown()", null);
        }

        public event OperationCompletedEventHandler OperationCompleted;

        protected void OnOperationCompleted(OperationCompletedEventArgs args)
        {
            var eventInstance = OperationCompleted;
            if (eventInstance != null)
                eventInstance(this, args);
        }

        private void EnsureNodeWasCreated(BatchStepResult createResponse)
        {
            if (createResponse.Status == HttpStatusCode.BadRequest && createResponse.Body != null)
            {
                var exceptionResponse = JsonConvert.DeserializeObject<ExceptionResponse>(createResponse.Body);

                if (exceptionResponse == null || string.IsNullOrEmpty(exceptionResponse.Message) || string.IsNullOrEmpty(exceptionResponse.Exception))
                    throw new Exception(string.Format("Response from Neo4J: {0}", createResponse.Body));

                throw new NeoException(exceptionResponse);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (transactionManager != null)
                transactionManager.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public DefaultContractResolver JsonContractResolver { get; set; }

        public ITransactionManager<HttpResponseMessage> TransactionManager
        {
            get { return transactionManager; }
        }

        #region ExecutionContext class
        private class ExecutionContext
        {
            private GraphClient owner;

            private readonly Stopwatch stopwatch;

            public IExecutionPolicy Policy { get; set; }
            public static bool HasErrors { get; set; }

            private ExecutionContext()
            {
                stopwatch = Stopwatch.StartNew();
            }

            public static ExecutionContext Begin(GraphClient owner)
            {
                owner.CheckRoot();
                var policy = owner.policyFactory.GetPolicy(PolicyType.Cypher);

                owner.CheckTransactionEnvironmentWithPolicy(policy);

                var executionContext = new ExecutionContext
                {
                    owner = owner,
                    Policy = policy
                };

                return executionContext;
            }

            public void Complete(CypherQuery query)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, 0, null);
            }

            public void Complete(CypherQuery query, int resultsCount)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, resultsCount, null, query.CustomHeaders);
            }

            public void Complete(CypherQuery query, Exception exception)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, -1, exception);
            }

            public void Complete(string queryText, int resultsCount = -1, Exception exception = null, NameValueCollection customHeaders = null, int? maxExecutionTime = null)
            {
                var args = new OperationCompletedEventArgs
                {
                    QueryText = queryText,
                    ResourcesReturned = resultsCount,
                    TimeTaken = stopwatch.Elapsed,
                    Exception = exception,
                    CustomHeaders = customHeaders,
                    MaxExecutionTime = maxExecutionTime
                };

                owner.OnOperationCompleted(args);
            }
        }

        #endregion
    }
}
