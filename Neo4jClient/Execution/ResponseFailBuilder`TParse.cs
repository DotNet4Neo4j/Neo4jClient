using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Neo4jClient.Execution
{
    internal class ResponseFailBuilder<TParse> : IResponseFailBuilder<TParse> where TParse : new()
    {
        private readonly HttpRequestMessage _request;
        private readonly ISet<HttpStatusCode> _expectedStatusCodes;
        private readonly ExecutionConfiguration _executionConfiguration;
        private readonly IList<ErrorGenerator> _errorGenerators;
        private readonly Func<HttpResponseMessage, bool> _errorCondition;

        public ResponseFailBuilder(HttpRequestMessage request, ISet<HttpStatusCode> expectedStatusCodes,
            ExecutionConfiguration executionConfiguration, IList<ErrorGenerator> errorGenerators,
            Func<HttpResponseMessage, bool> condition)
        {
            _request = request;
            _expectedStatusCodes = expectedStatusCodes;
            _executionConfiguration = executionConfiguration;
            _errorGenerators = errorGenerators;
            _errorCondition = condition;
        }

        public IResponseBuilder<TParse> WithError(Func<HttpResponseMessage, Exception> errorBuilder)
        {
            var newGenerators = new List<ErrorGenerator>(_errorGenerators)
            {
                new ErrorGenerator
                {
                    Condition = _errorCondition,
                    Generator = errorBuilder
                }
            };
            return new ResponseBuilder<TParse>(_request, _expectedStatusCodes, _executionConfiguration, newGenerators);
        }

        public IResponseBuilder<TParse> WithDefault()
        {
            var newGenerators = new List<ErrorGenerator>(_errorGenerators)
            {
                new ErrorGenerator
                {
                    Condition = _errorCondition,
                    Generator = null
                }
            };
            return new ResponseBuilder<TParse>(_request, _expectedStatusCodes, _executionConfiguration, newGenerators);
        }
    }
}
