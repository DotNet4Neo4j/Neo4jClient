using RestSharp;

namespace Neo4jClient.Test
{
    public class NeoHttpResponse : HttpResponse
    {
        public NeoHttpResponse()
        {
            RawBytes = new byte[0];
        }

        public string TestContent
        {
            set
            {
                RawBytes = System.Text.Encoding.ASCII.GetBytes(value);
            }
        }
    }
}