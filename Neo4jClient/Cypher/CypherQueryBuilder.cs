using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class CypherQueryBuilder
    {
        IDictionary<string, object> queryParameters = new Dictionary<string, object>();
        IList<CypherStartBit> startBits = new List<CypherStartBit>();
        string matchText;
        string relateText;
        string whereText;
        string deleteText;
        string returnText;
        bool returnDistinct;
        CypherResultMode resultMode;
        int? limit;
        int? skip;
        string orderBy;

        CypherQueryBuilder Clone()
        {
            return new CypherQueryBuilder
            {
                queryParameters = queryParameters,
                deleteText = deleteText,
                matchText = matchText,
                relateText = relateText,
                whereText = whereText,
                returnText = returnText,
                returnDistinct = returnDistinct,
                resultMode = resultMode,
                limit = limit,
                skip = skip,
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

        public CypherQueryBuilder SetDeleteText(string text)
        {
            var newBuilder = Clone();
            newBuilder.deleteText = text;
            return newBuilder;
        }

        public CypherQueryBuilder SetMatchText(string text)
        {
            var newBuilder = Clone();
            newBuilder.matchText = text;
            return newBuilder;
        }

        public CypherQueryBuilder SetRelateText(string text)
        {
            var newBuilder = Clone();
            newBuilder.relateText = text;
            return newBuilder;
        }

        public CypherQueryBuilder SetWhere(string text)
        {
            var newBuilder = Clone();
            newBuilder.whereText += string.Format("({0})", text);
            return newBuilder;
        }

        public CypherQueryBuilder SetWhere(LambdaExpression expression)
        {
            var newBuilder = Clone();
            newBuilder.whereText += whereText = CypherWhereExpressionBuilder.BuildText(expression, queryParameters);
            return newBuilder;
        }

        public CypherQueryBuilder SetAnd()
        {
            var newBuilder = Clone();
            newBuilder.whereText += " AND ";
            return newBuilder;
        }

        public CypherQueryBuilder SetOr()
        {
            var newBuilder = Clone();
            newBuilder.whereText += " OR ";
            return newBuilder;
        }

        public CypherQueryBuilder SetReturn(string identity, bool distinct, CypherResultMode mode = CypherResultMode.Set)
        {
            var newBuilder = Clone();
            newBuilder.returnText = identity;
            newBuilder.returnDistinct = distinct;
            newBuilder.resultMode = mode;
            return newBuilder;
        }

        public CypherQueryBuilder SetReturn(LambdaExpression expression, bool distinct)
        {
            var newBuilder = Clone();
            newBuilder.returnText = CypherReturnExpressionBuilder.BuildText(expression);
            newBuilder.returnDistinct = distinct;
            newBuilder.resultMode = CypherResultMode.Projection;
            return newBuilder;
        }

        public CypherQueryBuilder SetLimit(int? count)
        {
            var newBuilder = Clone();
            newBuilder.limit = count;
            return newBuilder;
        }

        public CypherQueryBuilder SetSkip(int? count)
        {
            var newBuilder = Clone();
            newBuilder.skip = count;
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
            WriteStartClause(queryTextBuilder, queryParameters);
            WriteMatchClause(queryTextBuilder);
            WriteRelateClause(queryTextBuilder);
            WriteWhereClause(queryTextBuilder);
            WriteReturnClause(queryTextBuilder);
            WriteOrderByClause(queryTextBuilder);
            WriteSkipClause(queryTextBuilder, queryParameters);
            WriteLimitClause(queryTextBuilder, queryParameters);
            WriteDeleteClause(queryTextBuilder);
            return new CypherQuery(queryTextBuilder.ToString(), queryParameters, resultMode);
        }

        public static string CreateParameter(IDictionary<string, object> parameters, object paramValue)
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

        void WriteDeleteClause(StringBuilder target)
        {
            if (deleteText == null) return;
            target.AppendFormat("\r\nDELETE {0}", deleteText);
        }

        void WriteRelateClause(StringBuilder target)
        {
            if (relateText == null) return;
            target.AppendFormat("\r\nRELATE {0}", relateText);
        }

        void WriteWhereClause(StringBuilder target)
        {
            if (whereText == null)
                return;

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

        void WriteSkipClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            if (skip == null) return;
            target.AppendFormat("\r\nSKIP {0}", CreateParameter(paramsDictionary, skip));
        }

        void WriteOrderByClause(StringBuilder target )
        {
            if (string.IsNullOrEmpty(orderBy)) return;
            target.AppendFormat("\r\nORDER BY {0}",  orderBy);
        }
    }
}
