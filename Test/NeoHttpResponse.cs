using System;
using RestSharp;

namespace Neo4jClient.Test
{
    [Obsolete("Use MockResponse instead so that we can move to HTTP-client agnostic tests")]
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