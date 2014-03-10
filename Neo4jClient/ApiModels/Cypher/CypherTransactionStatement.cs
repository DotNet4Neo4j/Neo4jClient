using System.Collections.Generic;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Cypher
{
    /// <summary>
    /// Very similar to CypherApiQuery but it's used for opened transactions as their serialization
    /// is different
    /// </summary>
    class CypherTransactionStatement
    {
        private readonly string _queryText;
        private readonly IDictionary<string, object> _queryParameters;
        private readonly string[] _formatContents; 

        public CypherTransactionStatement(CypherQuery query, bool restFormat)
        {
            _queryText = query.QueryText;
            _queryParameters = query.QueryParameters ?? new Dictionary<string, object>();
            _formatContents = restFormat ? new[] {"REST"} : new string[] {};
        }

        [JsonProperty("statement")]
        public string Statement
        {
            get { return _queryText; }
        }

        [JsonProperty("resultDataContents")]
        public IEnumerable<string> ResultDataContents
        {
            get { return _formatContents; }
        }

        [JsonProperty("parameters")]
        public IDictionary<string, object> Parameters
        {
            get { return _queryParameters; }
        }
    }
}
