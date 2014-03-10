using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Neo4jClient.Execution
{
    internal class RequestTypeBuilder : IRequestTypeBuilder
    {
        private readonly ExecutionConfiguration _executionConfiguration;
        
        public RequestTypeBuilder(ExecutionConfiguration executionConfiguration)
        {
            _executionConfiguration = executionConfiguration;
        }

        public IResponseBuilder Delete(Uri endpoint)
        {
            return new ResponseBuilder(new HttpRequestMessage(HttpMethod.Delete, endpoint), _executionConfiguration);
        }

        public IResponseBuilder Get(Uri endpoint)
        {
            return new ResponseBuilder(new HttpRequestMessage(HttpMethod.Get, endpoint), _executionConfiguration);
        }

        public IRequestWithPendingContentBuilder Post(Uri endpoint)
        {
            return new RequestWithPendingContentBuilder(HttpMethod.Post, endpoint, _executionConfiguration);
        }

        public IRequestWithPendingContentBuilder Put(Uri endpoint)
        {
            return new RequestWithPendingContentBuilder(HttpMethod.Put, endpoint, _executionConfiguration);
        }
    }

    
}
