using System;
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
        string returnText;
        bool returnDistinct;
        int? limit;

        CypherQueryBuilder Clone()
        {
            return new CypherQueryBuilder
            {
                matchText = matchText,
                returnText = returnText,
                returnDistinct = returnDistinct,
                limit = limit,
                startBits = startBits
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

        public CypherQueryBuilder SetReturn(string identity, bool distinct)
        {
            var newBuilder = Clone();
            newBuilder.returnText = identity;
            newBuilder.returnDistinct = distinct;
            return newBuilder;
        }

        public CypherQueryBuilder SetReturn<TResult>(Expression<Func<ICypherResultItem, TResult>> expression, bool distinct)
            where TResult : new()
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

        public ICypherQuery ToQuery()
        {
            var queryTextBuilder = new StringBuilder();
            var queryParameters = new Dictionary<string, object>();

            WriteStartClause(queryTextBuilder, queryParameters);
            WriteMatchClause(queryTextBuilder);
            WriteReturnClause(queryTextBuilder);
            WriteLimitClause(queryTextBuilder, queryParameters);

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

                return string.Format("{0}={1}({2})", bit.Identity, bit.LookupType, lookupContent);
            });

            target.Append(string.Join(", ", formattedStartBits));
        }

        void WriteMatchClause(StringBuilder target)
        {
            if (matchText == null) return;
            target.AppendFormat("\r\nMATCH {0}", matchText);
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
    }
}
