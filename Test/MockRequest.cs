using RestSharp;

namespace Neo4jClient.Test
{
    class MockRequest
    {
        public static IMockRequestDefinition Get(string uri)
        {
            return new NeoHttpRequest {Resource = uri, Method = Method.GET};
        }

        public static IMockRequestDefinition PostJson(string uri, string json)
        {
            return new NeoHttpRequest
            {
                Resource = uri,
                Method = Method.POST,
                RequestFormat = DataFormat.Json,
                Body = json
            };
        }

        public static IMockRequestDefinition PostObjectAsJson(string uri, object body)
        {
            return new NeoHttpRequest
            {
                Resource = uri,
                Method = Method.POST,
                RequestFormat = DataFormat.Json,
                Body = body
            };
        }

        public static IMockRequestDefinition Delete(string uri)
        {
            return new NeoHttpRequest { Resource = uri, Method = Method.DELETE };
        }
    }
}
