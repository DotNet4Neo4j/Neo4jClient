using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Neo4jClient.Serializer;
using Newtonsoft.Json;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class ReIndexTests
    {
        const string RootResponse = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                            'GremlinPlugin' : {
                              'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                            }
                          }
                        }";

        [Test]
        public void ShouldReturnHttpResponse201WhenCreatingAnIndexOfTypeFullText()
        {
            //Arrange
            IDictionary<string, object> indexkeyValues = new Dictionary<string, object>();
            indexkeyValues.Add("FooKey", "the_value with space");
            var indexEntries = new List<IndexEntry>
                {
                   new IndexEntry
                       {
                           Name = "my_nodes",
                           KeyValues = indexkeyValues,
                       }
                };


            var restRequest = new RestRequest("/index/node/my_nodes/FooKey/the_value%20with%20space", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };

            restRequest.AddBody("http://foo/db/data/node/123");

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = RootResponse.Replace('\'', '"')
                    }
                },
                {
                    restRequest,
                    new HttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        Content = "Location: http://foo/db/data/index/node/my_nodes/FooKey/the_value%20with%20space/123"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var nodeReference = new NodeReference<TestNode>(123);
            graphClient.ReIndex(nodeReference, indexEntries);

            // Assert
            Assert.Pass("Success.");
        }

        [Test]
        public void ShouldReturnHttpResponse201WhenCreatingAnIndexOfTypeFullTextWithADateTimeOffsetIndexValue()
        {
            //Arrange
            IDictionary<string, object> indexkeyValues = new Dictionary<string, object>();
            indexkeyValues.Add("FooKey", new DateTimeOffset(1000L,new TimeSpan(0)));
            var indexEntries = new List<IndexEntry>
                {
                   new IndexEntry
                       {
                           Name = "my_nodes",
                           KeyValues = indexkeyValues,
                       }
                };


            var restRequest = new RestRequest("/index/node/my_nodes/FooKey/1000", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };

            restRequest.AddBody("http://foo/db/data/node/123");

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = RootResponse.Replace('\'', '"')
                    }
                },
                {
                    restRequest,
                    new HttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        Content = "Location: http://foo/db/data/index/node/my_nodes/FooKey/1000/123"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var nodeReference = new NodeReference<TestNode>(123);
            graphClient.ReIndex(nodeReference, indexEntries);

            // Assert
            Assert.Pass("Success.");
        }

        [Test]
        public void ShouldUrlEncodeQuestionMarkInIndexValue()
        {
            //Arrange
            var indexKeyValues = new Dictionary<string, object>
            {
                {"FooKey", "foo?bar"}
            };
            var indexEntries = new List<IndexEntry>
            {
                new IndexEntry
                {
                    Name = "my_nodes",
                    KeyValues = indexKeyValues,
                }
            };

            var restRequest = new RestRequest("/index/node/my_nodes/FooKey/foo%3Fbar", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };

            restRequest.AddBody("http://foo/db/data/node/123");

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = RootResponse.Replace('\'', '"')
                    }
                },
                {
                    restRequest,
                    new HttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        Content = "Location: http://foo/db/data/index/node/my_nodes/FooKey/%3F/123"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var nodeReference = new NodeReference<TestNode>(123);
            graphClient.ReIndex(nodeReference, indexEntries);

            // Assert
            Assert.Pass("Success.");
        }

        [Test]
        public void ShouldReplaceSlashInIndexValueWithDash()
        {
            //Arrange
            var indexKeyValues = new Dictionary<string, object>
            {
                {"FooKey", "abc/def"}
            };
            var indexEntries = new List<IndexEntry>
            {
                new IndexEntry
                {
                    Name = "my_nodes",
                    KeyValues = indexKeyValues,
                }
            };

            var restRequest = new RestRequest("/index/node/my_nodes/FooKey/abc-def", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };

            restRequest.AddBody("http://foo/db/data/node/123");

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = RootResponse.Replace('\'', '"')
                    }
                },
                {
                    restRequest,
                    new HttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        Content = "Location: http://foo/db/data/index/node/my_nodes/FooKey/abc-def/123"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var nodeReference = new NodeReference<TestNode>(123);
            graphClient.ReIndex(nodeReference, indexEntries);

            // Assert
            Assert.Pass("Success.");
        }
    }

    public class TestNode
    {
        public string FooKey { get; set; }
    }

}