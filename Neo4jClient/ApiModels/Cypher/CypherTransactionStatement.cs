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

        public CypherTransactionStatement(CypherQuery query)
        {
            _queryText = query.QueryText;
            _queryParameters = query.QueryParameters ?? new Dictionary<string, object>();
        }

        [JsonProperty("statement")]
        public string Statement
        {
            get { return _queryText; }
        }

        [JsonProperty("parameters")]
        public IDictionary<string, object> Parameters
        {
            get { return _queryParameters; }
        }
    }
}
