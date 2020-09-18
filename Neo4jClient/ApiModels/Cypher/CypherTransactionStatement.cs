using System.Collections.Generic;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Cypher
{
    /// <summary>
    /// Very similar to CypherApiQuery but it's used for opened transactions as their serialization
    /// is different
    /// </summary>
    internal class CypherTransactionStatement
    {
        private readonly string[] formatContents; 

        public CypherTransactionStatement(CypherQuery query)
        {
            Statement = query.QueryText;
            Parameters = query.QueryParameters ?? new Dictionary<string, object>();
            formatContents = new string[] {};
            if (query.IncludeQueryStats)
                IncludeStats = query.IncludeQueryStats;
        }

        [JsonProperty("statement")]
        public string Statement { get; }

        [JsonProperty("resultDataContents")]
        public IEnumerable<string> ResultDataContents => formatContents;

        [JsonProperty("parameters")]
        public IDictionary<string, object> Parameters { get; }

        [JsonProperty("includeStats", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IncludeStats { get; }
    }
}
