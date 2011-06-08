using System;
using System.Collections.Generic;
using System.Net;
using Neo4jClient;
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
            var httpFactory = MockHttpFactory.Generate("http://foo", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse { StatusCode = HttpStatusCode.InternalServerError }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo"), httpFactory);
            graphClient.Connect();
        }
    }
}