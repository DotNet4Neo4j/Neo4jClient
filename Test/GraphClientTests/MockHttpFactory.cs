using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    public static class MockHttpFactory
    {
        public static IHttpFactory Generate(string baseUri, IDictionary<RestRequest, HttpResponse> cannedResponses)
        {
            var httpFactory = Substitute.For<IHttpFactory>();
            httpFactory
                .Create()
                .Returns(callInfo =>
                {
                    var http = Substitute.For<IHttp>();
                    http.Get().Returns(ci => HandleRequest(http, Method.GET, baseUri, cannedResponses));
                    http.Post().Returns(ci => HandleRequest(http, Method.POST, baseUri, cannedResponses));
                    return http;
                });
            return httpFactory;
        }

        static HttpResponse HandleRequest(IHttp http, Method method, string baseUri, IEnumerable<KeyValuePair<RestRequest, HttpResponse>> cannedResponses)
        {
            return cannedResponses
                .Where(can => http.Url.AbsoluteUri == baseUri + can.Key.Resource)
                .Where(can => can.Key.Method == method)
                .Select(can =>
                {
                    var response = can.Value;
                    if (response.ResponseStatus == ResponseStatus.None)
                        response.ResponseStatus = ResponseStatus.Completed;
                    return response;
                })
                .SingleOrDefault();
        }
    }
}