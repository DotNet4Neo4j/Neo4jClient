using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
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