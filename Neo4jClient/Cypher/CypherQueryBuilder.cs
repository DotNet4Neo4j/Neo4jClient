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
            };
        }

        public CypherQueryBuilder SetResultMode(CypherResultMode resultMode)
        {
            var newBuilder = Clone();
            newBuilder.resultMode = resultMode;
            return newBuilder;
        }

        public CypherQuery ToQuery()
        {
            var textBuilder = new StringBuilder(queryTextBuilder.ToString());
            var parameters = new Dictionary<string, object>(queryParameters);
            var writer = new QueryWriter(textBuilder, parameters);

            return writer.ToCypherQuery(resultMode);
        }

        public static string CreateParameter(IDictionary<string, object> parameters, object paramValue)
        {
            var paramName = string.Format("p{0}", parameters.Count);
            parameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
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
