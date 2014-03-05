using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient.Execution
{

    public class ExecutionConfiguration
    {
        public IHttpClient HttpClient { get; set; }
        public bool UseJsonStreaming { get; set; }
        public string UserAgent { get; set; }
        public IEnumerable<JsonConverter> JsonConverters { get; set; }
    }
}