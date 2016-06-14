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
        private readonly ExecutionConfiguration _executionConfiguration;
        private readonly NameValueCollection _customHeaders;
        private readonly int? _maxExecutionTime;

        public RequestTypeBuilder(ExecutionConfiguration executionConfiguration, NameValueCollection customHeaders, int? maxExecutionTime)
        {
            _executionConfiguration = executionConfiguration;
            _customHeaders = customHeaders;
            _maxExecutionTime = maxExecutionTime;
        }

        public IResponseBuilder Delete(Uri endpoint)
        {
            return new ResponseBuilder(new HttpRequestMessage(HttpMethod.Delete, endpoint), _executionConfiguration, _customHeaders);
        }

        public IResponseBuilder Get(Uri endpoint)
        {
            return new ResponseBuilder(new HttpRequestMessage(HttpMethod.Get, endpoint), _executionConfiguration, _customHeaders);
        }

        public IRequestWithPendingContentBuilder Post(Uri endpoint)
        {
            return new RequestWithPendingContentBuilder(HttpMethod.Post, endpoint, _executionConfiguration, _customHeaders, _maxExecutionTime);
        }

        public IRequestWithPendingContentBuilder Put(Uri endpoint)
        {
            return new RequestWithPendingContentBuilder(HttpMethod.Put, endpoint, _executionConfiguration, _customHeaders, _maxExecutionTime);
        }
    }

    
}
