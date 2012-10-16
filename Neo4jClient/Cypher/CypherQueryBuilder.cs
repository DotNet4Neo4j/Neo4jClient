using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class CypherQueryBuilder
    {
        IDictionary<string, object> queryParameters = new Dictionary<string, object>();
        IList<object> startBits = new List<object>();
        string matchText;
        string relateText;
        string createUniqueText;
        string whereText;
        string createText;
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
                createText = createText,
                deleteText = deleteText,
                matchText = matchText,
                relateText = relateText,
                createUniqueText = createUniqueText,
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

        public CypherQueryBuilder AddStartBitWithNodeIndexLookup(string identity, string indexName, string parameterText)
        {
            var newBuilder = Clone();
            newBuilder.startBits.Add(new CypherStartBitWithNodeIndexLookupWithSingleParameter(identity, indexName, parameterText));
            return newBuilder;
        }

        public CypherQueryBuilder AddStartBitWithNodeIndexLookup(string identity, string indexName, string key, object value)
        {
            var newBuilder = Clone();
            newBuilder.startBits.Add(new CypherStartBitWithNodeIndexLookup(identity, indexName, key, value));
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

        public CypherQueryBuilder SetCreateUniqueText(string text)
        {
            var newBuilder = Clone();
            newBuilder.createUniqueText = text;
            return newBuilder;
        }

        public CypherQueryBuilder SetCreateText(string text)
        {
            var newBuilder = Clone();
            newBuilder.createText = text;
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
            WriteCreateUniqueClause(queryTextBuilder);
            WriteCreateClause(queryTextBuilder);
            WriteWhereClause(queryTextBuilder);
            WriteDeleteClause(queryTextBuilder);
            WriteReturnClause(queryTextBuilder);
            WriteOrderByClause(queryTextBuilder);
            WriteSkipClause(queryTextBuilder, queryParameters);
            WriteLimitClause(queryTextBuilder, queryParameters);
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
                var standardStartBit = bit as CypherStartBit;
                if (standardStartBit != null)
                {
                    var lookupIdParameterNames = standardStartBit
                        .LookupIds
                        .Select(i => CreateParameter(paramsDictionary, i))
                        .ToArray();

                    var lookupContent = string.Join(", ", lookupIdParameterNames);
                    return string.Format("{0}={1}({2})",
                        standardStartBit.Identifier,
                        standardStartBit.LookupType,
                        lookupContent);
                }

                var startBithWithNodeIndexLookup = bit as CypherStartBitWithNodeIndexLookup;
                if (startBithWithNodeIndexLookup != null)
                {
                    var valueParameter = CreateParameter(paramsDictionary, startBithWithNodeIndexLookup.Value);
                    return string.Format("{0}=node:{1}({2} = {3})",
                        startBithWithNodeIndexLookup.Identifier,
                        startBithWithNodeIndexLookup.IndexName,
                        startBithWithNodeIndexLookup.Key,
                        valueParameter);
                }

                var startBithWithNodeIndexLookupSingleParameter = bit as CypherStartBitWithNodeIndexLookupWithSingleParameter;
                if (startBithWithNodeIndexLookupSingleParameter != null) {
                   var valueParameter = CreateParameter(paramsDictionary, startBithWithNodeIndexLookupSingleParameter.Parameter);
                   return string.Format("{0}=node:{1}({2})",
                       startBithWithNodeIndexLookupSingleParameter.Identifier,
                       startBithWithNodeIndexLookupSingleParameter.IndexName,
                       valueParameter);
                }

                throw new NotSupportedException(string.Format("Start bit of type {0} is not supported.", bit.GetType().FullName));
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

        void WriteCreateUniqueClause(StringBuilder target)
        {
            if (createUniqueText == null) return;
            target.AppendFormat("\r\nCREATE UNIQUE {0}", createUniqueText);
        }

        void WriteCreateClause(StringBuilder target)
        {
            if (createText == null) return;
            target.AppendFormat("\r\nCREATE {0}", createText);
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
