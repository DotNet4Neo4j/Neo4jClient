using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Neo4jClient.Execution;
using Neo4jClient.Transactions;
using NSubstitute;

namespace Neo4jClient.Tests
{
    public class RestTestHarness : IEnumerable, IDisposable
    {
        public enum Neo4jVersion
        {
            Neo19,
            Neo20,
            Neo22,
            Neo225,
            Neo226,
            Neo23,
            Neo30,
            Neo40
        }

        readonly IDictionary<MockRequest, MockResponse> recordedResponses = new Dictionary<MockRequest, MockResponse>();
        readonly List<MockRequest> requestsThatShouldNotBeProcessed = new List<MockRequest>();
        readonly IList<MockRequest> processedRequests = new List<MockRequest>();
        readonly IList<string> unservicedRequests = new List<string>();
        public string BaseUri { get; }
        private readonly bool assertConstraintsAreMet;

        public RestTestHarness() 
            : this(true)
        {
        }

        public RestTestHarness(bool assertConstraintsAreMet) 
            : this(assertConstraintsAreMet, "http://foo/db/data")
        {
        }

        public RestTestHarness(bool assertConstraintsAreMet, string baseUri)
        {
            this.assertConstraintsAreMet = assertConstraintsAreMet;
            BaseUri = baseUri;
        }

        public void Add(MockRequest request, MockResponse response)
        {
            recordedResponses.Add(request, response);
        }

        public RestTestHarness ShouldNotBeCalled(params MockRequest[] requests)
        {
            requestsThatShouldNotBeProcessed.AddRange(requests);
            return this;
        }

        public GraphClient CreateGraphClient(Neo4jVersion neoVersion)
        {
            if (!recordedResponses.Keys.Any(r => r.Resource == "" || r.Resource == "/"))
            {
                MockResponse response;
                switch (neoVersion)
                {
                    case Neo4jVersion.Neo19:
                        response = MockResponse.NeoRoot(1,9,0);
                        break;
                    case Neo4jVersion.Neo20:
                        response = MockResponse.NeoRoot20();
                        break;
                    case Neo4jVersion.Neo22:
                        response = MockResponse.NeoRoot(2,2,0);
                        break;
                    case Neo4jVersion.Neo225:
                        response = MockResponse.NeoRoot(2,2,5);
                        break;
                    case Neo4jVersion.Neo226:
                        response = MockResponse.NeoRoot(2,2,6);
                        break;
                    case Neo4jVersion.Neo23:
                        response = MockResponse.NeoRoot(2,3,0);
                        break;
                    case Neo4jVersion.Neo30:
                        response = MockResponse.NeoRoot(3,0,0);
                        break;
                    case Neo4jVersion.Neo40:
                        response = MockResponse.NeoRoot(4,0,0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(neoVersion), neoVersion, null);
                }
                Add(MockRequest.Get(""), response);
            }

            var httpClient = GenerateHttpClient(BaseUri);

            var graphClient = new GraphClient(new Uri(BaseUri), httpClient);
            return graphClient;
        }

        public async Task<ITransactionalGraphClient> CreateAndConnectTransactionalGraphClient(Neo4jVersion version = Neo4jVersion.Neo20)
        {
            var graphClient = CreateGraphClient(version);
            await graphClient.ConnectAsync();
            return graphClient;
        }

        public async Task<IRawGraphClient> CreateAndConnectGraphClient(Neo4jVersion version = Neo4jVersion.Neo19)
        {
            var graphClient = CreateGraphClient(version);
            await graphClient.ConnectAsync();
            return graphClient;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotSupportedException("This is just here to support dictionary style collection initializers for this type. Nothing more than syntactic sugar. Do not try and enumerate this type.");
        }

        public void AssertRequestConstraintsAreMet()
        {
            if (unservicedRequests.Any())
                throw new Exception(string.Join("\r\n\r\n", unservicedRequests.ToArray()));

            var resourcesThatWereNeverRequested = recordedResponses.Select(r => r.Key).Where(r => !(processedRequests.Contains(r) || requestsThatShouldNotBeProcessed.Contains(r))).Select(r => string.Format("{0} {1}", r.Method, r.Resource)).ToArray();

            var processedResourcesThatShouldntHaveBeenRequested = requestsThatShouldNotBeProcessed.Where(r => processedRequests.Contains(r)).Select(r => string.Format("{0} {1}", r.Method, r.Resource)).ToArray();

            if (processedResourcesThatShouldntHaveBeenRequested.Any())
            {
                throw new Exception($"The test should not have made REST requests for the following resources: {string.Join(", ", processedResourcesThatShouldntHaveBeenRequested)}");
            }

            if (!resourcesThatWereNeverRequested.Any())
                return;

            throw new Exception($"The test expected REST requests for the following resources, but they were never made: {string.Join(", ", resourcesThatWereNeverRequested)}");
    }

        public IHttpClient GenerateHttpClient(string baseUri)
        {
            var httpClient = Substitute.For<IHttpClient>();

            httpClient.SendAsync(Arg.Any<HttpRequestMessage>()).ReturnsForAnyArgs(ci =>
            {
                var request = ci.Arg<HttpRequestMessage>();
                return HandleRequest(request, baseUri);
            });

            return httpClient;
        }

        protected virtual async Task<HttpResponseMessage> HandleRequest(HttpRequestMessage request, string baseUri)
        {
            // User info isn't transmitted over the wire, so we need to strip it here too
            var requestUri = request.RequestUri;
            if (!string.IsNullOrEmpty(requestUri.UserInfo))
                requestUri = new UriBuilder(requestUri) {UserName = "", Password = ""}.Uri;

            var matchingRequests = recordedResponses.Where(can => requestUri.AbsoluteUri == baseUri + can.Key.Resource).Where(can => request.Method.ToString().Equals(can.Key.Method.ToString(), StringComparison.OrdinalIgnoreCase));

            string requestBody = null;
            if (request.Content != null)
            {
                var requestBodyTask = request.Content.ReadAsStringAsync();
                requestBody = await requestBodyTask;
            }

            if (request.Method == HttpMethod.Post)
            {
                matchingRequests = matchingRequests.Where(can =>
                {
                    var cannedRequest = can.Key;
                    var cannedRequestBody = cannedRequest.Body;
                    cannedRequestBody = cannedRequestBody ?? "";
                    return IsJsonEquivalent(cannedRequestBody, requestBody);
                });
            }

            var results = matchingRequests.ToArray();

            if (!results.Any())
            {
                var message = $"No corresponding request-response pair was defined in the test harness for: {request.Method} {requestUri.AbsoluteUri}";
                if (!string.IsNullOrEmpty(requestBody))
                {
                    message += "\r\n\r\n" + requestBody;
                }
                unservicedRequests.Add(message);
                throw new InvalidOperationException(message);
            }

            var (key, response) = results.Single();

            processedRequests.Add(key);

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = response.StatusCode, ReasonPhrase = response.StatusDescription, Content = string.IsNullOrEmpty(response.Content) ? null : new StringContent(response.Content, null, response.ContentType)
            };

            if (string.IsNullOrEmpty(response.Location))
            {
                return httpResponse;
            }

            httpResponse.Headers.Location = new Uri(response.Location);
            return httpResponse;
        }

        private static bool IsJsonEquivalent(string lhs, string rhs)
        {
            lhs = NormalizeJson(lhs);
            rhs = NormalizeJson(rhs);
            return lhs == rhs;
        }

        private static string NormalizeJson(string input)
        {
            if (input.First() == '"' && input.Last() == '"')
                input = input.Substring(1, input.Length - 2);

            return input.Replace(" ", "").Replace("'", "\"").Replace("\r", "").Replace("\\r", "").Replace("\n", "").Replace("\\n", "");
        }

        public void Dispose()
        {
            if (assertConstraintsAreMet)
            {
                AssertRequestConstraintsAreMet();
            }
        }
    }
}
