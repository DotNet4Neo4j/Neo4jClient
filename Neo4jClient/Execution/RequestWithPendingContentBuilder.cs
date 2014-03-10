using System;
using System.Net.Http;
using System.Text;

namespace Neo4jClient.Execution
{
    internal class RequestWithPendingContentBuilder : IRequestWithPendingContentBuilder
    {
        private readonly HttpMethod _httpMethod;
        private readonly Uri _endpoint;
        private readonly ExecutionConfiguration _executionConfiguration;

        public RequestWithPendingContentBuilder(HttpMethod httpMethod, Uri endpoint, ExecutionConfiguration executionConfiguration)
        {
            _httpMethod = httpMethod;
            _endpoint = endpoint;
            _executionConfiguration = executionConfiguration;
        }

        public IResponseBuilder WithContent(string content)
        {
            return new ResponseBuilder(
                new HttpRequestMessage(_httpMethod, _endpoint)
                {
                    Content = new StringContent(content, Encoding.UTF8)
                },
                _executionConfiguration
            );
        }

        public IResponseBuilder WithJsonContent(string jsonContent)
        {
            return new ResponseBuilder(
                new HttpRequestMessage(_httpMethod, _endpoint)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                }, 
                _executionConfiguration
            );
        }
    }
}