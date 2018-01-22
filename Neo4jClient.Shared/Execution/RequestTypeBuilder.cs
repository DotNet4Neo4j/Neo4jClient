using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Neo4jClient.Execution
{
    internal class RequestTypeBuilder : IRequestTypeBuilder
    {
        private readonly ExecutionConfiguration executionConfiguration;
        private readonly NameValueCollection customHeaders;
        private readonly int? maxExecutionTime;

        public RequestTypeBuilder(ExecutionConfiguration executionConfiguration, NameValueCollection customHeaders, int? maxExecutionTime)
        {
            this.executionConfiguration = executionConfiguration;
            this.customHeaders = customHeaders;
            this.maxExecutionTime = maxExecutionTime;
        }

        public IResponseBuilder Delete(Uri endpoint)
        {
            return new ResponseBuilder(new HttpRequestMessage(HttpMethod.Delete, endpoint), executionConfiguration, customHeaders);
        }

        public IResponseBuilder Get(Uri endpoint)
        {
            return new ResponseBuilder(new HttpRequestMessage(HttpMethod.Get, endpoint), executionConfiguration, customHeaders);
        }

        public IRequestWithPendingContentBuilder Post(Uri endpoint)
        {
            return new RequestWithPendingContentBuilder(HttpMethod.Post, endpoint, executionConfiguration, customHeaders, maxExecutionTime);
        }

        public IRequestWithPendingContentBuilder Put(Uri endpoint)
        {
            return new RequestWithPendingContentBuilder(HttpMethod.Put, endpoint, executionConfiguration, customHeaders, maxExecutionTime);
        }
    }

    
}
