using System;
using System.Collections.Generic;

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

        CypherQueryBuilder Clone()
        {
            var clonedWriter = queryWriter.Clone();
            return new CypherQueryBuilder(clonedWriter);
        }

        public CypherQueryBuilder CallWriter(Action<QueryWriter> callback)
        {
            return CallWriter((w, cp) => callback(w));
        }

        public CypherQueryBuilder CallWriter(Action<QueryWriter, Func<object, string>> callback)
        {
            var newBuilder = Clone();
            callback(
                newBuilder.queryWriter,
                v => newBuilder.queryWriter.CreateParameter(v));
            return newBuilder;
        }
    }
}
