using RestSharp;

namespace Neo4jClient.Test
{
    class MockRequest
    {
        public static IMockRequestDefinition Get(string uri)
        {
            return new NeoHttpRequest {Resource = uri, Method = Method.GET};
        }

        public static IMockRequestDefinition Post(string uri, string jsonBody)
        {
            return new NeoHttpRequest
            {
                Resource = uri,
                Method = Method.POST,
                RequestFormat = DataFormat.Json,
                Body = jsonBody
            };
        }
    }
}
