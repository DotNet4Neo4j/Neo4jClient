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
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.Cypher;
using Neo4jClient.Deserializer;
using Neo4jClient.Gremlin;
using Neo4jClient.Serializer;

namespace Neo4jClient
{
    public class GraphClient : IRawGraphClient
    {
        internal readonly Uri RootUri;
        readonly IHttpClient httpClient;
        internal RootApiResponse RootApiResponse;
        RootNode rootNode;
        bool jsonStreamingAvailable;
        readonly string userAgent;

        const string IndexRestApiVersionCompatMessage = "The REST indexing API was changed in neo4j 1.5M02. This version of Neo4jClient is only compatible with the new API call. You need to either a) upgrade your neo4j install to 1.5M02 or above (preferred), or b) downgrade your Neo4jClient library to 1.0.0.203 or below.";

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
            return response.Content == null ? default(T) : response.Content.ReadAsJson<T>();
        }

        public virtual void Connect()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = SendHttpRequestAndParseResultAs<RootApiResponse>(
                HttpGet(""),
                HttpStatusCode.OK);

            RootApiResponse = result;
            RootApiResponse.Batch = RootApiResponse.Batch.Substring(RootUri.AbsoluteUri.Length);
            RootApiResponse.Node = RootApiResponse.Node.Substring(RootUri.AbsoluteUri.Length);
            RootApiResponse.NodeIndex = RootApiResponse.NodeIndex.Substring(RootUri.AbsoluteUri.Length);
            RootApiResponse.RelationshipIndex = RootApiResponse.RelationshipIndex.Substring(RootUri.AbsoluteUri.Length);
            RootApiResponse.ExtensionsInfo = RootApiResponse.ExtensionsInfo.Substring(RootUri.AbsoluteUri.Length);
            if (RootApiResponse.Extensions != null && RootApiResponse.Extensions.GremlinPlugin != null)
            {
                RootApiResponse.Extensions.GremlinPlugin.ExecuteScript =
                    RootApiResponse.Extensions.GremlinPlugin.ExecuteScript.Substring(RootUri.AbsoluteUri.Length);
            }

            if (RootApiResponse.Cypher != null)
            {
                RootApiResponse.Cypher =
                    RootApiResponse.Cypher.Substring(RootUri.AbsoluteUri.Length);
            }

            rootNode = string.IsNullOrEmpty(RootApiResponse.ReferenceNode)
                ? null
                : new RootNode(int.Parse(GetLastPathSegment(RootApiResponse.ReferenceNode)), this);

            // http://blog.neo4j.org/2012/04/streaming-rest-api-interview-with.html
            jsonStreamingAvailable = RootApiResponse.Version >= new Version(1, 8);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = "Connect",
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

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

            if (indexEntries.Any())
                AssertMinimumDatabaseVersion(new Version(1, 5, 0, 2), IndexRestApiVersionCompatMessage);

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
                        IndexAddress = BuildNodeIndexAddress(i.Name),
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
            var nodeId = int.Parse(GetLastPathSegment(createResponse.Location));
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

// ReSharper disable UnusedParameter.Local
        void AssertMinimumDatabaseVersion(Version minimumVersion, string message)
// ReSharper restore UnusedParameter.Local
        {
            if (RootApiResponse.Version < minimumVersion)
                throw new NotSupportedException(message);
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
                .ReadAsJson<RelationshipApiResponse<object>>()
                .ToRelationshipReference(this);
        }

        static CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer();
        }

        public void DeleteRelationship(RelationshipReference reference)
        {
            CheckRoot();

            var relationshipEndpoint = ResolveEndpoint(reference);
            var response = SendHttpRequest(
                HttpDelete(relationshipEndpoint),
                HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApplicationException(string.Format(
                    "Unable to delete the relationship. The response status was: {0} {1}",
                    (int) response.StatusCode,
                    response.ReasonPhrase));
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
                        .ReadAsJson<NodeApiResponse<TNode>>()
                        .ToNode(this);
                });
        }

        public virtual Node<TNode> Get<TNode>(NodeReference<TNode> reference)
        {
            return Get<TNode>((NodeReference) reference);
        }

        public void Update<TNode>(NodeReference<TNode> nodeReference, Action<TNode> updateCallback,
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
                if (indexEntries.Any())
                    AssertMinimumDatabaseVersion(new Version(1, 5, 0, 2), IndexRestApiVersionCompatMessage);
            }

            var serializer = BuildSerializer();

            var originalValuesString = changeCallback == null ? null : serializer.Serialize(node.Data);

            updateCallback(node.Data);

            if (changeCallback != null)
            {
                var originalValuesDictionary = new CustomJsonDeserializer().Deserialize<Dictionary<string, string>>(originalValuesString);
                var newValuesString = serializer.Serialize(node.Data);
                var newValuesDictionary = new CustomJsonDeserializer().Deserialize<Dictionary<string, string>>(newValuesString);
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

        static string ResolveEndpoint(RelationshipReference relationship)
        {
            //TODO: Make this a dynamic endpoint resolution
            return "relationship/" + relationship.Id;
        }

        static string GetLastPathSegment(string uri)
        {
            var path = new Uri(uri).AbsolutePath;
            return path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();
        }

        public ICypherFluentQueryPreStart Cypher
        {
            get {return new CypherFluentQuery(this); }
        }

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

        public virtual string ExecuteScalarGremlin(string query, IDictionary<string, object> parameters)
        {
            CheckRoot();

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

            var results = GremlinTableCapResponse.TransferResponseToResult<TResult>(responses);

            return results;
        }

        [Obsolete("This method is for use by the framework internally. You should really be using GraphClient.Cypher instead. If you really really want to use this method, you'll have to access it via an explicit interface implementation on IRawGraphClient instead. This hurdle is for your own protection.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IEnumerable<TResult> ExecuteGetCypherResults<TResult>(CypherQuery query)
        {
            throw new NotImplementedException();
        }

        IEnumerable<TResult> IRawGraphClient.ExecuteGetCypherResults<TResult>(CypherQuery query)
        {
            var task = ((IRawGraphClient) this).ExecuteGetCypherResultsAsync<TResult>(query);
            Task.WaitAll(task);
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
                        QueryText = query.QueryText,
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
                QueryText = query.QueryText,
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        public virtual IEnumerable<RelationshipInstance> ExecuteGetAllRelationshipsGremlin(string query, IDictionary<string, object> parameters)
        {
            return ExecuteGetAllRelationshipsGremlin<object>(query, parameters);
        }

        public virtual IEnumerable<RelationshipInstance<TData>> ExecuteGetAllRelationshipsGremlin<TData>(string query, IDictionary<string, object> parameters)
            where TData : class, new()
        {
            CheckRoot();

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

        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query, IDictionary<string, object> parameters)
        {
            return ExecuteGetAllNodesGremlin<TNode>(new GremlinQuery(this, query, parameters, new List<string>()));
        }

        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query, IDictionary<string, object> parameters, IList<string> declarations)
        {
            return ExecuteGetAllNodesGremlin<TNode>(new GremlinQuery(this, query, parameters, declarations));
        }

        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(IGremlinQuery query)
        {
            CheckRoot();

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

            var result = response.Content.ReadAsJson<Dictionary<string, IndexMetaData>>();

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
            if (indexEntries == null)
                throw new ArgumentNullException("indexEntries");

            AssertMinimumDatabaseVersion(new Version(1, 5, 0, 2), IndexRestApiVersionCompatMessage);

            CheckRoot();

            var nodeAddress = string.Join("/", new[] { RootApiResponse.Node, node.Id.ToString(CultureInfo.InvariantCulture) });

            var updates = indexEntries
                .SelectMany(
                    i => i.KeyValues,
                    (i, kv) => new {IndexName = i.Name, kv.Key, kv.Value})
                .Where(update => update.Value != null)
                .ToList();

            foreach(var indexName in updates.Select(u => u.IndexName).Distinct())
            {
                DeleteIndexEntries(indexName,node.Id);
            }

            foreach (var update in updates)
            {
                AddIndexEntry(update.IndexName, update.Key, update.Value, nodeAddress);
            }
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

        public void DeleteIndexEntries(string indexName, long nodeId)
        {
            var nodeIndexAddress = string.Join("/", new[]
            {
                RootApiResponse.NodeIndex,
                Uri.EscapeDataString(indexName),
                Uri.EscapeDataString(nodeId.ToString(CultureInfo.InvariantCulture))
            });

            SendHttpRequest(
                HttpDelete(nodeIndexAddress),
                string.Format("Deleting entries from index {0} for node {1}", indexName, nodeId),
                HttpStatusCode.NoContent);
        }

        void AddIndexEntry(string indexName, string indexKey, object indexValue, string nodeAddress)
        {
            var encodedIndexValue = EncodeIndexValue(indexValue);
            if (string.IsNullOrWhiteSpace(encodedIndexValue))
                return;

            var nodeIndexAddress = BuildNodeIndexAddress(indexName);

            var indexEntry = new
            {
                key = indexKey,
                value = encodedIndexValue,
                uri = string.Join("", RootUri, nodeAddress)
            };

            SendHttpRequest(
                HttpPostAsJson(nodeIndexAddress, indexEntry),
                string.Format("Adding '{0}'='{1}' to index {2} for {3}", indexKey, indexValue, indexName, nodeAddress),
                HttpStatusCode.Created);
        }

        string BuildNodeIndexAddress(string indexName)
        {
            var nodeIndexAddress = string.Join("/", new[]
            {
                RootApiResponse.NodeIndex,
                Uri.EscapeDataString(indexName)
            });
            return nodeIndexAddress;
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

            var data = new CustomJsonDeserializer().Deserialize<List<NodeApiResponse<TNode>>>(response.Content.ReadAsString());

            return data == null
                ? Enumerable.Empty<Node<TNode>>()
                : data.Select(r => r.ToNode(this));
        }

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
    }
}
