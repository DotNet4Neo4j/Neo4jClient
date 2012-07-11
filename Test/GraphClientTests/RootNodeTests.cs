using System;
using NUnit.Framework;
using Neo4jClient.Cypher;
using Neo4jClient.Gremlin;
using RestSharp;
using System.Collections.Generic;
using System.Net;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class RootNodeTests
    {
        const string baseUri = "http://foo/db/data";
        IHttpFactory httpFactory = MockHttpFactory.Generate(baseUri, new Dictionary<IRestRequest, IHttpResponse>
        {
            {
                new RestRequest { Resource = "", Method = Method.GET },
                new NeoHttpResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    ContentType = "application/json",
                    TestContent = @"{
                        'batch' : 'http://foo/db/data/batch',
                        'node' : 'http://foo/db/data/node',
                        'node_index' : 'http://foo/db/data/index/node',
                        'relationship_index' : 'http://foo/db/data/index/relationship',
                        'reference_node' : 'http://foo/db/data/node/123',
                        'neo4j_version' : '1.5.M02',
                        'extensions_info' : 'http://foo/db/data/ext',
                        'extensions' : {
                        }
                    }".Replace('\'', '"')
                }
            }
        });

        [Test]
        public void RootNodeShouldHaveReferenceBackToClient()
        {
            var client = new GraphClient(new Uri(baseUri), httpFactory);
            client.Connect();
            var rootNode = client.RootNode;
            Assert.AreEqual(client, ((IGremlinQuery)rootNode).Client);
        }

        [Test]
        public void RootNodeShouldSupportGremlinQueries()
        {

            var client = new GraphClient(new Uri(baseUri), httpFactory);
            client.Connect();
            var rootNode = client.RootNode;
            Assert.AreEqual("g.v(p0)", ((IGremlinQuery)rootNode).QueryText);
            Assert.AreEqual(123, ((IGremlinQuery)rootNode).QueryParameters["p0"]);
        }
    }
}
