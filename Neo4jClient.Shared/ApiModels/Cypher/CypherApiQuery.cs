using System.Collections.Generic;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Cypher
{
    class CypherApiQuery
    {
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;

        public CypherApiQuery(CypherQuery query)
        {
            queryText = query.QueryText;
            queryParameters = query.QueryParameters ?? new Dictionary<string, object>();
        }

        [JsonProperty("query")]
        public string Query
        {
            get { return queryText; }
        }

        [JsonProperty("params")]
        public IDictionary<string, object> Parameters
        {
            get { return queryParameters; }
        }
    }
}
