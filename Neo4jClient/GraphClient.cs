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
        internal ApiRootEndpoints ApiRootEndpoints;

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
            var response = client.Execute<ApiRootEndpoints>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
                throw new ApplicationException("Failed to connect to server.", response.ErrorException);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(string.Format(
                    "Received a non-200 HTTP response when connecting to the server. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.StatusDescription));

            ApiRootEndpoints = response.Data;
            ApiRootEndpoints.Node = ApiRootEndpoints.Node.Substring(client.BaseUrl.Length);
            ApiRootEndpoints.NodeIndex = ApiRootEndpoints.NodeIndex.Substring(client.BaseUrl.Length);
            ApiRootEndpoints.RelationshipIndex = ApiRootEndpoints.RelationshipIndex.Substring(client.BaseUrl.Length);
            ApiRootEndpoints.ReferenceNode = ApiRootEndpoints.ReferenceNode.Substring(client.BaseUrl.Length);
            ApiRootEndpoints.ExtensionsInfo = ApiRootEndpoints.ExtensionsInfo.Substring(client.BaseUrl.Length);
        }

        public NodeReference Create<TNode>(TNode node, params OutgoingRelationship<TNode>[] outgoingRelationships) where TNode : class
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (ApiRootEndpoints == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            if (outgoingRelationships.Any())
                throw new NotImplementedException("Relationships aren't implemented yet.");

            var request = new RestRequest(ApiRootEndpoints.Node, Method.POST) {RequestFormat = DataFormat.Json};
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

        public TNode Get<TNode>(NodeReference reference)
        {
            if (ApiRootEndpoints == null)
                throw new InvalidOperationException("The graph client is not connected to the server. Call the Connect method first.");

            var nodeResouce = ApiRootEndpoints.Node + "/" + reference.Id;
            var request = new RestRequest(nodeResouce, Method.GET);
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

        static string GetLastPathSegment(string uri)
        {
            var path = new Uri(uri).AbsolutePath;
            return path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();
        }
    }
}