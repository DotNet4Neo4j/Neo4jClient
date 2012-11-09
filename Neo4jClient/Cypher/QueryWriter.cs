using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class QueryWriter
    {
        readonly StringBuilder queryTextBuilder;
        readonly IDictionary<string, object> queryParameters;

        public QueryWriter()
        {
            queryTextBuilder = new StringBuilder();
            queryParameters = new Dictionary<string, object>();
        }

        [Obsolete]
        internal QueryWriter(
            StringBuilder queryTextBuilder,
            IDictionary<string, object> queryParameters)
        {
            this.queryTextBuilder = queryTextBuilder;
            this.queryParameters = queryParameters;
        }

        public CypherQuery ToCypherQuery(CypherResultMode resultMode = CypherResultMode.Projection)
        {
            var queryText = queryTextBuilder
                .ToString()
                .TrimEnd(Environment.NewLine.ToCharArray());

            return new CypherQuery(
                queryText,
                new Dictionary<string, object>(queryParameters),
                resultMode);
        }

        public void AppendClause(string clause, params object[] paramValues)
        {
            if (paramValues.Any())
            {
                var paramPlaceholders = paramValues
                    .Select(CreateParameterAndReturnPlaceholder)
                    .Cast<object>()
                    .ToArray();
                clause = string.Format(clause, paramPlaceholders);
            }

            // Only needed while migrating off CypherQueryBuilder
            if (queryTextBuilder.Length > 0 &&
                !queryTextBuilder.ToString().EndsWith(Environment.NewLine))
            {
                queryTextBuilder.AppendLine();
            }

            queryTextBuilder.AppendLine(clause);
        }

        string CreateParameterAndReturnPlaceholder(object paramValue)
        {
            var paramName = string.Format("p{0}", queryParameters.Count);
            queryParameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
        }
    }
}
