using System;
using System.IO;
using System.Linq;
using System.Net;
using RestSharp;

namespace Neo4jClient
{
    public class GraphClient : IGraphClient
    {
        readonly RestClient client;
        internal ApiEndpoints ApiEndpoints;

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
            var response = client.Execute<ApiEndpoints>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
                throw new ApplicationException("Failed to connect to server.", response.ErrorException);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(string.Format(
                    "Received a non-200 HTTP response when connecting to the server. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            ApiEndpoints = response.Data;
            ApiEndpoints.Node = ApiEndpoints.Node.Substring(client.BaseUrl.Length);
            ApiEndpoints.NodeIndex = ApiEndpoints.NodeIndex.Substring(client.BaseUrl.Length);
            ApiEndpoints.RelationshipIndex = ApiEndpoints.RelationshipIndex.Substring(client.BaseUrl.Length);
            ApiEndpoints.ReferenceNode = ApiEndpoints.ReferenceNode.Substring(client.BaseUrl.Length);
            ApiEndpoints.ExtensionsInfo = ApiEndpoints.ExtensionsInfo.Substring(client.BaseUrl.Length);
        }

        public NodeReference Create<TNode>(TNode node, params OutgoingRelationship<TNode>[] outgoingRelationships) where TNode : class
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (outgoingRelationships.Any())
                throw new NotImplementedException("Relationships aren't implemented yet.");

            var request = new RestRequest(ApiEndpoints.Node, Method.POST) {RequestFormat = DataFormat.Json};
            request.AddBody(node);

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.Created)
                throw new ApplicationException(string.Format(
                    "Received an unexpected HTTP status when executing the request. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            var nodeLocation = response.Headers.GetParameter("Location");
            var nodeId = int.Parse(GetLastPathSegment(nodeLocation));

            return new NodeReference(nodeId);
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