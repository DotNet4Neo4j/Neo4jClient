using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;

namespace Neo4jClient.Execution
{
    internal class RequestWithPendingContentBuilder : IRequestWithPendingContentBuilder
    {
        private readonly HttpMethod httpMethod;
        private readonly Uri endpoint;
        private readonly ExecutionConfiguration executionConfiguration;
        private readonly NameValueCollection customHeaders;
        private readonly int? maxExecutionTime;

        public RequestWithPendingContentBuilder(HttpMethod httpMethod, Uri endpoint, ExecutionConfiguration executionConfiguration, NameValueCollection customHeaders, int? maxExecutionTime)
        {
            this.httpMethod = httpMethod;
            this.endpoint = endpoint;
            this.executionConfiguration = executionConfiguration;
            this.customHeaders = customHeaders;
            this.maxExecutionTime = maxExecutionTime;
        }

        public IResponseBuilder WithContent(string content)
        {
            return new ResponseBuilder(
                new HttpRequestMessage(httpMethod, endpoint)
                {
                    Content = new StringContent(content, Encoding.UTF8)
                },
                executionConfiguration,
                customHeaders,
                maxExecutionTime
            );
        }

        public IResponseBuilder WithJsonContent(string jsonContent)
        {
            return new ResponseBuilder(
                new HttpRequestMessage(httpMethod, endpoint)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                }, 
                executionConfiguration,
                customHeaders,
                maxExecutionTime
            );
        }
    }
}