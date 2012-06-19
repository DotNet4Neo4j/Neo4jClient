using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.Cypher;
using Neo4jClient.Deserializer;
using Neo4jClient.Gremlin;
using Neo4jClient.Serializer;
using Newtonsoft.Json;
using RestSharp;

namespace Neo4jClient
{
    public class GraphClient : IGraphClient
    {
        internal readonly Uri RootUri;
        readonly IHttpFactory httpFactory;
        internal RootApiResponse RootApiResponse;
        internal readonly IAuthenticator Authenticator;
        bool jsonStreamingAvailable;

        const string IndexRestApiVersionCompatMessage = "The REST indexing API was changed in neo4j 1.5M02. This version of Neo4jClient is only compatible with the new API call. You need to either a) upgrade your neo4j install to 1.5M02 or above (preferred), or b) downgrade your Neo4jClient library to 1.0.0.203 or below.";

        public NullValueHandling JsonSerializerNullValueHandling { get; set; }
        public bool UseJsonStreamingIfAvailable { get; set; }
        public bool EnableSupportForNeo4jOnHeroku { get; set; }

        public GraphClient(Uri rootUri)
            : this(rootUri, new Http())
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.UseNagleAlgorithm = false;
        }

        public GraphClient(Uri rootUri, bool expect100Continue, bool useNagleAlgorithm)
            : this(rootUri, new Http())
        {
            ServicePointManager.Expect100Continue = expect100Continue;
            ServicePointManager.UseNagleAlgorithm = useNagleAlgorithm;
        }

        public GraphClient(Uri rootUri, IHttpFactory httpFactory)
        {
            if(!string.IsNullOrWhiteSpace(rootUri.UserInfo))
            {
                string[] userInfoParts = rootUri.UserInfo.Split(':');
                string userInfoContent = rootUri.UserInfo + "@";

                if(userInfoParts.Length == 2 && rootUri.OriginalString.Contains(userInfoContent))
                {
                    this.Authenticator = new HttpBasicAuthenticator(userInfoParts[0], userInfoParts[1]);
                    rootUri = new Uri(rootUri.OriginalString.Replace(userInfoContent, ""));
                }
            }

            this.RootUri = rootUri;
            this.httpFactory = httpFactory;
            JsonSerializerNullValueHandling = NullValueHandling.Ignore;
            UseJsonStreamingIfAvailable = true;
        }

        IRestClient CreateClient()
        {
            var client = new RestClient(RootUri.AbsoluteUri) {HttpFactory = httpFactory};

            if(this.Authenticator != null)
            {
                client.Authenticator = this.Authenticator;
            }
            
            client.RemoveHandler("application/json");
            client.AddHandler("application/json", new CustomJsonDeserializer());
            if (UseJsonStreamingIfAvailable && jsonStreamingAvailable) client.AddDefaultHeader("Accept", "application/json;stream=true");
            return client;
        }

        public virtual void Connect()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //HACK: temporary solution to issue: http://goo.gl/oCKsq
            var request = new RestRequest(EnableSupportForNeo4jOnHeroku ? "/ " : "", Method.GET);
            var response = CreateClient().Execute<RootApiResponse>(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.OK);

            RootApiResponse = response.Data;
            RootApiResponse.Batch = RootApiResponse.Batch.Substring(RootUri.AbsoluteUri.Length);
            RootApiResponse.Node = RootApiResponse.Node.Substring(RootUri.AbsoluteUri.Length);
            RootApiResponse.NodeIndex = RootApiResponse.NodeIndex.Substring(RootUri.AbsoluteUri.Length);
            RootApiResponse.RelationshipIndex = RootApiResponse.RelationshipIndex.Substring(RootUri.AbsoluteUri.Length);
            RootApiResponse.ReferenceNode = RootApiResponse.ReferenceNode.Substring(RootUri.AbsoluteUri.Length);
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
            get { return new RootNode(this); }
        }

        public virtual NodeReference<TNode> Create<TNode>(
            TNode node,
            IEnumerable<IRelationshipAllowingParticipantNode<TNode>> relationships,
            IEnumerable<IndexEntry> indexEntries)
            where TNode : class
        {
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

            var createNodeStep = batchSteps.Add(Method.POST, "/node", node);

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
                batchSteps.Add(Method.POST, sourceNode + "/relationships", relationshipTemplate);
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
                batchSteps.Add(Method.POST, indexEntry.IndexAddress, new
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
            var request = new RestRequest(RootApiResponse.Batch, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(batchSteps);
            var response = CreateClient().Execute<BatchResponse>(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.OK);

            return response.Data;
        }

        public virtual void CreateRelationship<TSourceNode, TRelationship>(
            NodeReference<TSourceNode> sourceNodeReference,
            TRelationship relationship)
            where TRelationship :
                Relationship,
                IRelationshipAllowingSourceNode<TSourceNode>
        {
            if (relationship.Direction == RelationshipDirection.Incoming)
                throw new NotSupportedException("Incoming relationships are not yet supported by this method.");

            CreateRelationship(
                sourceNodeReference,
                relationship.OtherNode,
                relationship.RelationshipTypeKey,
                relationship.Data);
        }

        void CreateRelationship(NodeReference sourceNode, NodeReference targetNode, string relationshipTypeKey,
                                object data)
        {
            var relationship = new RelationshipTemplate
                {
                    To = RootUri + ResolveEndpoint(targetNode),
                    Data = data,
                    Type = relationshipTypeKey
                };

            var sourceNodeEndpoint = ResolveEndpoint(sourceNode) + "/relationships";
            var request = new RestRequest(sourceNodeEndpoint, Method.POST)
                {
                    RequestFormat = DataFormat.Json,
                    JsonSerializer = new CustomJsonSerializer {NullHandling = JsonSerializerNullValueHandling}
                };
            request.AddBody(relationship);
            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.Created, HttpStatusCode.NotFound);

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApplicationException(string.Format(
                    "One of the nodes referenced in the relationship could not be found. Referenced nodes were {0} and {1}.",
                    sourceNode.Id,
                    targetNode.Id));
        }

        public void DeleteRelationship(RelationshipReference reference)
        {
            CheckRoot();

            var relationshipEndpoint = ResolveEndpoint(reference);
            var request = new RestRequest(relationshipEndpoint, Method.DELETE);
            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApplicationException(string.Format(
                    "Unable to delete the relationship. The response status was: {0} {1}",
                    (int) response.StatusCode,
                    response.StatusDescription));
        }

        public virtual Node<TNode> Get<TNode>(NodeReference reference)
        {
            CheckRoot();

            var nodeEndpoint = ResolveEndpoint(reference);
            var request = new RestRequest(nodeEndpoint, Method.GET);
            var response = CreateClient().Execute<NodeApiResponse<TNode>>(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.OK, HttpStatusCode.NotFound);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return response.Data.ToNode(this);
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

            var serializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling };

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

            var nodeEndpoint = ResolveEndpoint(nodeReference);
            var request = new RestRequest(nodeEndpoint + "/properties", Method.PUT)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(node.Data);
            var response = CreateClient().Execute(request);

            if (indexEntriesCallback != null)
            {
                ReIndex(node.Reference, indexEntries);
            }

            ValidateExpectedResponseCodes(response, HttpStatusCode.NoContent);

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

            var getRequest = new RestRequest(propertiesEndpoint, Method.GET);
            var getResponse = CreateClient().Execute<TRelationshipData>(getRequest);
            ValidateExpectedResponseCodes(getResponse, HttpStatusCode.OK, HttpStatusCode.NoContent);

            var payload = getResponse.Data ?? new TRelationshipData();
            updateCallback(payload);

            var updateRequest = new RestRequest(propertiesEndpoint, Method.PUT)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            updateRequest.AddBody(payload);
            var updateResponse = CreateClient().Execute(updateRequest);
            ValidateExpectedResponseCodes(updateResponse, HttpStatusCode.NoContent);

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
            var request = new RestRequest(nodeEndpoint, Method.DELETE);
            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.NoContent, HttpStatusCode.Conflict);

            if (response.StatusCode == HttpStatusCode.Conflict)
                throw new ApplicationException(string.Format(
                    "Unable to delete the node. The node may still have relationships. The response status was: {0} {1}",
                    (int) response.StatusCode,
                    response.StatusDescription));

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
            var request = new RestRequest(relationshipsEndpoint, Method.GET);
            var response = CreateClient().Execute<List<RelationshipApiResponse<object>>>(request);

            var relationshipResources = response
                .Data
                .Select(r => r.Self.Substring(RootUri.AbsoluteUri.Length));

            foreach (var relationshipResource in relationshipResources)
            {
                request = new RestRequest(relationshipResource, Method.DELETE);
                CreateClient().Execute(request);
            }
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

        public virtual string ExecuteScalarGremlin(string query, IDictionary<string, object> parameters)
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var request = new RestRequest(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(new GremlinApiQuery(query, parameters));
            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(
                response,
                string.Format("The query was: {0}", query),
                HttpStatusCode.OK);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query,
                ResourcesReturned = 1,
                TimeTaken = stopwatch.Elapsed
            });

            return response.Content;
        }

        public virtual IEnumerable<TResult> ExecuteGetAllProjectionsGremlin<TResult>(IGremlinQuery query) where TResult : new()
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var request = new RestRequest(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(new GremlinApiQuery(query.QueryText, query.QueryParameters));
            var response = CreateClient().Execute<List<List<GremlinTableCapResponse>>>(request);

            ValidateExpectedResponseCodes(
                response,
                string.Format("The query was: {0}", query.QueryText),
                HttpStatusCode.OK);

            var responses = response.Data ?? new List<List<GremlinTableCapResponse>> { new List<GremlinTableCapResponse>() };

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

        public virtual IEnumerable<TResult> ExecuteGetCypherResults<TResult>(CypherQuery query)
        {
            CheckRoot();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var request = new RestRequest(RootApiResponse.Cypher, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(new CypherApiQuery(query));
            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(
                response,
                string.Format("The query was: {0}", query.QueryText),
                HttpStatusCode.OK);

            var deserializer = new CypherJsonDeserializer<TResult>(this, query.ResultMode);
            var results = deserializer
                .Deserialize(response)
                .ToList();

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query.QueryText,
                ResourcesReturned = results.Count(),
                TimeTaken = stopwatch.Elapsed
            });

            return results;
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

            var request = new RestRequest(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(new GremlinApiQuery(query, parameters));
            var response = CreateClient().Execute<List<RelationshipApiResponse<TData>>>(request);

            ValidateExpectedResponseCodes(
                response,
                string.Format("The query was: {0}", query),
                HttpStatusCode.OK);

            var relationships = response.Data == null
                ? new RelationshipInstance<TData>[0]
                : response.Data.Select(r => r.ToRelationshipInstance(this)).ToArray();

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

            var request = new RestRequest(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(new GremlinApiQuery(query.QueryText, query.QueryParameters));
            var response = CreateClient().Execute<List<NodeApiResponse<TNode>>>(request);

            ValidateExpectedResponseCodes(
                response,
                string.Format("The query was: {0}", query.QueryText),
                HttpStatusCode.OK);

            var nodes = response.Data == null
                ? new Node<TNode>[0]
                : response.Data.Select(r => r.ToNode(this)).ToArray();

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

            var request = new RestRequest(indexResource, Method.GET)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };

            var response =  CreateClient().Execute<Dictionary<string, IndexMetaData>>(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.OK);

            return response.Data;
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

            var request = new RestRequest(string.Format("{0}/{1}",indexResource, indexName), Method.GET)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };

            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.OK, HttpStatusCode.NotFound);

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

            string nodeResource;
            switch (indexFor)
            {
                case IndexFor.Node:
                    nodeResource = RootApiResponse.NodeIndex;
                    break;
                case IndexFor.Relationship:
                    nodeResource = RootApiResponse.RelationshipIndex;
                    break;
                default:
                    throw new NotSupportedException(string.Format("CreateIndex does not support indexfor {0}", indexFor));
            }

            var createIndexApiRequest = new
                {
                    name = indexName,
                    config
                };

            var request = new RestRequest(nodeResource, Method.POST)
                {
                    RequestFormat = DataFormat.Json,
                    JsonSerializer = new CustomJsonSerializer {NullHandling = JsonSerializerNullValueHandling}
                };
            request.AddBody(createIndexApiRequest);

            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.Created);
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

            var request = new RestRequest(string.Format("{0}/{1}", indexResource, indexName), Method.DELETE)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };

            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.NoContent);
        }

        void DeleteIndexEntries(string indexName, long nodeId)
        {
            var nodeIndexAddress = string.Join("/", new[]
            {
                RootApiResponse.NodeIndex,
                Uri.EscapeDataString(indexName),
                Uri.EscapeDataString(nodeId.ToString(CultureInfo.InvariantCulture))
            });
            var request = new RestRequest(nodeIndexAddress, Method.DELETE)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };

            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(
                response,
                string.Format(
                    "Deleting entries from index {0} for node {1} by DELETing to {2}.",
                    indexName,
                    nodeId,
                    nodeIndexAddress
                ),
                HttpStatusCode.NoContent);
        }

        void AddIndexEntry(string indexName, string indexKey, object indexValue, string nodeAddress)
        {
            var encodedIndexValue = EncodeIndexValue(indexValue);
            if (string.IsNullOrWhiteSpace(encodedIndexValue))
                return;

            var nodeIndexAddress = BuildNodeIndexAddress(indexName);

            var request = new RestRequest(nodeIndexAddress, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(new
            {
                key = indexKey,
                value = encodedIndexValue,
                uri = string.Join("", RootUri, nodeAddress)
            });

            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(
                response,
                string.Format(
                    "Adding '{0}'='{1}' to index {2} for {3} by POSTing to {4}.",
                    indexKey,
                    indexValue,
                    indexName,
                    nodeAddress,
                    nodeIndexAddress
                ),
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

            var request = new RestRequest(indexResource + "/" + indexName, Method.GET)
                {
                    RequestFormat = DataFormat.Json,
                    JsonSerializer = new CustomJsonSerializer {NullHandling = JsonSerializerNullValueHandling}
                };

            request.AddParameter("query", query);

            var response = CreateClient().Execute(request);

            ValidateExpectedResponseCodes(response, HttpStatusCode.OK);

            var data = new CustomJsonDeserializer().Deserialize<List<NodeApiResponse<TNode>>>(response);

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

        static void ValidateExpectedResponseCodes(RestResponseBase response, params HttpStatusCode[] allowedStatusCodes)
        {
            ValidateExpectedResponseCodes(response, null, allowedStatusCodes);
        }

// ReSharper disable UnusedParameter.Local
        static void ValidateExpectedResponseCodes(RestResponseBase response, string commandDescription, params HttpStatusCode[] allowedStatusCodes)
// ReSharper restore UnusedParameter.Local
        {
            commandDescription = string.IsNullOrWhiteSpace(commandDescription)
                ? ""
                : commandDescription + "\r\n\r\n";

            var rawBody = response.RawBytes == null || response.RawBytes.Length == 0
                ? string.Empty
                : string.Format("\r\n\r\nThe raw response body was: {0}", Encoding.UTF8.GetString(response.RawBytes));

            if (response.ErrorException != null)
                throw new ApplicationException(string.Format(
                    "Received an exception when executing the request.\r\n\r\n{0}The exception was: {1} {2}{3}",
                    commandDescription,
                    response.ErrorMessage,
                    response.ErrorException,
                    rawBody));

            if (!allowedStatusCodes.Contains(response.StatusCode))
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request.\r\n\r\n{0}The response status was: {1} {2}{3}",
                    commandDescription,
                    (int) response.StatusCode,
                    response.StatusDescription,
                    rawBody));
        }
    }
}
