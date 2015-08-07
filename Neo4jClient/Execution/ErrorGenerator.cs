using System;
using System.Net.Http;

namespace Neo4jClient.Execution
{
    internal class ErrorGenerator
    {
        public Func<HttpResponseMessage, bool> Condition { get; set; }
        public Func<HttpResponseMessage, Exception> Generator { get; set; }
    }
}