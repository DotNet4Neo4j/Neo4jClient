using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Neo4j.Driver;
using Neo4jClient.Serialization;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Cypher
{
    public enum CypherResultFormat
    {
        Rest,
        Transactional,
        DependsOnEnvironment
    }

    [DebuggerDisplay("{DebugQueryText}")]
    public class CypherQuery
    {
        private readonly string queryText;
        private readonly IDictionary<string, object> queryParameters;

        public CypherQuery(
            string queryText,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode, 
            string database,
            IContractResolver contractResolver = null) :
            this(queryText, queryParameters, resultMode, CypherResultFormat.DependsOnEnvironment, database, contractResolver)
        {
        }

        public CypherQuery(
            string queryText,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode,
            CypherResultFormat resultFormat,
            string database,
            IContractResolver contractResolver = null, 
            int? maxExecutionTime = null, 
            NameValueCollection customHeaders = null,
            bool isWrite = true,
            IEnumerable<Bookmark> bookmarks = null,
            string identifier = null,
            bool includeQueryStats = false
            )
        {
            this.queryText = queryText;
            this.queryParameters = queryParameters;
            this.ResultMode = resultMode;
            this.ResultFormat = resultFormat;
            JsonContractResolver = contractResolver ?? GraphClient.DefaultJsonContractResolver;
            this.MaxExecutionTime = maxExecutionTime;
            this.CustomHeaders = customHeaders;
            IsWrite = isWrite;
            IncludeQueryStats = includeQueryStats;
            Bookmarks = bookmarks;
            Identifier = identifier;
            Database = database;
        }

        public bool IsWrite { get; }
        public bool IncludeQueryStats { get; }

        public string Identifier { get; set; }

        public IEnumerable<Bookmark> Bookmarks { get; set; }

        public IDictionary<string, object> QueryParameters => queryParameters;

        public string QueryText => queryText;

        public CypherResultFormat ResultFormat { get; }

        public CypherResultMode ResultMode { get; }

        public IContractResolver JsonContractResolver { get; }

        public string Database { get; }

        public int? MaxExecutionTime { get; }

        /// <summary>
        /// Custom headers to add to REST calls to Neo4j server.
        /// Example usage: This can be used to provide extra information to a Neo4j Loadbalancer. 
        /// </summary>
        public NameValueCollection CustomHeaders { get; }

        private CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer { JsonConverters = GraphClient.DefaultJsonConverters, JsonContractResolver = JsonContractResolver };
        }

        public string DebugQueryText
        {
            get
            {
                if (queryParameters == null)
                {
                    return queryText;
                }

                var serializer = BuildSerializer();
                var text = queryParameters
                    .Keys
                    .Aggregate(
                        queryText,
                        (current, paramName) =>
                        {
                            var value = queryParameters[paramName];
                            value = serializer.Serialize(value);                           
                            return current.Replace($"${paramName}", value.ToString());
                        });

                return text;
            }
        }
    }
}
