using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test
{
    public class RestTestHarness : IEnumerable, IDisposable
    {
        readonly IDictionary<IMockRequestDefinition, IHttpResponse> recordedResponses = new Dictionary<IMockRequestDefinition, IHttpResponse>();
        readonly IList<IMockRequestDefinition> processedRequests = new List<IMockRequestDefinition>();
        readonly IList<string> unservicedRequests = new List<string>();
        public readonly string BaseUri = "http://foo/db/data";

        public void Add(IMockRequestDefinition request, IHttpResponse response)
        {
            recordedResponses.Add(request, response);
        }

        IHttpFactory HttpFactory
        {
            get { return GenerateHttpFactory(BaseUri); }
        }

        public GraphClient CreateGraphClient()
        {
            if (!recordedResponses.Keys.Any(r => r.Resource == "" || r.Resource == "/"))
                Add(MockRequest.Get(""), MockResponse.NeoRoot());

            var graphClient = new GraphClient(new Uri(BaseUri), HttpFactory);
            return graphClient;
        }

        public IGraphClient CreateAndConnectGraphClient()
        {
            var graphClient = CreateGraphClient();
            graphClient.Connect();
            return graphClient;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotSupportedException("This is just here to support dictionary style collection initializers for this type. Nothing more than syntactic sugar. Do not try and enumerate this type.");
        }

        public void AssertAllRequestsWereReceived()
        {
            if (unservicedRequests.Any())
                Assert.Fail(string.Join("\r\n\r\n", unservicedRequests.ToArray()));

            var resourcesThatWereNeverRequested = recordedResponses
                .Select(r => r.Key)
                .Where(r => !processedRequests.Contains(r))
                .Select(r => string.Format("{0} {1}", r.Method, r.Resource))
                .ToArray();

            if (!resourcesThatWereNeverRequested.Any())
                return;

            Assert.Fail(
                "The test expected REST requests for the following resources, but they were never made: {0}",
                string.Join(", ", resourcesThatWereNeverRequested));
        }

        public IHttpFactory GenerateHttpFactory(string baseUri)
        {
            var httpFactory = Substitute.For<IHttpFactory>();
            httpFactory
                .Create()
                .Returns(callInfo =>
                {
                    var http = Substitute.For<IHttp>();
                    http.Delete().Returns(ci => HandleRequest(http, Method.DELETE, baseUri));
                    http.Get().Returns(ci => HandleRequest(http, Method.GET, baseUri));
                    http.Post().Returns(ci => HandleRequest(http, Method.POST, baseUri));
                    http.Parameters.ReturnsForAnyArgs(ci => HandleParameters(http, baseUri));
                    http.Put().Returns(ci => HandleRequest(http, Method.PUT, baseUri));
                    return http;
                });
            return httpFactory;
        }

        IList<HttpParameter> HandleParameters(IHttp http, string baseUri)
        {
            var matchingRequests = recordedResponses
                .Where(can => http.Url.AbsoluteUri == baseUri + can.Key.Resource);

            return matchingRequests
                .Select(can => can
                    .Key
                    .Parameters
                    .Select(param => new HttpParameter { Name = param.Name, Value = param.Value.ToString() })
                    .ToList())
                .SingleOrDefault();
        }

        IHttpResponse HandleRequest(IHttp http, Method method, string baseUri)
        {
            var matchingRequests = recordedResponses
                .Where(can => http.Url.AbsoluteUri == baseUri + can.Key.Resource)
                .Where(can => can.Key.Method == method);

            if (method == Method.POST)
            {
                matchingRequests = matchingRequests
                    .Where(can =>
                    {
                        var request = can.Key;
                        var requestBody = request
                            .Parameters
                            .Where(p => p.Type == ParameterType.RequestBody)
                            .Select(p => p.Value as string)
                            .SingleOrDefault();
                        requestBody = requestBody ?? "";
                        return IsJsonEquivalent(requestBody, http.RequestBody);
                    });
            }

            var results = matchingRequests.ToArray();

            if (!results.Any())
            {
                var message = string.Format("No corresponding request-response pair was defined in the test harness for: {0} {1}", method, http.Url.AbsoluteUri);
                if (!string.IsNullOrEmpty(http.RequestBody))
                {
                    message += "\r\n\r\n" + http.RequestBody;
                }
                unservicedRequests.Add(message);
                throw new InvalidOperationException(message);
            }

            var result = results.Single();

            processedRequests.Add(result.Key);

            var response = result.Value;
            if (response.ResponseStatus == ResponseStatus.None)
                response.ResponseStatus = ResponseStatus.Completed;
            return response;
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
            AssertAllRequestsWereReceived();
        }
    }
}
