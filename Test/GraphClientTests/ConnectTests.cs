using System;
using System.Net;
using Neo4jClient;
using NSubstitute;
using NUnit.Framework;
using RestSharp;

namespace Test.GraphClientTests
{
    [TestFixture]
    public class ConnectTests
    {
        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Failed to connect to server.")]
        public void ShouldThrowConnectionExceptionFor500Response()
        {
            var httpFactory = Substitute.For<IHttpFactory>();
            httpFactory
                .Create()
                .Returns(callInfo =>
                {
                    var http = Substitute.For<IHttp>();
                    http.Get().Returns(ci =>
                        http.Url == new Uri("http://foo/")
                        ? new HttpResponse {StatusCode = HttpStatusCode.InternalServerError}
                        : null);
                    return http;
                });

            var graphClient = new GraphClient(new Uri("http://foo"), httpFactory);
            graphClient.Connect();
        }
    }
}