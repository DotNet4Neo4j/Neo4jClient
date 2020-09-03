using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;

namespace Neo4jClient.Execution
{
    internal class ResponseFailBuilder : IResponseFailBuilder
    {
        private readonly HttpRequestMessage _request;
        private readonly ISet<HttpStatusCode> _expectedStatusCodes;
        private readonly ExecutionConfiguration _executionConfiguration;
        private readonly IList<ErrorGenerator> _errorGenerators; 
        private readonly Func<HttpResponseMessage, bool> _errorCondition;
        private readonly NameValueCollection _customHeaders;

        public ResponseFailBuilder(HttpRequestMessage request, ISet<HttpStatusCode> expectedStatusCodes,
            ExecutionConfiguration executionConfiguration, IList<ErrorGenerator> errorGenerators,
            Func<HttpResponseMessage, bool> errorCondition, NameValueCollection customHeaders)
        {
            _request = request;
            _expectedStatusCodes = expectedStatusCodes;
            _executionConfiguration = executionConfiguration;
            _errorGenerators = errorGenerators;
            _errorCondition = errorCondition;
            _customHeaders = customHeaders;
        }

        public IResponseBuilder WithError(Func<HttpResponseMessage, Exception> errorBuilder)
        {
            var newGenerators = new List<ErrorGenerator>(_errorGenerators)
            {
                new ErrorGenerator
                {
                    Condition = _errorCondition,
                    Generator = errorBuilder
                }
            };

            return new ResponseBuilder(
                _request,
                _expectedStatusCodes,
                _executionConfiguration,
                newGenerators,
                _customHeaders
                );
        }

        public IResponseBuilder WithNull()
        {
            var newGenerators = new List<ErrorGenerator>(_errorGenerators)
            {
                new ErrorGenerator
                {
                    Condition = _errorCondition,
                    Generator = null
                }
            };

            return new ResponseBuilder(
                _request,
                _expectedStatusCodes,
                _executionConfiguration,
                newGenerators
                ,_customHeaders
                );
        }
    }
}