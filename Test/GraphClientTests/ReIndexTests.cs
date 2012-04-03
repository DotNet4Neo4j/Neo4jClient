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
        readonly string rootResponse = @"{
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/0',
                'neo4j_version' : '1.5.M02',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {
                }
            }"
            .Replace('\'', '"');

        readonly string pre15M02RootResponse = @"{
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/0',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {
                }
            }"
            .Replace('\'', '"');

        [Test]
        public void ShouldReindexNodeWithIndexEntryContainingSpace()
        {
            //Arrange
            var indexEntries = new List<IndexEntry>
            {
                new IndexEntry
                {
                    Name = "my_nodes",
                    KeyValues = new Dictionary<string, object>
                    {
                        { "FooKey", "the_value with space" }
                    },
                }
            };

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = rootResponse.Replace('\'', '"')
                    }
                },
                {
                    new RestRequest("/index/node/my_nodes", Method.POST)
                    {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                    }
                    .AddBody(new { key="FooKey", value="the_value with space", uri="http://foo/db/data/node/123"}),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        TestContent = "Location: http://foo/db/data/index/node/my_nodes/FooKey/the_value%20with%20space/123"
                    }
                },
                {
                   new RestRequest("/index/node/my_nodes/123", Method.DELETE) {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                        },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NoContent,
                        ContentType = "application/json",
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
        public void ShouldReindexNodeWithDateTimeOffsetIndexEntry()
        {
            //Arrange
            var indexEntries = new List<IndexEntry>
            {
                new IndexEntry
                {
                    Name = "my_nodes",
                    KeyValues = new Dictionary<string, object>
                    {
                        { "FooKey", new DateTimeOffset(1000, new TimeSpan(0)) }
                    },
                }
            };

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = rootResponse.Replace('\'', '"')
                    }
                },
                {
                    new RestRequest("/index/node/my_nodes", Method.POST)
                    {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                    }
                    .AddBody(new { key="FooKey", value="1000", uri="http://foo/db/data/node/123"}),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        TestContent = "Location: http://foo/db/data/index/node/my_nodes/FooKey/the_value%20with%20space/123"
                    }
                },
                {
                   new RestRequest("/index/node/my_nodes/123", Method.DELETE) {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                        },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NoContent,
                        ContentType = "application/json",
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
        public void ShouldAcceptQuestionMarkInIndexValue()
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

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = rootResponse.Replace('\'', '"')
                    }
                },
                {
                    new RestRequest("/index/node/my_nodes", Method.POST)
                    {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                    }
                    .AddBody(new { key="FooKey", value="foo?bar", uri="http://foo/db/data/node/123"}),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        TestContent = "Location: http://foo/db/data/index/node/my_nodes/FooKey/%3F/123"
                    }
                },
                {
                   new RestRequest("/index/node/my_nodes/123", Method.DELETE) {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                        },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NoContent,
                        ContentType = "application/json",
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
        public void ShouldPreserveSlashInIndexValue()
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

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = rootResponse.Replace('\'', '"')
                    }
                },
                {
                    new RestRequest("/index/node/my_nodes", Method.POST)
                    {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                    }
                    .AddBody(new { key = "FooKey", value ="abc/def", uri = "http://foo/db/data/node/123"}),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        TestContent = "Location: http://foo/db/data/index/node/my_nodes/FooKey/abc-def/123"
                    }
                },
                {
                   new RestRequest("/index/node/my_nodes/123", Method.DELETE) {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                        },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NoContent,
                        ContentType = "application/json",
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
        [ExpectedException(typeof(NotSupportedException))]
        public void ShouldThrowNotSupportExceptionForPre15M02Database()
        {
            //Arrange
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = pre15M02RootResponse.Replace('\'', '"')
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var nodeReference = new NodeReference<TestNode>(123);
            graphClient.ReIndex(nodeReference, new IndexEntry[0]);
        }
    }

    public class TestNode
    {
        public string FooKey { get; set; }
    }

}