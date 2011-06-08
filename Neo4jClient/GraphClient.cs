using System;
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
            client = new RestClient(rootUri.AbsolutePath) { HttpFactory = httpFactory };
        }

        public void Connect()
        {
            var request = new RestRequest("");
            var response = client.Execute<ApiEndpoints>(request);
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