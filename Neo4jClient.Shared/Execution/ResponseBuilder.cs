using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.Execution
{
    internal class ResponseBuilder : IResponseBuilder
    {
        protected readonly HttpRequestMessage _request;
        protected readonly ExecutionConfiguration _executionConfiguration;
        protected readonly ISet<HttpStatusCode> _expectedStatusCodes;
        protected readonly IList<ErrorGenerator> _errorGenerators;
        protected readonly NameValueCollection _customHeaders;
        protected readonly int? _maxExecutionTime;
        internal const string MaxExecutionTimeHeaderKey = "max-execution-time";

        public ISet<HttpStatusCode> ExpectedStatusCodes
        {
            get { return _expectedStatusCodes; }
        }

        public IList<ErrorGenerator> ErrorGenerators
        {
            get { return _errorGenerators; }
        }

        public ResponseBuilder(HttpRequestMessage request, ExecutionConfiguration executionConfiguration, NameValueCollection nameValueCollection, int? maxExecutionTime = null)
            : this(request, new HashSet<HttpStatusCode>(), executionConfiguration, new List<ErrorGenerator>(), nameValueCollection, maxExecutionTime)
        {
        }

        public ResponseBuilder(HttpRequestMessage request, ISet<HttpStatusCode> expectedStatusCodes, ExecutionConfiguration executionConfiguration) :
            this(request, expectedStatusCodes, executionConfiguration, new List<ErrorGenerator>(), null)
        {
        }

        public ResponseBuilder(HttpRequestMessage request, ISet<HttpStatusCode> expectedStatusCodes,
            ExecutionConfiguration executionConfiguration, IList<ErrorGenerator> errorGenerators, NameValueCollection customHeaders, int? maxExecutionTime = null)
        {
            _request = request;
            _expectedStatusCodes = expectedStatusCodes;
            _executionConfiguration = executionConfiguration;
            _errorGenerators = errorGenerators;
            _customHeaders = customHeaders;
            _maxExecutionTime = maxExecutionTime;
        }

        protected ISet<HttpStatusCode> UnionStatusCodes(
            IEnumerable<HttpStatusCode> source1,
            IEnumerable<HttpStatusCode> source2
        )
        {
            var expectedStatusCodes = new HashSet<HttpStatusCode>(source1);
            expectedStatusCodes.UnionWith(source2);
            return expectedStatusCodes;
        }

        public IResponseBuilder WithExpectedStatusCodes(params HttpStatusCode[] statusCodes)
        {
            return new ResponseBuilder(_request, UnionStatusCodes(_expectedStatusCodes, statusCodes),
                _executionConfiguration, _errorGenerators, _customHeaders, _maxExecutionTime);
        }

        public IResponseFailBuilder FailOnCondition(Func<HttpResponseMessage, bool> condition)
        {
            return new ResponseFailBuilder(_request, _expectedStatusCodes, _executionConfiguration, _errorGenerators,
                condition, _customHeaders);
        }

        private async Task<HttpResponseMessage> PrepareAsync(TaskFactory taskFactory)
        {
            if (_executionConfiguration.UseJsonStreaming)
            {
                _request.Headers.Accept.Clear();
                _request.Headers.Remove("Accept");
                _request.Headers.Add("Accept", "application/json;stream=true");
                _request.Headers.Remove("X-Stream");
                _request.Headers.Add("X-Stream", "true");
            }

            _request.Headers.Add("User-Agent", _executionConfiguration.UserAgent);

            if (_maxExecutionTime.HasValue)
            {
                _request.Headers.Add(MaxExecutionTimeHeaderKey, _maxExecutionTime.Value.ToString());
            }

            if (_customHeaders != null && _customHeaders.Count > 0)
            {
                foreach (var customHeaderKey in _customHeaders.AllKeys)
                {
                    var headerValue = _customHeaders.Get(customHeaderKey);
                    if (!string.IsNullOrWhiteSpace(headerValue))
                    {
                        _request.Headers.Add(customHeaderKey, headerValue);
                    }
                }
            }

            var userInfo = _request.RequestUri.UserInfo;
            if (!string.IsNullOrEmpty(userInfo))
            {
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userInfo));
                _request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }

            if (taskFactory == null)
            {
                // use the standard factory
                return await _executionConfiguration.HttpClient.SendAsync(_request).ConfigureAwait(false);
            }

            // use a custom task factory
            return await taskFactory.StartNew(() => _executionConfiguration.HttpClient.SendAsync(_request).Result).ConfigureAwait(false);
        }

        public Task<HttpResponseMessage> ExecuteAsync()
        {
            return ExecuteAsync(null, null, null);
        }

        public Task<HttpResponseMessage> ExecuteAsync(string commandDescription)
        {
            return ExecuteAsync(commandDescription, null, null);
        }

        public Task<HttpResponseMessage> ExecuteAsync(string commandDescription, Func<Task<HttpResponseMessage>, HttpResponseMessage> continuationFunction)
        {
            return ExecuteAsync(commandDescription, continuationFunction, null);
        }

        public Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>, HttpResponseMessage> continuationFunction)
        {
            return ExecuteAsync(null, continuationFunction, null);
        }

        public Task<HttpResponseMessage> ExecuteAsync(string commandDescription, Func<Task<HttpResponseMessage>, HttpResponseMessage> continuationFunction, TaskFactory taskFactory)
        {
            var executionTask = PrepareAsync(taskFactory).ContinueWith(requestTask =>
            {
                var response = requestTask.Result;
                if (string.IsNullOrEmpty(commandDescription))
                {
                    response.EnsureExpectedStatusCode(_expectedStatusCodes.ToArray());
                }
                else
                {
                    response.EnsureExpectedStatusCode(commandDescription, _expectedStatusCodes.ToArray());
                }

                // if there is condition for an error, but its generator is null then return null
                // for generics this will get converted to default(TParse)
                foreach (var errorGenerator in _errorGenerators)
                {
                    if (errorGenerator.Condition(response))
                    {
                        if (errorGenerator.Generator != null)
                        {
                            throw errorGenerator.Generator(response);
                        }

                        return null;
                    }
                }

                return response;
            });

            return continuationFunction != null ?
                executionTask.ContinueWith(continuationFunction) :
                executionTask;
        }

        public Task<TExpected> ExecuteAsync<TExpected>(Func<Task<HttpResponseMessage>, TExpected> continuationFunction)
        {
            return ExecuteAsync(null, continuationFunction);
        }

        public Task<TExpected> ExecuteAsync<TExpected>(string commandDescription, Func<Task<HttpResponseMessage>, TExpected> continuationFunction)
        {
            return ExecuteAsync(commandDescription, continuationFunction, null);
        }

        public Task<TExpected> ExecuteAsync<TExpected>(string commandDescription, Func<Task<HttpResponseMessage>, TExpected> continuationFunction, TaskFactory taskFactory)
        {
            return ExecuteAsync(commandDescription, null, taskFactory).ContinueWith(continuationFunction);
        }

        public HttpResponseMessage Execute()
        {
            return Execute(null, null);
        }

        public HttpResponseMessage Execute(string commandDescription)
        {
            return Execute(commandDescription, null);
        }

        public HttpResponseMessage Execute(string commandDescription, TaskFactory taskFactory)
        {
            var task = ExecuteAsync(commandDescription, null, taskFactory);
            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException ex)
            {
                Exception e;
                if (ex.TryUnwrap(out e))
                    throw e;
                throw;
            }
            return task.Result;
        }

        public IResponseBuilder<TParse> ParseAs<TParse>() where TParse : new()
        {
            return new ResponseBuilder<TParse>(_request, _expectedStatusCodes, _executionConfiguration);
        }
    }
}
