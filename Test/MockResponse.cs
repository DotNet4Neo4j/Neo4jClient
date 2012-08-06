using System.Net;

namespace Neo4jClient.Test
{
    class MockResponse
    {
        public static NeoHttpResponse Json(HttpStatusCode statusCode, string json)
        {
            return new NeoHttpResponse
            {
                StatusCode = statusCode,
                ContentType = "application/json",
                TestContent = json
            };
        }
    }
}
