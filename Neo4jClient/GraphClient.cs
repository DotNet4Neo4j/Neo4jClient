using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using Neo4jClient.Deserializer;
using Neo4jClient.Serializer;
using Newtonsoft.Json;
using RestSharp;

namespace Neo4jClient
{
    public class GraphClient : IGraphClient
    {
        readonly RestClient client;
        internal RootApiResponse RootApiResponse;

        public NullValueHandling JsonSerializerNullValueHandling { get; set; }

        public GraphClient(Uri rootUri)
            : this(rootUri, new Http())
        {
        }

        public GraphClient(Uri rootUri, IHttpFactory httpFactory)
        {
            JsonSerializerNullValueHandling = NullValueHandling.Ignore;
            client = new RestClient(rootUri.AbsoluteUri) { HttpFactory = httpFactory };
            client.RemoveHandler("application/json");
            client.AddHandler("application/json",new CustomJsonDeserializer());
        }

        public virtual void Connect()
        {
            var request = new RestRequest("", Method.GET);
            var response = client.Execute<RootApiResponse>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
                throw new ApplicationException(string.Format(
                    "Failed to connect to server at {0}. Failure details are described in the inner exception.",
                    client.BaseUrl),
                    response.ErrorException);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(string.Format(
                    "Received a non-200 HTTP response when connecting to the server. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            RootApiResponse = response.Data;
            RootApiResponse.Node = RootApiResponse.Node.Substring(client.BaseUrl.Length);
            RootApiResponse.NodeIndex = RootApiResponse.NodeIndex.Substring(client.BaseUrl.Length);
            RootApiResponse.RelationshipIndex = RootApiResponse.RelationshipIndex.Substring(client.BaseUrl.Length);
            RootApiResponse.ReferenceNode = RootApiResponse.ReferenceNode.Substring(client.BaseUrl.Length);
            RootApiResponse.ExtensionsInfo = RootApiResponse.ExtensionsInfo.Substring(client.BaseUrl.Length);
            if (RootApiResponse.Extensions != null && RootApiResponse.Extensions.GremlinPlugin != null)
            {
                RootApiResponse.Extensions.GremlinPlugin.ExecuteScript =
                    RootApiResponse.Extensions.GremlinPlugin.ExecuteScript.Substring(client.BaseUrl.Length);
            }
        }

        public virtual RootNode RootNode
        {
            get { return new RootNode(this); }
        }

        public virtual NodeReference<TNode> Create<TNode>(TNode node, params IRelationshipAllowingParticipantNode<TNode>[] relationships) where TNode : class
        {
            if (node == null)
                throw new ArgumentNullException("node");

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

            if (RootApiResponse == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            var request = new RestRequest(RootApiResponse.Node, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(node);
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.Created)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            var nodeLocation = response.Headers.GetParameter("Location");
            var nodeId = int.Parse(GetLastPathSegment(nodeLocation));
            var nodeReference = new NodeReference<TNode>(nodeId, this);

            foreach (var relationship in calculatedRelationships)
            {
                var participants = new[] {nodeReference, relationship.Relationship.OtherNode};
                NodeReference sourceNode, targetNode;
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
                            "The specified relationship direction is not supported: {0}", relationship.CalculatedDirection));
                }

                CreateRelationship(
                    sourceNode,
                    targetNode,
                    relationship.Relationship.RelationshipTypeKey,
                    relationship.Relationship.Data);
            }

            return nodeReference;
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

        void CreateRelationship(NodeReference sourceNode, NodeReference targetNode, string relationshipTypeKey, object data)
        {
            var relationship = new RelationshipTemplate
            {
                To = client.BaseUrl + ResolveEndpoint(targetNode),
                Data = data,
                Type = relationshipTypeKey
            };

            var sourceNodeEndpoint = ResolveEndpoint(sourceNode) + "/relationships";
            var request = new RestRequest(sourceNodeEndpoint, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(relationship);
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApplicationException(string.Format(
                    "One of the nodes referenced in the relationship could not be found. Referenced nodes were {0} and {1}.",
                    sourceNode.Id,
                    targetNode.Id));

            if (response.StatusCode != HttpStatusCode.Created)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));
        }

        public void DeleteRelationship(RelationshipReference reference)
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            var relationshipEndpoint = ResolveEndpoint(reference);
            var request = new RestRequest(relationshipEndpoint, Method.DELETE);
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApplicationException(string.Format(
                    "Unable to delete the relationship. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            if (response.StatusCode != HttpStatusCode.NoContent)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));
        }

        public virtual Node<TNode> Get<TNode>(NodeReference reference)
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            var nodeEndpoint = ResolveEndpoint(reference);
            var request = new RestRequest(nodeEndpoint, Method.GET);
            var response = client.Execute<NodeApiResponse<TNode>>(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            return response.Data.ToNode(this);
        }

        public virtual Node<TNode> Get<TNode>(NodeReference<TNode> reference)
        {
            return Get<TNode>((NodeReference) reference);
        }

        public void Update<TNode>(NodeReference<TNode> nodeReference, Action<TNode> updateCallback)
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            var node = Get(nodeReference);
            updateCallback(node.Data);

            var nodeEndpoint = ResolveEndpoint(nodeReference);
            var request = new RestRequest(nodeEndpoint + "/properties", Method.PUT)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = JsonSerializerNullValueHandling }
            };
            request.AddBody(node.Data);
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));
            }
        }

        public virtual void Delete(NodeReference reference, DeleteMode mode)
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            if (mode == DeleteMode.NodeAndRelationships)
            {
                DeleteAllRelationships(reference);
            }

            var nodeEndpoint = ResolveEndpoint(reference);
            var request = new RestRequest(nodeEndpoint, Method.DELETE);
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.Conflict)
                throw new ApplicationException(string.Format(
                    "Unable to delete the node. The node may still have relationships. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            if (response.StatusCode != HttpStatusCode.NoContent)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));
        }

        void DeleteAllRelationships(NodeReference reference)
        {
            //TODO: Make this a dynamic endpoint resolution
            var relationshipsEndpoint = ResolveEndpoint(reference) + "/relationships/all";
            var request = new RestRequest(relationshipsEndpoint, Method.GET);
            var response = client.Execute<List<RelationshipApiResponse>>(request);

            var relationshipResources = response
                .Data
                .Select(r => r.Self.Substring(client.BaseUrl.Length));

            foreach (var relationshipResource in relationshipResources)
            {
                request = new RestRequest(relationshipResource, Method.DELETE);
                client.Execute(request);
            }
        }

        private string ResolveEndpoint(NodeReference node)
        {
            return RootApiResponse.Node + "/" + node.Id;
        }

        string ResolveEndpoint(RelationshipReference relationship)
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

        public virtual string ExecuteScalarGremlin(string query)
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            var nodeResource = RootApiResponse.Extensions.GremlinPlugin.ExecuteScript;
            var request = new RestRequest(nodeResource, Method.POST);

            request.AddParameter("script", query, ParameterType.GetOrPost);
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request.\r\n\r\nThe query was: {0}\r\n\r\nThe response status was: {1} {2}",
                    query,
                    (int)response.StatusCode,
                    response.StatusDescription));

            return response.Content;
        }

        public virtual IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query)
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            var nodeResource = RootApiResponse.Extensions.GremlinPlugin.ExecuteScript;
            var request = new RestRequest(nodeResource, Method.POST);
            request.AddParameter("script", query, ParameterType.GetOrPost);
            var response = client.Execute<List<NodeApiResponse<TNode>>>(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request.\r\n\r\nThe query was: {0}\r\n\r\nThe response status was: {1} {2}",
                    query,
                    (int)response.StatusCode,
                    response.StatusDescription));

            return response.Data == null
                ? Enumerable.Empty<Node<TNode>>()
                : response.Data.Select(r => r.ToNode(this));
        }
    }
}