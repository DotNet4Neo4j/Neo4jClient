using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.Cypher;
using Neo4jClient.Gremlin;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient
{
    public class GraphClient : IRawGraphClient
    {
        internal const string GremlinPluginUnavailable = "You're attempting to execute a Gremlin query, however the server instance you are connected to does not have the Gremlin plugin loaded. If you've recently upgraded to Neo4j 2.0, you'll need to be aware that Gremlin no longer ships as part of the normal Neo4j distribution.  Please move to equivalent (but much more powerful and readable!) Cypher.";

        public static readonly JsonConverter[] DefaultJsonConverters =
        {
            new TypeConverterBasedJsonConverter(),
            new NullableEnumValueConverter(),
            new TimeZoneInfoConverter(),
            new EnumValueConverter()
        };

        public static readonly DefaultContractResolver DefaultJsonContractResolver  = new DefaultContractResolver();


        internal readonly Uri RootUri;
        readonly IHttpClient httpClient;
        internal RootApiResponse RootApiResponse;
        RootNode rootNode;
        bool jsonStreamingAvailable;
        readonly string userAgent;
        CypherCapabilities cypherCapabilities = CypherCapabilities.Default;

        public bool UseJsonStreamingIfAvailable { get; set; }
        
        public GraphClient(Uri rootUri)
            : this(rootUri, new HttpClientWrapper())
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.UseNagleAlgorithm = false;
        }

        public GraphClient(Uri rootUri, bool expect100Continue, bool useNagleAlgorithm)
            : this(rootUri, new HttpClientWrapper())
        {
            ServicePointManager.Expect100Continue = expect100Continue;
            ServicePointManager.UseNagleAlgorithm = useNagleAlgorithm;
        }

        public GraphClient(Uri rootUri, IHttpClient httpClient)
        {
            RootUri = rootUri;
            this.httpClient = httpClient;
            UseJsonStreamingIfAvailable = true;

            var assemblyVersion = GetType().Assembly.GetName().Version;
            userAgent = string.Format("Neo4jClient/{0}", assemblyVersion);

            JsonConverters = new List<JsonConverter>();
            JsonConverters.AddRange(DefaultJsonConverters);
            JsonContractResolver = DefaultJsonContractResolver;
        }

        internal string UserAgent { get { return userAgent; } }

        Uri BuildUri(string relativeUri)
        {
            var baseUri = RootUri;
            if (!RootUri.AbsoluteUri.EndsWith("/"))
                baseUri = new Uri(RootUri.AbsoluteUri + "/");

            if (relativeUri.StartsWith("/"))
                relativeUri = relativeUri.Substring(1);

            return new Uri(baseUri, relativeUri);
        }

        HttpRequestMessage HttpDelete(string relativeUri)
        {
            var absoluteUri = BuildUri(relativeUri);
            return new HttpRequestMessage(HttpMethod.Delete, absoluteUri);
        }

        HttpRequestMessage HttpGet(string relativeUri)
        {
            var absoluteUri = BuildUri(relativeUri);
            return new HttpRequestMessage(HttpMethod.Get, absoluteUri);
        }

        HttpRequestMessage HttpPostAsJson(string relativeUri, object postBody)
        {
            var absoluteUri = BuildUri(relativeUri);
            var postBodyJson = BuildSerializer().Serialize(postBody);
            var request = new HttpRequestMessage(HttpMethod.Post, absoluteUri)
            {
                Content = new StringContent(postBodyJson, Encoding.UTF8, "application/json")
            };
            return request;
        }

        HttpRequestMessage HttpPutAsJson(string relativeUri, object putBody)
        {
            var absoluteUri = BuildUri(relativeUri);
            var postBodyJson = BuildSerializer().Serialize(putBody);
            var request = new HttpRequestMessage(HttpMethod.Put, absoluteUri)
            {
                Content = new StringContent(postBodyJson, Encoding.UTF8, "application/json")
            };
            return request;
        }

        HttpResponseMessage SendHttpRequest(HttpRequestMessage request, params HttpStatusCode[] expectedStatusCodes)
        {
            return SendHttpRequest(request, null, expectedStatusCodes);
        }

        Task<HttpResponseMessage> SendHttpRequestAsync(HttpRequestMessage request, params HttpStatusCode[] expectedStatusCodes)
        {
            return SendHttpRequestAsync(request, null, expectedStatusCodes);
        }

        HttpResponseMessage SendHttpRequest(HttpRequestMessage request, string commandDescription, params HttpStatusCode[] expectedStatusCodes)
        {
            var task = SendHttpRequestAsync(request, commandDescription, expectedStatusCodes);
            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count() == 1)
                    throw ex.InnerExceptions.Single();
                throw;
            }
            return task.Result;
        }

        Task<HttpResponseMessage> SendHttpRequestAsync(HttpRequestMessage request, string commandDescription, params HttpStatusCode[] expectedStatusCodes)
        {
            if (UseJsonStreamingIfAvailable && jsonStreamingAvailable)
            {
                request.Headers.Accept.Clear();
                request.Headers.Remove("Accept");
                request.Headers.Add("Accept", "application/json;stream=true");
            }

            request.Headers.Add("User-Agent", userAgent);

            var userInfo = request.RequestUri.UserInfo;
            if (!string.IsNullOrEmpty(userInfo))
            {
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userInfo));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }

            var baseTask = httpClient.SendAsync(request);
            var continuationTask = baseTask.ContinueWith(requestTask =>
            {
                var response = requestTask.Result;
                response.EnsureExpectedStatusCode(commandDescription, expectedStatusCodes);
                return response;
            });
            return continuationTask;
        }

        T SendHttpRequestAndParseResultAs<T>(HttpRequestMessage request, params HttpStatusCode[] expectedStatusCodes) where T : new()
        {
            return SendHttpRequestAndParseResultAs<T>(request, null, expectedStatusCodes);
        }

        T SendHttpRequestAndParseResultAs<T>(HttpRequestMessage request, string commandDescription, params HttpStatusCode[] expectedStatusCodes) where T : new()
        {
            var response = SendHttpRequest(request, commandDescription, expectedStatusCodes);
            return response.Content == null ? default(T) : response.Content.ReadAsJson<T>(JsonConverters, JsonContractResolver);
        }

        public virtual void Connect()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = SendHttpRequestAndParseResultAs<RootApiResponse>(
                HttpGet(""),
                HttpStatusCode.OK);

            var rootUriWithoutUserInfo = RootUri;
            if (!string.IsNullOrEmpty(rootUriWithoutUserInfo.UserInfo))
                rootUriWithoutUserInfo = new UriBuilder(RootUri.AbsoluteUri) {UserName = "", Password = ""}.Uri;
            var baseUriLengthToTrim = rootUriWithoutUserInfo.AbsoluteUri.Length;

            RootApiResponse = result;
            RootApiResponse.Batch = RootApiResponse.Batch.Substring(baseUriLengthToTrim);
            RootApiResponse.Node = RootApiResponse.Node.Substring(baseUriLengthToTrim);
            RootApiResponse.NodeIndex = RootApiResponse.NodeIndex.Substring(baseUriLengthToTrim);
            RootApiResponse.Relationship = "/relationship"; //Doesn't come in on the Service Root
            RootApiResponse.RelationshipIndex = RootApiResponse.RelationshipIndex.Substring(baseUriLengthToTrim);
            RootApiResponse.ExtensionsInfo = RootApiResponse.ExtensionsInfo.Substring(baseUriLengthToTrim);
            if (RootApiResponse.Extensions != null && RootApiResponse.Extensions.GremlinPlugin != null)
            {
                RootApiResponse.Extensions.GremlinPlugin.ExecuteScript =
                    RootApiResponse.Extensions.GremlinPlugin.ExecuteScript.Substring(baseUriLengthToTrim);
            }

            if (RootApiResponse.Cypher != null)
            {
                RootApiResponse.Cypher =
                    RootApiResponse.Cypher.Substring(baseUriLengthToTrim);
            }

            rootNode = string.IsNullOrEmpty(RootApiResponse.ReferenceNode)
                ? null
                : new RootNode(long.Parse(GetLastPathSegment(RootApiResponse.ReferenceNode)), this);

            // http://blog.neo4j.org/2012/04/streaming-rest-api-interview-with.html
            jsonStreamingAvailable = RootApiResponse.Version >= new Version(1, 8);

            if (RootApiResponse.Version < new Version(2, 0))
                cypherCapabilities = CypherCapabilities.Cypher19;

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = "Connect",
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        [Obsolete("The concept of a single root node has being dropped in Neo4j 2.0. Use an alternate strategy for having known reference points in the graph, such as labels.")]
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
            if (typeof(TNode).IsGenericType &&
                typeof(TNode).GetGenericTypeDefinition() == typeof(Node<>))
            {
                throw new ArgumentException(string.Format(
                    "You're trying to pass in a Node<{0}> instance. Just pass the {0} instance instead.",
                    typeof(TNode).GetGenericArguments()[0].Name),
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
                        CalculatedDirection = Relationship.DetermineRelationshipDirection(typeof (TNode), r),
                        Relationship = r
                    })
                .ToArray();

            CheckRoot();

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
                        IndexAddress = BuildIndexAddress(i.Name, IndexFor.Node),
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

            var batchResponse = ExecuteBatch(batchSteps);

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

        BatchResponse ExecuteBatch(List<BatchStep> batchSteps)
        {
            var response = SendHttpRequestAndParseResultAs<BatchResponse>(
                HttpPostAsJson(RootApiResponse.Batch, batchSteps),
                HttpStatusCode.OK);

            return response;
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

            return CreateRelationship(
                sourceNodeReference,
                relationship.OtherNode,
                relationship.RelationshipTypeKey,
                relationship.Data);
        }

        RelationshipReference CreateRelationship(NodeReference sourceNode, NodeReference targetNode, string relationshipTypeKey, object data)
        {
            var relationship = new RelationshipTemplate
                {
                    To = RootUri + ResolveEndpoint(targetNode),
                    Data = data,
                    Type = relationshipTypeKey
                };

            var sourceNodeEndpoint = ResolveEndpoint(sourceNode) + "/relationships";
            var response = SendHttpRequest(
                HttpPostAsJson(sourceNodeEndpoint, relationship),
                HttpStatusCode.Created, HttpStatusCode.NotFound);

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApplicationException(string.Format(
                    "One of the nodes referenced in the relationship could not be found. Referenced nodes were {0} and {1}.",
                    sourceNode.Id,
                    targetNode.Id));

            return response
                .Content
                .ReadAsJson<RelationshipApiResponse<object>>(JsonConverters,JsonContractResolver)
                .ToRelationshipReference(this);
        }

        CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer { JsonConverters = JsonConverters, JsonContractResolver = JsonContractResolver };
        }

        public void DeleteRelationship(RelationshipReference reference)
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var relationshipEndpoint = ResolveEndpoint(reference);
            var response = SendHttpRequest(
                HttpDelete(relationshipEndpoint),
                HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApplicationException(string.Format(
                    "Unable to delete the relationship. The response status was: {0} {1}",
                    (int) response.StatusCode,
                    response.ReasonPhrase));

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
            Task.WaitAll(task);
            return task.Result;
        }

        public virtual Task<Node<TNode>> GetAsync<TNode>(NodeReference reference)
        {
            CheckRoot();

            var nodeEndpoint = ResolveEndpoint(reference);
            return
                SendHttpRequestAsync(HttpGet(nodeEndpoint), HttpStatusCode.OK, HttpStatusCode.NotFound)
                .ContinueWith(responseTask =>
                {
                    var response = responseTask.Result;

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return (Node<TNode>)null;

                    return response
                        .Content
                        .ReadAsJson<NodeApiResponse<TNode>>(JsonConverters,JsonContractResolver)
                        .ToNode(this);
                });
        }

        public virtual Node<TNode> Get<TNode>(NodeReference<TNode> reference)
        {
            return Get<TNode>((NodeReference) reference);
        }

        public virtual RelationshipInstance<TData> Get<TData>(RelationshipReference<TData> reference) where TData : class, new()
        {
            return Get<TData>((RelationshipReference)reference);
        }

        public virtual RelationshipInstance<TData> Get<TData>(RelationshipReference reference) where TData : class, new()
        {
            var task = GetAsync<TData>(reference);
            Task.WaitAll(task);
            return task.Result;
        }

        public virtual Task<RelationshipInstance<TData>> GetAsync<TData>(RelationshipReference reference) where TData : class, new()
        {
            CheckRoot();

            var endpoint = ResolveEndpoint(reference);
            return
                SendHttpRequestAsync(HttpGet(endpoint), HttpStatusCode.OK, HttpStatusCode.NotFound)
                .ContinueWith(responseTask =>
                {
                    var response = responseTask.Result;

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return (RelationshipInstance<TData>)null;

                    return response
                        .Content
                        .ReadAsJson<RelationshipApiResponse<TData>>(JsonConverters,JsonContractResolver)
                        .ToRelationshipInstance(this);
                });
        }

        public void Update<TNode>(NodeReference<TNode> nodeReference, TNode replacementData, IEnumerable<IndexEntry> indexEntries = null)
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var allIndexEntries = indexEntries == null
                ? new IndexEntry[0]
                : indexEntries.ToArray();

            var nodePropertiesEndpoint = ResolveEndpoint(nodeReference) + "/properties";
            SendHttpRequest(
                HttpPutAsJson(nodePropertiesEndpoint, replacementData),
                HttpStatusCode.NoContent);

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

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var node = Get(nodeReference);

            var indexEntries = new IndexEntry[] {};

            if (indexEntriesCallback != null)
            {
                indexEntries = indexEntriesCallback(node.Data).ToArray();
            }

            var serializer = BuildSerializer();

            var originalValuesString = changeCallback == null ? null : serializer.Serialize(node.Data);

            updateCallback(node.Data);

            if (changeCallback != null)
            {
                var originalValuesDictionary = new CustomJsonDeserializer(JsonConverters,resolver:JsonContractResolver).Deserialize<Dictionary<string, string>>(originalValuesString);
                var newValuesString = serializer.Serialize(node.Data);
                var newValuesDictionary = new CustomJsonDeserializer(JsonConverters, resolver: JsonContractResolver).Deserialize<Dictionary<string, string>>(newValuesString);
                var differences = Utilities.GetDifferencesBetweenDictionaries(originalValuesDictionary, newValuesDictionary);
                changeCallback(differences);
            }

            var nodePropertiesEndpoint = ResolveEndpoint(nodeReference) + "/properties";
            SendHttpRequest(
                HttpPutAsJson(nodePropertiesEndpoint, node.Data),
                HttpStatusCode.NoContent);

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

        public void Update<TRelationshipData>(RelationshipReference<TRelationshipData> relationshipReference, Action<TRelationshipData> updateCallback)
            where TRelationshipData : class, new()
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var propertiesEndpoint = ResolveEndpoint(relationshipReference) + "/properties";

            var currentData = SendHttpRequestAndParseResultAs<TRelationshipData>(
                HttpGet(propertiesEndpoint),
                HttpStatusCode.OK, HttpStatusCode.NoContent);

            var payload = currentData ?? new TRelationshipData();
            updateCallback(payload);

            SendHttpRequest(
                HttpPutAsJson(propertiesEndpoint, payload),
                HttpStatusCode.NoContent);

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

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (mode == DeleteMode.NodeAndRelationships)
            {
                DeleteAllRelationships(reference);
            }

            var nodeEndpoint = ResolveEndpoint(reference);
            var response = SendHttpRequest(
                HttpDelete(nodeEndpoint),
                HttpStatusCode.NoContent, HttpStatusCode.Conflict);

            if (response.StatusCode == HttpStatusCode.Conflict)
                throw new ApplicationException(string.Format(
                    "Unable to delete the node. The node may still have relationships. The response status was: {0} {1}",
                    (int) response.StatusCode,
                    response.ReasonPhrase));

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = "Delete " + reference.Id,
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        void DeleteAllRelationships(NodeReference reference)
        {
            //TODO: Make this a dynamic endpoint resolution
            var relationshipsEndpoint = ResolveEndpoint(reference) + "/relationships/all";
            var result = SendHttpRequestAndParseResultAs<List<RelationshipApiResponse<object>>>(
                HttpGet(relationshipsEndpoint),
                HttpStatusCode.OK);

            var relationshipResources = result
                .Select(r => r.Self.Substring(RootUri.AbsoluteUri.Length));

            foreach (var relationshipResource in relationshipResources)
                SendHttpRequest(
                    HttpDelete(relationshipResource),
                    HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        string ResolveEndpoint(NodeReference node)
        {
            return RootApiResponse.Node + "/" + node.Id;
        }

        string ResolveEndpoint(RelationshipReference relationship)
        {
            return RootApiResponse.Relationship + "/" + relationship.Id;
        }

        static string GetLastPathSegment(string uri)
        {
            var path = new Uri(uri).AbsolutePath;
            return path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();
        }

        public ICypherFluentQuery Cypher
        {
            get {return new CypherFluentQuery(this); }
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
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

        public List<JsonConverter> JsonConverters { get; private set; }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public virtual string ExecuteScalarGremlin(string query, IDictionary<string, object> parameters)
        {
            CheckRoot();

            if (RootApiResponse.Extensions.GremlinPlugin == null ||
                RootApiResponse.Extensions.GremlinPlugin.ExecuteScript == null)
                throw new ApplicationException(GremlinPluginUnavailable);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = SendHttpRequest(
                HttpPostAsJson(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript, new GremlinApiQuery(query, parameters)),
                string.Format("The query was: {0}", query),
                HttpStatusCode.OK);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query,
                ResourcesReturned = 1,
                TimeTaken = stopwatch.Elapsed
            });

            return response.Content.ReadAsString();
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public virtual IEnumerable<TResult> ExecuteGetAllProjectionsGremlin<TResult>(IGremlinQuery query) where TResult : new()
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = SendHttpRequestAndParseResultAs<List<List<GremlinTableCapResponse>>>(
                HttpPostAsJson(
                    RootApiResponse.Extensions.GremlinPlugin.ExecuteScript,
                    new GremlinApiQuery(query.QueryText, query.QueryParameters)),
                string.Format("The query was: {0}", query.QueryText),
                HttpStatusCode.OK);

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

        public CypherCapabilities CypherCapabilities
        {
            get { return cypherCapabilities; }
        }

        [Obsolete("This method is for use by the framework internally. Use IGraphClient.Cypher instead, and read the documentation at https://bitbucket.org/Readify/neo4jclient/wiki/cypher. If you really really want to call this method directly, and you accept the fact that YOU WILL LIKELY INTRODUCE A RUNTIME SECURITY RISK if you do so, then it shouldn't take you too long to find the correct explicit interface implementation that you have to call. This hurdle is for your own protection. You really really should not do it. This signature may be removed or renamed at any time.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IEnumerable<TResult> ExecuteGetCypherResults<TResult>(CypherQuery query)
        {
            throw new NotImplementedException();
        }

        IEnumerable<TResult> IRawGraphClient.ExecuteGetCypherResults<TResult>(CypherQuery query)
        {
            var task = ((IRawGraphClient) this).ExecuteGetCypherResultsAsync<TResult>(query);
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

        Task<IEnumerable<TResult>> IRawGraphClient.ExecuteGetCypherResultsAsync<TResult>(CypherQuery query)
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            return
                SendHttpRequestAsync(
                    HttpPostAsJson(RootApiResponse.Cypher, new CypherApiQuery(query)),
                    string.Format("The query was: {0}", query.QueryText),
                    HttpStatusCode.OK)
                .ContinueWith(responseTask =>
                {
                    var response = responseTask.Result;
                    var deserializer = new CypherJsonDeserializer<TResult>(this, query.ResultMode);
                    var results = deserializer
                        .Deserialize(response.Content.ReadAsString())
                        .ToList();

                    stopwatch.Stop();
                    OnOperationCompleted(new OperationCompletedEventArgs
                    {
                        QueryText = query.DebugQueryText,
                        ResourcesReturned = results.Count(),
                        TimeTaken = stopwatch.Elapsed
                    });

                    return (IEnumerable<TResult>)results;
                });
        }

        void IRawGraphClient.ExecuteCypher(CypherQuery query)
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            SendHttpRequest(
                HttpPostAsJson(RootApiResponse.Cypher, new CypherApiQuery(query)),
                string.Format("The query was: {0}", query.QueryText),
                HttpStatusCode.OK);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query.DebugQueryText,
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        Task IRawGraphClient.ExecuteCypherAsync(CypherQuery query)
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            return SendHttpRequestAsync(
                HttpPostAsJson(RootApiResponse.Cypher, new CypherApiQuery(query)),
                string.Format("The query was: {0}", query.QueryText),
                HttpStatusCode.OK)
                .ContinueWith(t =>
                {
                    // Rethrow any exception (instead of using TaskContinuationOptions.OnlyOnRanToCompletion, which for failures, returns a canceled task instead of a faulted task)
                    var _ = t.Result;
                        
                    stopwatch.Stop();
                    OnOperationCompleted(new OperationCompletedEventArgs
                    {
                        QueryText = query.DebugQueryText,
                        ResourcesReturned = 0,
                        TimeTaken = stopwatch.Elapsed
                    });
                })
            ;
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public virtual IEnumerable<RelationshipInstance> ExecuteGetAllRelationshipsGremlin(string query, IDictionary<string, object> parameters)
        {
            return ExecuteGetAllRelationshipsGremlin<object>(query, parameters);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public virtual IEnumerable<RelationshipInstance<TData>> ExecuteGetAllRelationshipsGremlin<TData>(string query, IDictionary<string, object> parameters)
            where TData : class, new()
        {
            CheckRoot();

            if (RootApiResponse.Extensions.GremlinPlugin == null ||
                RootApiResponse.Extensions.GremlinPlugin.ExecuteScript == null)
                throw new ApplicationException(GremlinPluginUnavailable);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = SendHttpRequestAndParseResultAs<List<RelationshipApiResponse<TData>>>(
                HttpPostAsJson(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript, new GremlinApiQuery(query, parameters)),
                string.Format("The query was: {0}", query),
                HttpStatusCode.OK);

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

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query, IDictionary<string, object> parameters)
        {
            return ExecuteGetAllNodesGremlin<TNode>(new GremlinQuery(this, query, parameters, new List<string>()));
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query, IDictionary<string, object> parameters, IList<string> declarations)
        {
            return ExecuteGetAllNodesGremlin<TNode>(new GremlinQuery(this, query, parameters, declarations));
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(IGremlinQuery query)
        {
            CheckRoot();

            if (RootApiResponse.Extensions.GremlinPlugin == null ||
                RootApiResponse.Extensions.GremlinPlugin.ExecuteScript == null)
                throw new ApplicationException(GremlinPluginUnavailable);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = SendHttpRequestAndParseResultAs<List<NodeApiResponse<TNode>>>(
                HttpPostAsJson(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript, new GremlinApiQuery(query.QueryText, query.QueryParameters)),
                string.Format("The query was: {0}", query.QueryText),
                HttpStatusCode.OK);

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

        public Dictionary<string, IndexMetaData> GetIndexes(IndexFor indexFor)
        {
            CheckRoot();

            string indexResource;
            switch (indexFor)
            {
                case IndexFor.Node:
                    indexResource = RootApiResponse.NodeIndex;
                    break;
                case IndexFor.Relationship:
                    indexResource = RootApiResponse.RelationshipIndex;
                    break;
                default:
                    throw new NotSupportedException(string.Format("GetIndexes does not support indexfor {0}", indexFor));
            }

            var response = SendHttpRequest(
                HttpGet(indexResource),
                HttpStatusCode.OK, HttpStatusCode.NoContent);

            if(response.StatusCode == HttpStatusCode.NoContent)
                return new Dictionary<string, IndexMetaData>();

            var result = response.Content.ReadAsJson<Dictionary<string, IndexMetaData>>(JsonConverters,JsonContractResolver);

            return result;
        }

        public bool CheckIndexExists(string indexName, IndexFor indexFor)
        {
            CheckRoot();

            string indexResource;
            switch (indexFor)
            {
                case IndexFor.Node:
                    indexResource = RootApiResponse.NodeIndex;
                    break;
                case IndexFor.Relationship:
                    indexResource = RootApiResponse.RelationshipIndex;
                    break;
                default:
                    throw new NotSupportedException(string.Format("IndexExists does not support indexfor {0}", indexFor));
            }

            var response = SendHttpRequest(
                HttpGet(string.Format("{0}/{1}",indexResource, indexName)),
                HttpStatusCode.OK, HttpStatusCode.NotFound);

            return response.StatusCode == HttpStatusCode.OK;
        }

        void CheckRoot()
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException(
                    "The graph client is not connected to the server. Call the Connect method first.");
        }

        public void CreateIndex(string indexName, IndexConfiguration config, IndexFor indexFor)
        {
            CheckRoot();

            string indexResource;
            switch (indexFor)
            {
                case IndexFor.Node:
                    indexResource = RootApiResponse.NodeIndex;
                    break;
                case IndexFor.Relationship:
                    indexResource = RootApiResponse.RelationshipIndex;
                    break;
                default:
                    throw new NotSupportedException(string.Format("CreateIndex does not support indexfor {0}", indexFor));
            }

            var createIndexApiRequest = new
            {
                name = indexName,
                config
            };

            SendHttpRequest(
                HttpPostAsJson(indexResource, createIndexApiRequest),
                HttpStatusCode.Created);
        }

        public void ReIndex(NodeReference node, IEnumerable<IndexEntry> indexEntries)
        {
            var entityUri = ResolveEndpoint(node);
            var entityId = node.Id;
            ReIndex(entityUri, entityId, IndexFor.Node, indexEntries);
        }

        public void ReIndex(RelationshipReference relationship, IEnumerable<IndexEntry> indexEntries)
        {
            var entityUri = ResolveEndpoint(relationship);
            var entityId = relationship.Id;
            ReIndex(entityUri, entityId, IndexFor.Relationship, indexEntries);
        }

        public void ReIndex(string entityUri, long entityId, IndexFor indexFor, IEnumerable<IndexEntry> indexEntries)
        {
            if (indexEntries == null)
                throw new ArgumentNullException("indexEntries");

            CheckRoot();

            var updates = indexEntries
                .SelectMany(
                    i => i.KeyValues,
                    (i, kv) => new { IndexName = i.Name, kv.Key, kv.Value })
                .Where(update => update.Value != null)
                .ToList();

            foreach (var indexName in updates.Select(u => u.IndexName).Distinct())
                DeleteIndexEntries(indexName, entityId, indexFor);

            foreach (var update in updates)
                AddIndexEntry(update.IndexName, update.Key, update.Value, entityUri, indexFor);
        }

        public void DeleteIndex(string indexName, IndexFor indexFor)
        {
            CheckRoot();

            string indexResource;
            switch (indexFor)
            {
                case IndexFor.Node:
                    indexResource = RootApiResponse.NodeIndex;
                    break;
                case IndexFor.Relationship:
                    indexResource = RootApiResponse.RelationshipIndex;
                    break;
                default:
                    throw new NotSupportedException(string.Format("DeleteIndex does not support indexfor {0}", indexFor));
            }

            SendHttpRequest(
                HttpDelete(string.Format("{0}/{1}", indexResource, indexName)),
                HttpStatusCode.NoContent);
        }

        public void DeleteIndexEntries(string indexName, NodeReference nodeReference)
        {
            DeleteIndexEntries(indexName, nodeReference.Id, IndexFor.Node);
        }

        public void DeleteIndexEntries(string indexName, RelationshipReference relationshipReference)
        {
            DeleteIndexEntries(indexName, relationshipReference.Id, IndexFor.Relationship);
        }

        void DeleteIndexEntries(string indexName, long id, IndexFor indexFor)
        {
            var indexResponse = indexFor == IndexFor.Node
                ? RootApiResponse.NodeIndex
                : RootApiResponse.RelationshipIndex;

            var indexAddress = string.Join("/", new[]
            {
                indexResponse,
                Uri.EscapeDataString(indexName),
                Uri.EscapeDataString(id.ToString(CultureInfo.InvariantCulture))
            });

            SendHttpRequest(
                HttpDelete(indexAddress),
                string.Format("Deleting entries from index {0} for node {1}", indexName, id),
                HttpStatusCode.NoContent);
        }

        void AddIndexEntry(string indexName, string indexKey, object indexValue, string address, IndexFor indexFor)
        {
            var encodedIndexValue = EncodeIndexValue(indexValue);
            if (string.IsNullOrWhiteSpace(encodedIndexValue))
                return;

            var indexAddress = BuildIndexAddress(indexName, indexFor);

            var indexEntry = new
            {
                key = indexKey,
                value = encodedIndexValue,
                uri = RootUri + address
            };

            SendHttpRequest(
                HttpPostAsJson(indexAddress, indexEntry),
                string.Format("Adding '{0}'='{1}' to index {2} for {3}", indexKey, indexValue, indexName, address),
                HttpStatusCode.Created);
        }

        string BuildIndexAddress(string indexName, IndexFor indexFor)
        {
            var indexResponse = indexFor == IndexFor.Node
                ? RootApiResponse.NodeIndex
                : RootApiResponse.RelationshipIndex;

            var indexAddress = string.Join("/", new[]
            {
                indexResponse,
                Uri.EscapeDataString(indexName)
            });
            return indexAddress;
        }

        static string EncodeIndexValue(object value)
        {
            string indexValue;
            if (value is DateTimeOffset)
            {
                indexValue = ((DateTimeOffset) value).UtcTicks.ToString(CultureInfo.InvariantCulture);
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
        [Obsolete("There are encoding issues with this method. You should use the newer Cypher approach instead. See https://bitbucket.org/Readify/neo4jclient/issue/54/spaces-in-search-text-while-searching-for for an explanation of the problem, and https://bitbucket.org/Readify/neo4jclient/wiki/cypher for documentation about doing index queries with Cypher.")]
        public IEnumerable<Node<TNode>> QueryIndex<TNode>(string indexName, IndexFor indexFor, string query)
        {
            CheckRoot();

            string indexResource;
            switch (indexFor)
            {
                case IndexFor.Node:
                    indexResource = RootApiResponse.NodeIndex;
                    break;
                case IndexFor.Relationship:
                    indexResource = RootApiResponse.RelationshipIndex;
                    break;
                default:
                    throw new NotSupportedException(string.Format("QueryIndex does not support indexfor {0}", indexFor));
            }

            indexResource = string.Format("{0}/{1}?query={2}", indexResource, indexName, Uri.EscapeDataString(query));
            var response = SendHttpRequest(
                HttpGet(indexResource),
                HttpStatusCode.OK);

            var data = new CustomJsonDeserializer(JsonConverters, resolver: JsonContractResolver).Deserialize<List<NodeApiResponse<TNode>>>(response.Content.ReadAsString());

            return data == null
                ? Enumerable.Empty<Node<TNode>>()
                : data.Select(r => r.ToNode(this));
        }

        public IEnumerable<Node<TNode>> LookupIndex<TNode>(string exactIndexName, IndexFor indexFor, string indexKey, long id)
        {
            return BuildLookupIndex<TNode>(exactIndexName, indexFor, indexKey, id.ToString(CultureInfo.InvariantCulture));
        }

        public IEnumerable<Node<TNode>> LookupIndex<TNode>(string exactIndexName, IndexFor indexFor, string indexKey, int id)
        {
            return BuildLookupIndex<TNode>(exactIndexName, indexFor, indexKey, id.ToString(CultureInfo.InvariantCulture));
        }

        IEnumerable<Node<TNode>> BuildLookupIndex<TNode>(string exactIndexName, IndexFor indexFor, string indexKey, string id)
        {
            CheckRoot();

            string indexResource;
            switch (indexFor)
            {
                case IndexFor.Node:
                    indexResource = RootApiResponse.NodeIndex;
                    break;
                case IndexFor.Relationship:
                    indexResource = RootApiResponse.RelationshipIndex;
                    break;
                default:
                    throw new NotSupportedException(string.Format("LookupIndex does not support indexfor {0}", indexFor));
            }

            indexResource = string.Format("{0}/{1}/{2}/{3}", indexResource, exactIndexName, indexKey, id);
            var response = SendHttpRequest(
                HttpGet(indexResource),
                HttpStatusCode.OK);

            var data = new CustomJsonDeserializer(JsonConverters, resolver: JsonContractResolver).Deserialize<List<NodeApiResponse<TNode>>>(response.Content.ReadAsString());

            return data == null
                ? Enumerable.Empty<Node<TNode>>()
                : data.Select(r => r.ToNode(this));
        }

        [Obsolete("This method depends on Cypher, which is being dropped in Neo4j 2.0. Find an alternate strategy for server lifetime management.")]
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
                    throw new ApplicationException(string.Format("Response from Neo4J: {0}", createResponse.Body));

                throw new NeoException(exceptionResponse);
            }
        }

        public DefaultContractResolver JsonContractResolver { get; set; }
    }
}
