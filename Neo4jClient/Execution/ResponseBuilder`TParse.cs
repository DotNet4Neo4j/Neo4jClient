using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Neo4jClient.Execution
{
    internal class ResponseBuilder<TParse> 
        : ResponseBuilder, IResponseBuilder<TParse> where TParse : new()
    {
        public ResponseBuilder(HttpRequestMessage request, ISet<HttpStatusCode> expectedStatusCodes,
            ExecutionConfiguration executionConfiguration)
            : this(request, expectedStatusCodes, executionConfiguration, new List<ErrorGenerator>())
        {
        }

        public ResponseBuilder(HttpRequestMessage request, ISet<HttpStatusCode> expectedStatusCodes,
            ExecutionConfiguration executionConfiguration, IList<ErrorGenerator> errorGenerators)
            : base(request, expectedStatusCodes, executionConfiguration, errorGenerators, null)
        {
        }

        private TParse CastIntoResult(HttpResponseMessage response)
        {
            return response == null || response.Content == null ?
                default(TParse) :
                response.Content.ReadAsJson<TParse>(_executionConfiguration.JsonConverters);
        }

        public new IResponseBuilder<TParse> WithExpectedStatusCodes(params HttpStatusCode[] statusCodes)
        {
            return new ResponseBuilder<TParse>(_request, UnionStatusCodes(_expectedStatusCodes, statusCodes), _executionConfiguration);
        }

        public new IResponseFailBuilder<TParse> FailOnCondition(Func<HttpResponseMessage, bool> condition)
        {
            return new ResponseFailBuilder<TParse>(_request, _expectedStatusCodes, _executionConfiguration, ErrorGenerators, condition);
        }

        public new Task<TParse> ExecuteAsync(string commandDescription)
        {
            return ExecuteAsync(commandDescription, null);
        }

        public Task<TParse> ExecuteAsync(Func<Task<TParse>, TParse> continuationFunction)
        {
            return ExecuteAsync(null, continuationFunction);
        }


        public Task<TParse> ExecuteAsync(string commandDescription, Func<Task<TParse>, TParse> continuationFunction)
        {
            var executionTask = base.ExecuteAsync(commandDescription, null)
                .ContinueWith(
                    responseAction =>
                        responseAction.Result == null ? default(TParse) : CastIntoResult(responseAction.Result));
            return continuationFunction == null ? executionTask : executionTask.ContinueWith(continuationFunction);
        }

        public new Task<TParse> ExecuteAsync()
        {
            return ExecuteAsync(null, null);
        }

        public Task<TExpected> ExecuteAsync<TExpected>(Func<Task<TParse>, TExpected> continuationFunction)
        {
            return ExecuteAsync(null, continuationFunction);
        }

        public Task<TExpected> ExecuteAsync<TExpected>(string commandDescription, Func<Task<TParse>, TExpected> continuationFunction)
        {
            return ExecuteAsync(commandDescription).ContinueWith(continuationFunction);
        }

        public new TParse Execute(string commandDescription)
        {
            return CastIntoResult(base.Execute(commandDescription));
        }

        public new TParse Execute()
        {
            return Execute(null);
        }
    }
}
