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

        private async Task<TParse> CastIntoResult(HttpResponseMessage response)
        {
            return response == null || response.Content == null ?
                default(TParse) :
                await response.Content.ReadAsJsonAsync<TParse>(_executionConfiguration.JsonConverters).ConfigureAwait(false);
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

        public Task<TParse> ExecuteAsync(Func<TParse, TParse> continuationFunction)
        {
            return ExecuteAsync(null, continuationFunction);
        }
        
        public async Task<TParse> ExecuteAsync(string commandDescription, Func<TParse, TParse> continuationFunction)
        {
            var response = await base.ExecuteAsync(commandDescription, null).ConfigureAwait(false);
            var parsed = response == null ? default(TParse) : await CastIntoResult(response).ConfigureAwait(false);
            return continuationFunction == null ? parsed : continuationFunction(parsed);
        }

        public new Task<TParse> ExecuteAsync()
        {
            return ExecuteAsync(null, null);
        }

        public Task<TExpected> ExecuteAsync<TExpected>(Func<TParse, TExpected> continuationFunction)
        {
            return ExecuteAsync(null, continuationFunction);
        }

        public async Task<TExpected> ExecuteAsync<TExpected>(string commandDescription, Func<TParse, TExpected> continuationFunction)
        {
            return continuationFunction(await ExecuteAsync(commandDescription).ConfigureAwait(false));
        }
    }
}
