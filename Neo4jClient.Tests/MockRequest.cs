using System.Net.Http;
using Neo4jClient.Serialization;

namespace Neo4jClient.Tests
{
    public class MockRequest
    {
        MockRequest() {}

        public HttpMethod Method { get; set; }
        public string Resource { get; set; }
        public string Body { get; set; }

        public static MockRequest Get(string uri)
        {
            if (uri == "") uri = "/";
            return new MockRequest { Resource = uri, Method = HttpMethod.Get };
        }

        public static MockRequest PostJson(string uri, string json)
        {
            return new MockRequest
            {
                Resource = uri,
                Method = HttpMethod.Post,
                Body = json
            };
        }

        public static MockRequest PostObjectAsJson(string uri, object body)
        {
            var test = new MockRequest
            {
                Resource = uri,
                Method = HttpMethod.Post,
                Body = new CustomJsonSerializer { JsonConverters = GraphClient.DefaultJsonConverters }.Serialize(body)
            };
            
            return new MockRequest
            {
                Resource = uri,
                Method = HttpMethod.Post,
                Body = new CustomJsonSerializer{JsonConverters = GraphClient.DefaultJsonConverters}.Serialize(body)
            };
        }

        public static MockRequest PutObjectAsJson(string uri, object body)
        {
            return new MockRequest
            {
                Resource = uri,
                Method = HttpMethod.Put,
                Body = new CustomJsonSerializer{JsonConverters = GraphClient.DefaultJsonConverters}.Serialize(body)
            };
        }

        public static MockRequest Delete(string uri)
        {
            return new MockRequest { Resource = uri, Method = HttpMethod.Delete };
        }
    }
}
