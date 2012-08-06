using System;
using System.Collections;
using System.Collections.Generic;
using RestSharp;

namespace Neo4jClient.Test
{
    public class RestTestHarness : IEnumerable
    {
        readonly IDictionary<IRestRequest, IHttpResponse> recordedResponses = new Dictionary<IRestRequest, IHttpResponse>();

        public void Add(IRestRequest request, IHttpResponse response)
        {
            recordedResponses.Add(request, response);
        }

        IHttpFactory HttpFactory
        {
            get { return MockHttpFactory.Generate("http://foo/db/data", recordedResponses); }
        }

        public IGraphClient CreateAndConnectGraphClient()
        {
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), HttpFactory);
            graphClient.Connect();
            return graphClient;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }
}
