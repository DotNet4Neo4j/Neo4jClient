using System.Net;

namespace Neo4jClient.Tests
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
        public string Location { get; set; }

        public static MockResponse Json(int statusCode, string json)
        {
            return Json((HttpStatusCode)statusCode, json, null);
        }

        public static MockResponse Json(int statusCode, string json, string location)
        {
            return Json((HttpStatusCode)statusCode, json, location);
        }

        public static MockResponse Json(HttpStatusCode statusCode, string json)
        {
            return Json(statusCode, json, null);
        }

        public static MockResponse Json(HttpStatusCode statusCode, string json, string location)
        {
            return new MockResponse
            {
                StatusCode = statusCode,
                ContentType = "application/json",
                Content = json,
                Location = location
            };
        }

        // public static MockResponse NeoRoot()
        // {
        //     return Json(HttpStatusCode.OK, @"{
        //         'cypher' : 'http://foo/db/data/cypher',
        //         'batch' : 'http://foo/db/data/batch',
        //         'node' : 'http://foo/db/data/node',
        //         'node_index' : 'http://foo/db/data/index/node',
        //         'relationship_index' : 'http://foo/db/data/index/relationship',
        //         'reference_node' : 'http://foo/db/data/node/123',
        //         'neo4j_version' : '1.5.M02',
        //         'extensions_info' : 'http://foo/db/data/ext',
        //         'extensions' : {
        //             'GremlinPlugin' : {
        //                 'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
        //             }
        //         }
        //     }");
        // }

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
                'transaction': 'http://foo/db/data/transaction',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {}
            }");
        }

        public static MockResponse NeoRoot(int v1, int v2, int v3)
        {
            if (v1 >= 4)
                return NeoRoot40(v1, v2, v3);

            var version = $"{v1}.{v2}.{v3}";
            return Json(HttpStatusCode.OK, string.Format(@"{{
                'cypher' : 'http://foo/db/data/cypher',
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/123',
                'neo4j_version' : '{0}',
                'transaction': 'http://foo/db/data/transaction',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {{}}
            }}", version));
        }

        public static MockResponse NeoRoot40(int v1, int v2, int v3)
        {
            var version = $"{v1}.{v2}.{v3}";
            return Json(HttpStatusCode.OK, string.Format(@"{{
               'bolt_direct': 'bolt://foo:7687',
               'bolt_routing': 'neo4j://foo:7687',
               'cluster': 'http://foo:7474/db/{{databaseName}}/cluster',
               'transaction': 'http://foo:7474/db/{{databaseName}}/tx',
               'neo4j_version' : '{0}',
               'neo4j_edition': 'enterprise'
            }}", version));
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
