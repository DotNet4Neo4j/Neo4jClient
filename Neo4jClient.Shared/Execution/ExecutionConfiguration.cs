using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient.Execution
{
    public class ExecutionConfiguration
    {
        public ExecutionConfiguration()
        {
            ResourceManagerId = new Guid("{BB792575-FAA7-4C72-A6B1-A69876CC3E1E}");
        }

        public IHttpClient HttpClient { get; set; }
        public bool UseJsonStreaming { get; set; }
        public string UserAgent { get; set; }
        public IEnumerable<JsonConverter> JsonConverters { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool HasErrors { get; set; }
        public Guid ResourceManagerId { get; set; }
        public string Realm { get; set; }
    }
}