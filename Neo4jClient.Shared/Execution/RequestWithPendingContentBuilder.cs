using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;

namespace Neo4jClient.Execution
{
    internal class RequestWithPendingContentBuilder : IRequestWithPendingContentBuilder
    {
        private readonly HttpMethod _httpMethod;
        private readonly Uri _endpoint;
        private readonly ExecutionConfiguration _executionConfiguration;
        private readonly NameValueCollection _customHeaders;
        private readonly int? _maxExecutionTime;

        public RequestWithPendingContentBuilder(HttpMethod httpMethod, Uri endpoint, ExecutionConfiguration executionConfiguration, NameValueCollection customHeaders, int? maxExecutionTime)
        {
            _httpMethod = httpMethod;
            _endpoint = endpoint;
            _executionConfiguration = executionConfiguration;
            _customHeaders = customHeaders;
            _maxExecutionTime = maxExecutionTime;
        }

        public IResponseBuilder WithContent(string content)
        {
            return new ResponseBuilder(
                new HttpRequestMessage(_httpMethod, _endpoint)
                {
                    Content = new StringContent(content, Encoding.UTF8)
                },
                _executionConfiguration,
                _customHeaders,
                _maxExecutionTime
            );
        }

        public IResponseBuilder WithJsonContent(string jsonContent)
        {
            return new ResponseBuilder(
                new HttpRequestMessage(_httpMethod, _endpoint)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                }, 
                _executionConfiguration,
                _customHeaders,
                _maxExecutionTime
            );
        }
    }
}