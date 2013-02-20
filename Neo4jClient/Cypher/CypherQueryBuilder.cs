using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.Cypher
{
    [Obsolete("Use QueryWriter instead")]
    public class CypherQueryBuilder
    {
        readonly QueryWriter queryWriter;
        readonly StringBuilder queryTextBuilder;
        readonly IDictionary<string, object> queryParameters;

        CypherResultMode resultMode;
        int? limit;

        public CypherQueryBuilder()
        {
            queryTextBuilder = new StringBuilder();
            queryParameters = new Dictionary<string, object>();
            queryWriter = new QueryWriter(queryTextBuilder, queryParameters);
        }

        public CypherQueryBuilder(
            QueryWriter queryWriter,
            StringBuilder queryTextBuilder,
            IDictionary<string, object> queryParameters)
        {
            this.queryWriter = queryWriter;
            this.queryTextBuilder = queryTextBuilder;
            this.queryParameters = queryParameters;
        }

        CypherQueryBuilder Clone()
        {
            var clonedQueryTextBuilder = new StringBuilder(queryTextBuilder.ToString());
            var clonedParameters = new Dictionary<string, object>(queryParameters);
            var clonedWriter = new QueryWriter(clonedQueryTextBuilder, clonedParameters);
            return new CypherQueryBuilder(
                clonedWriter,
                clonedQueryTextBuilder,
                clonedParameters
            )
            {
                resultMode = resultMode,
                limit = limit,
            };
        }

        public CypherQueryBuilder SetResultMode(CypherResultMode resultMode)
        {
            var newBuilder = Clone();
            newBuilder.resultMode = resultMode;
            return newBuilder;
        }

        [Obsolete]
        public CypherQueryBuilder SetLimit(int? count)
        {
            var newBuilder = Clone();
            newBuilder.limit = count;
            return newBuilder;
        }

        public CypherQuery ToQuery()
        {
            var textBuilder = new StringBuilder(queryTextBuilder.ToString());
            var parameters = new Dictionary<string, object>(queryParameters);
            var writer = new QueryWriter(textBuilder, parameters);

            WriteLimitClause(textBuilder, parameters);

            return writer.ToCypherQuery(resultMode);
        }

        public static string CreateParameter(IDictionary<string, object> parameters, object paramValue)
        {
            var paramName = string.Format("p{0}", parameters.Count);
            parameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
        }

        void WriteLimitClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            if (limit == null) return;
            target.AppendFormat("LIMIT {0}", CreateParameter(paramsDictionary, limit));
            target.AppendLine();
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
                v => CreateParameter(newBuilder.queryParameters, v));
            return newBuilder;
        }
    }
}
