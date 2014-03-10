using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Neo4jClient.Execution;
using Neo4jClient.Transactions;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test
{
    public class RestTestHarness : IEnumerable, IDisposable
    {
        readonly IDictionary<MockRequest, MockResponse> recordedResponses = new Dictionary<MockRequest, MockResponse>();
        readonly List<MockRequest> requestsThatShouldNotBeProcessed = new List<MockRequest>();
        readonly IList<MockRequest> processedRequests = new List<MockRequest>();
        readonly IList<string> unservicedRequests = new List<string>();
        public readonly string BaseUri = "http://foo/db/data";
        private readonly bool assertConstraintsAreMet;

        public RestTestHarness() : this(true)
        {
        }

        public RestTestHarness(bool assertConstraintsAreMet)
        {
            this.assertConstraintsAreMet = assertConstraintsAreMet;
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

        public GraphClient CreateGraphClient(bool neo4j2)
        {
            if (!recordedResponses.Keys.Any(r => r.Resource == "" || r.Resource == "/"))
                Add(MockRequest.Get(""), neo4j2 ? MockResponse.NeoRoot20() : MockResponse.NeoRoot());

            var httpClient = GenerateHttpClient(BaseUri);

            var graphClient = new GraphClient(new Uri(BaseUri), httpClient);
            return graphClient;
        }

        public ITransactionalGraphClient CreateAndConnectTransactionalGraphClient()
        {
            var graphClient = CreateGraphClient(true);
            graphClient.Connect();
            return graphClient;
        }

        public IRawGraphClient CreateAndConnectGraphClient()
        {
            var graphClient = CreateGraphClient(false);
            graphClient.Connect();
            return graphClient;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotSupportedException("This is just here to support dictionary style collection initializers for this type. Nothing more than syntactic sugar. Do not try and enumerate this type.");
        }

        public void AssertRequestConstraintsAreMet()
        {
            if (unservicedRequests.Any())
                Assert.Fail(string.Join("\r\n\r\n", unservicedRequests.ToArray()));

            var resourcesThatWereNeverRequested = recordedResponses
                .Select(r => r.Key)
                .Where(r => !(processedRequests.Contains(r) || requestsThatShouldNotBeProcessed.Contains(r)))
                .Select(r => string.Format("{0} {1}", r.Method, r.Resource))
                .ToArray();

            var processedResourcesThatShouldntHaveBeenRequested = requestsThatShouldNotBeProcessed
                .Where(r => processedRequests.Contains(r))
                .Select(r => string.Format("{0} {1}", r.Method, r.Resource))
                .ToArray();

            if (processedResourcesThatShouldntHaveBeenRequested.Any())
            {
                Assert.Fail(
                    "The test should not have made REST requests for the following resources: {0}",
                    string.Join(", ", processedResourcesThatShouldntHaveBeenRequested));
            }

            if (!resourcesThatWereNeverRequested.Any())
                return;

            Assert.Fail(
                "The test expected REST requests for the following resources, but they were never made: {0}",
                string.Join(", ", resourcesThatWereNeverRequested));
        }

        public IHttpClient GenerateHttpClient(string baseUri)
        {
            var httpClient = Substitute.For<IHttpClient>();

            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .ReturnsForAnyArgs(ci =>
                {
                    var request = ci.Arg<HttpRequestMessage>();
                    var task = new Task<HttpResponseMessage>(() => HandleRequest(request, baseUri));
                    task.Start();
                    return task;
                });

            return httpClient;
        }

        protected virtual HttpResponseMessage HandleRequest(HttpRequestMessage request, string baseUri)
        {
            // User info isn't transmitted over the wire, so we need to strip it here too
            var requestUri = request.RequestUri;
            if (!string.IsNullOrEmpty(requestUri.UserInfo))
                requestUri = new UriBuilder(requestUri) {UserName = "", Password = ""}.Uri;

            var matchingRequests = recordedResponses
                .Where(can => requestUri.AbsoluteUri == baseUri + can.Key.Resource)
                .Where(can => request.Method.ToString().Equals(can.Key.Method.ToString(), StringComparison.OrdinalIgnoreCase));

            string requestBody = null;
            if (request.Content != null)
            {
                var requestBodyTask = request.Content.ReadAsStringAsync();
                requestBodyTask.Wait();
                requestBody = requestBodyTask.Result;
            }

            if (request.Method == HttpMethod.Post)
            {
                matchingRequests = matchingRequests
                    .Where(can =>
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
                var message = string.Format("No corresponding request-response pair was defined in the test harness for: {0} {1}", request.Method, requestUri.AbsoluteUri);
                if (!string.IsNullOrEmpty(requestBody))
                {
                    message += "\r\n\r\n" + requestBody;
                }
                unservicedRequests.Add(message);
                throw new InvalidOperationException(message);
            }

            var result = results.Single();

            processedRequests.Add(result.Key);

            var response = result.Value;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.StatusDescription,
                Content = string.IsNullOrEmpty(response.Content) ? null : new StringContent(response.Content, null, response.ContentType)
            };

            if (string.IsNullOrEmpty(response.Location))
            {
                return httpResponse;
            }

            httpResponse.Headers.Location = new Uri(response.Location);
            return httpResponse;
        }

        static bool IsJsonEquivalent(string lhs, string rhs)
        {
            lhs = NormalizeJson(lhs);
            rhs = NormalizeJson(rhs);
            return lhs == rhs;
        }

        static string NormalizeJson(string input)
        {
            if (input.First() == '"' &&
                input.Last() == '"')
                input = input.Substring(1, input.Length - 2);

            return input
                .Replace(" ", "")
                .Replace("'", "\"")
                .Replace("\r", "")
                .Replace("\\r", "")
                .Replace("\n", "")
                .Replace("\\n", "");
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
