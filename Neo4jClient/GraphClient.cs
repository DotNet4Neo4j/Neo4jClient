using System;
using System.Net;
using RestSharp;

namespace Neo4jClient
{
    public class GraphClient : IGraphClient
    {
        readonly RestClient client;
        ApiEndpoints apiEndpoints;

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

            if (response.ResponseStatus != ResponseStatus.Completed ||
                response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException("Failed to connect to server.");

            apiEndpoints = response.Data;
        }

        public NodeReference Create<TNode>(TNode node, params OutgoingRelationship<TNode>[] outgoingRelationships) where TNode : class
        {
            if (node == null)
                throw new ArgumentNullException("node");

            throw new NotImplementedException();
        }
    }
}