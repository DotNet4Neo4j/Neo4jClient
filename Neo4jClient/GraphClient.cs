﻿using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using RestSharp;

namespace Neo4jClient
{
    public class GraphClient : IGraphClient
    {
        readonly RestClient client;
        internal RootEndpoints RootEndpoints;

        public GraphClient(Uri rootUri)
            : this(rootUri, new Http())
        {
        }

        public GraphClient(Uri rootUri, IHttpFactory httpFactory)
        {
            client = new RestClient(rootUri.AbsoluteUri) { HttpFactory = httpFactory };
        }

        public void Connect()
        {
            var request = new RestRequest("", Method.GET);
            var response = client.Execute<RootEndpoints>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
                throw new ApplicationException("Failed to connect to server.", response.ErrorException);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(string.Format(
                    "Received a non-200 HTTP response when connecting to the server. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            RootEndpoints = response.Data;
            RootEndpoints.Node = RootEndpoints.Node.Substring(client.BaseUrl.Length);
            RootEndpoints.NodeIndex = RootEndpoints.NodeIndex.Substring(client.BaseUrl.Length);
            RootEndpoints.RelationshipIndex = RootEndpoints.RelationshipIndex.Substring(client.BaseUrl.Length);
            RootEndpoints.ReferenceNode = RootEndpoints.ReferenceNode.Substring(client.BaseUrl.Length);
            RootEndpoints.ExtensionsInfo = RootEndpoints.ExtensionsInfo.Substring(client.BaseUrl.Length);
        }

        public NodeReference<TNode> Create<TNode>(TNode node, params IRelationshipAllowingParticipantNode<TNode>[] relationships) where TNode : class
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

            if (RootEndpoints == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            var request = new RestRequest(RootEndpoints.Node, Method.POST) {RequestFormat = DataFormat.Json};
            request.AddBody(node);

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.Created)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            var nodeLocation = response.Headers.GetParameter("Location");
            var nodeId = int.Parse(GetLastPathSegment(nodeLocation));
            var nodeReference = new NodeReference<TNode>(nodeId);

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

        void CreateRelationship(NodeReference sourceNode, NodeReference targetNode, string relationshipTypeKey, object data)
        {
            var relationship = new RelationshipPacket
            {
                To = client.BaseUrl + ResolveEndpoint(targetNode),
                Data = data,
                Type = relationshipTypeKey
            };

            var sourceNodeEndpoint = ResolveEndpoint(sourceNode) + "/relationships";
            var request = new RestRequest(sourceNodeEndpoint, Method.POST) { RequestFormat = DataFormat.Json };
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

        public TNode Get<TNode>(NodeReference reference)
        {
            if (RootEndpoints == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            var nodeEndpoint = ResolveEndpoint(reference);
            var request = new RestRequest(nodeEndpoint, Method.GET);
            var response = client.Execute<NodePacket<TNode>>(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return default(TNode);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            return response.Data.Data;
        }

        public TNode Get<TNode>(NodeReference<TNode> reference)
        {
            return Get<TNode>((NodeReference) reference);
        }

        private string ResolveEndpoint(NodeReference node)
        {
            return RootEndpoints.Node + "/" + node.Id;
        }

        static string GetLastPathSegment(string uri)
        {
            var path = new Uri(uri).AbsolutePath;
            return path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();
        }
    }
}