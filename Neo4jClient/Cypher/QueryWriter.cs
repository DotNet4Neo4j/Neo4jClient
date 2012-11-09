using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class QueryWriter
    {
        readonly IDictionary<string, object> queryParameters = new Dictionary<string, object>();
        readonly StringBuilder queryTextBuilder = new StringBuilder();

        public CypherQuery ToCypherQuery()
        {
            var queryText = queryTextBuilder
                .ToString()
                .TrimEnd(Environment.NewLine.ToCharArray());

            return new CypherQuery(
                queryText,
                new Dictionary<string, object>(queryParameters),
                CypherResultMode.Projection);
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
