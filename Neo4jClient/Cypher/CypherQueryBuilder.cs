using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class CypherQueryBuilder
    {
        readonly QueryWriter queryWriter;
        readonly StringBuilder queryTextBuilder;
        readonly IDictionary<string, object> queryParameters;

        string matchText;
        string relateText;
        string createUniqueText;
        string whereText;
        IList<object> createBits = new List<object>();
        string deleteText;
        string returnText;
        bool returnDistinct;
        CypherResultMode resultMode;
        int? limit;
        int? skip;
        string orderBy;
        string setText;

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
                createBits = createBits,
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
                orderBy = orderBy,
                setText = setText
            };
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
            newBuilder.createBits.Add(new CypherCreateTextBit(text));
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
            newBuilder.whereText += whereText = CypherWhereExpressionBuilder.BuildText(expression, newBuilder.queryParameters);
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

        public CypherQueryBuilder SetSetText(string text)
        {
            var newBuilder = Clone();
            newBuilder.setText = text;
            return newBuilder;
        }

        public CypherQuery ToQuery()
        {
            var textBuilder = new StringBuilder(queryTextBuilder.ToString());
            var parameters = new Dictionary<string, object>(queryParameters);
            var writer = new QueryWriter(textBuilder, parameters);

            WriteMatchClause(textBuilder);
            WriteRelateClause(writer);
            WriteCreateUniqueClause(textBuilder);
            WriteCreateClause(textBuilder);
            WriteWhereClause(textBuilder);
            WriteDeleteClause(textBuilder);
            WriteSetClause(textBuilder);
            WriteReturnClause(textBuilder);
            WriteOrderByClause(textBuilder);
            WriteSkipClause(textBuilder, parameters);
            WriteLimitClause(textBuilder, parameters);

            return writer.ToCypherQuery(resultMode);
        }

        public static string CreateParameter(IDictionary<string, object> parameters, object paramValue)
        {
            var paramName = string.Format("p{0}", parameters.Count);
            parameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
        }

        void WriteMatchClause(StringBuilder target)
        {
            if (matchText == null) return;
            target.AppendFormat("MATCH {0}", matchText);
            target.AppendLine();
        }

        void WriteDeleteClause(StringBuilder target)
        {
            if (deleteText == null) return;
            target.AppendFormat("DELETE {0}", deleteText);
            target.AppendLine();
        }

        void WriteRelateClause(QueryWriter writer)
        {
            if (relateText == null) return;
            writer.AppendClause("RELATE " + relateText);
        }

        void WriteCreateUniqueClause(StringBuilder target)
        {
            if (createUniqueText == null) return;
            target.AppendFormat("CREATE UNIQUE {0}", createUniqueText);
            target.AppendLine();
        }

        void WriteCreateClause(StringBuilder target)
        {
            if (!createBits.Any()) return;
            target.Append("CREATE ");
            var formattedCreateBits = createBits.Select(bit =>
            {
                var createTextbit = bit as CypherCreateTextBit;
                if (createTextbit != null)
                {
                    return createTextbit.CreateText;
                }

                throw new NotSupportedException(string.Format("Create bit of type {0} is not supported.", bit.GetType().FullName));
            });

            target.Append(string.Join("", formattedCreateBits));
            target.AppendLine();
        }

        void WriteWhereClause(StringBuilder target)
        {
            if (whereText == null) return;
            target.Append("WHERE " + whereText);
            target.AppendLine();
        }

        void WriteReturnClause(StringBuilder target)
        {
            if (returnText == null) return;
            target.Append("RETURN ");
            if (returnDistinct) target.Append("distinct ");
            target.Append(returnText);
            target.AppendLine();
        }

        void WriteLimitClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            if (limit == null) return;
            target.AppendFormat("LIMIT {0}", CreateParameter(paramsDictionary, limit));
            target.AppendLine();
        }

        void WriteSkipClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            if (skip == null) return;
            target.AppendFormat("SKIP {0}", CreateParameter(paramsDictionary, skip));
            target.AppendLine();
        }

        void WriteOrderByClause(StringBuilder target )
        {
            if (string.IsNullOrEmpty(orderBy)) return;
            target.AppendFormat("ORDER BY {0}", orderBy);
            target.AppendLine();
        }

        void WriteSetClause(StringBuilder target)
        {
            if (setText == null) return;
            target.AppendFormat("SET {0}", setText);
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
