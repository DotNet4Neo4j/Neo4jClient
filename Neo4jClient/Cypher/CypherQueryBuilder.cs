using System;

namespace Neo4jClient.Cypher
{
    [Obsolete("Use QueryWriter instead")]
    public class CypherQueryBuilder
    {
        readonly QueryWriter queryWriter;

        public CypherQueryBuilder(QueryWriter queryWriter)
        {
            this.queryWriter = queryWriter;
        }

        public QueryWriter QueryWriter
        {
            get { return queryWriter; }
        }
    }
}
