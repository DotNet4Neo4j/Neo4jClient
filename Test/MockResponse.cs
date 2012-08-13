using System.Net;
using RestSharp;

namespace Neo4jClient.Test
{
    class MockResponse
    {
        public static IHttpResponse Json(HttpStatusCode statusCode, string json)
        {
            return new NeoHttpResponse
            {
                StatusCode = statusCode,
                ContentType = "application/json",
                TestContent = json
            };
        }

        public static IHttpResponse NeoRoot()
        {
            return Json(HttpStatusCode.OK, @"{
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/123',
                'neo4j_version' : '1.5.M02',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {
                }
            }");
        }

        public static IHttpResponse NeoRootPre15M02()
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

        public static IHttpResponse InternalServerError()
        {
            return new NeoHttpResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                StatusDescription = "Internal Server Error"
            };
        }

        public static IHttpResponse Unauthorized()
        {
            return new NeoHttpResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                StatusDescription = "Unauthorized"
            };
        }
    }
}
