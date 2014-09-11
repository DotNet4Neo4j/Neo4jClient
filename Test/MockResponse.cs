using System.Net;

namespace Neo4jClient.Test
{
    public class MockResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string StatusDescription
        {
            get { return StatusCode.ToString(); }
        }

        public string ContentType { get; set; }
        public virtual string Content { get; set; }

        public static MockResponse Json(HttpStatusCode statusCode, string json)
        {
            return new MockResponse
            {
                StatusCode = statusCode,
                ContentType = "application/json",
                Content = json
            };
        }

        public static MockResponse NeoRoot()
        {
            return Json(HttpStatusCode.OK, @"{
                'cypher' : 'http://foo/db/data/cypher',
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/123',
                'neo4j_version' : '1.5.M02',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {
                    'GremlinPlugin' : {
                        'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                    }
                }
            }");
        }

        public static MockResponse NeoRoot20()
        {
            return Json(HttpStatusCode.OK, @"{
                'cypher' : 'http://foo/db/data/cypher',
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/123',
                'neo4j_version' : '2.0.M06',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {}
            }");
        }

        public static MockResponse NeoRootPre15M02()
        {
            return Json(HttpStatusCode.OK, @"{
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/123',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {
                }
            }");
        }

        public static MockResponse Http(int statusCode)
        {
            return new MockResponse
            {
                StatusCode = (HttpStatusCode)statusCode
            };
        }

        public static MockResponse Throws()
        {
            return new MockResponseThrows();
        }
    }
}
