using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class UpdateRelationshipTests
    {
        [Test]
        public void ShouldUpdatePayload()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions'' : {
                          }
                        }".Replace('\'', '"')
                    }
                },
                 {
                    new RestRequest { Resource = "/relationship/456/properties", Method = Method.GET },
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{ 'Foo': 'foo', 'Bar': 'bar', 'Baz': 'baz' }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/relationship/456/properties",
                        Method = Method.PUT,
                        RequestFormat = DataFormat.Json
                    }.AddBody(new TestPayload { Foo = "fooUpdated", Bar = "bar", Baz = "bazUpdated" }),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.NoContent
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            graphClient.Update<TestPayload>(
                new RelationshipReference(456),
                payloadFromDb =>
                {
                    payloadFromDb.Foo = "fooUpdated";
                    payloadFromDb.Baz = "bazUpdated";
                }
            );

            Assert.Inconclusive("Not actually asserting the calls were all made");
        }

        public class TestPayload
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }
    }
}