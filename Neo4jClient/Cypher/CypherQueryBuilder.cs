using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class CypherQueryBuilder
    {
        IList<CypherStartBit> startBits = new List<CypherStartBit>();
        string matchText;
        string whereText;
        string returnText;
        bool returnDistinct;
        int? limit;
        string orderBy;

        CypherQueryBuilder Clone()
        {
            return new CypherQueryBuilder
            {
                matchText = matchText,
                whereText = whereText,
                returnText = returnText,
                returnDistinct = returnDistinct,
                limit = limit,
                startBits = startBits,
                orderBy = orderBy
            };
        }

        public CypherQueryBuilder AddStartBit(string identity, params NodeReference[] nodeReferences)
        {
            var newBuilder = Clone();
            newBuilder.startBits.Add(new CypherStartBit(identity, "node", nodeReferences.Select(r => r.Id).ToArray()));
            return newBuilder;
        }

        public CypherQueryBuilder AddStartBit(string identity, params RelationshipReference[] relationshipReferences)
        {
            var newBuilder = Clone();
            newBuilder.startBits.Add(new CypherStartBit(identity, "relationship", relationshipReferences.Select(r => r.Id).ToArray()));
            return newBuilder;
        }

        public CypherQueryBuilder SetMatchText(string text)
        {
            var newBuilder = Clone();
            newBuilder.matchText = text;
            return newBuilder;
        }

        public CypherQueryBuilder SetWhere(LambdaExpression expression)
        {
            var newBuilder = Clone();
            newBuilder.whereText = CypherWhereExpressionBuilder.BuildText(expression);
            return newBuilder;
        }

        public CypherQueryBuilder SetReturn(string identity, bool distinct)
        {
            var newBuilder = Clone();
            newBuilder.returnText = identity;
            newBuilder.returnDistinct = distinct;
            return newBuilder;
        }

        public CypherQueryBuilder SetReturn(LambdaExpression expression, bool distinct)
        {
            var newBuilder = Clone();
            newBuilder.returnText = CypherReturnExpressionBuilder.BuildText(expression);
            newBuilder.returnDistinct = distinct;
            return newBuilder;
        }

        public CypherQueryBuilder SetLimit(int? count)
        {
            var newBuilder = Clone();
            newBuilder.limit = count;
            return newBuilder;
        }

        public CypherQueryBuilder SetOrderBy(OrderByType orderByType, params string[] properties)
        {
            var newBuilder = Clone();
            newBuilder.orderBy = string.Join(", ", properties);

            if (orderByType == OrderByType.Descending)
                newBuilder.orderBy += " DESC";

            return newBuilder;
        }

        public CypherQuery ToQuery()
        {
            var queryTextBuilder = new StringBuilder();
            var queryParameters = new Dictionary<string, object>();

            WriteStartClause(queryTextBuilder, queryParameters);
            WriteMatchClause(queryTextBuilder);
            WriteWhereClause(queryTextBuilder);
            WriteReturnClause(queryTextBuilder);
            WriteLimitClause(queryTextBuilder, queryParameters);
            WriteOrderByClause(queryTextBuilder, queryParameters);

            return new CypherQuery(queryTextBuilder.ToString(), queryParameters);
        }

        static string CreateParameter(IDictionary<string, object> parameters, object paramValue)
        {
            var paramName = string.Format("p{0}", parameters.Count);
            parameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
        }

        void WriteStartClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            target.Append("START ");

            var formattedStartBits = startBits.Select(bit =>
            {
                var lookupIdParameterNames = bit
                    .LookupIds
                    .Select(i => CreateParameter(paramsDictionary, i))
                    .ToArray();

                var lookupContent = string.Join(", ", lookupIdParameterNames);

                return string.Format("{0}={1}({2})", bit.Identifier, bit.LookupType, lookupContent);
            });

            target.Append(string.Join(", ", formattedStartBits));
        }

        void WriteMatchClause(StringBuilder target)
        {
            if (matchText == null) return;
            target.AppendFormat("\r\nMATCH {0}", matchText);
        }

        void WriteWhereClause(StringBuilder target)
        {
            if (whereText == null) return;
            target.Append("\r\nWHERE ");
            target.Append(whereText);
        }

        void WriteReturnClause(StringBuilder target)
        {
            if (returnText == null) return;
            target.Append("\r\nRETURN ");
            if (returnDistinct) target.Append("distinct ");
            target.Append(returnText);
        }

        void WriteLimitClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            if (limit == null) return;
            target.AppendFormat("\r\nLIMIT {0}", CreateParameter(paramsDictionary, limit));
        }

        void WriteOrderByClause(StringBuilder target, IDictionary<string, object> paramsDictionary )
        {
            if (string.IsNullOrEmpty(orderBy)) return;
            target.AppendFormat("\r\nORDER BY {0}", CreateParameter(paramsDictionary, orderBy));
        }
    }
}
