using System;
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
        }

        public NodeReference Create<TNode>(TNode node, params OutgoingRelationship<TNode>[] outgoingRelationships) where TNode : class
        {
            if (node == null)
                throw new ArgumentNullException("node");

            throw new NotImplementedException();
        }
    }
}