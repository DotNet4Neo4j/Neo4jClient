using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        internal ICypherFluentQuery WhereIf(bool condition,LambdaExpression expression)
        {
            if (condition)
                return Mutate(w =>
                w.AppendClause("WHERE " + CypherWhereExpressionBuilder.BuildText(expression, w.CreateParameter, Client.CypherCapabilities, CamelCaseProperties)));
            return Mutate(w => w.Clone());
        }

        internal ICypherFluentQuery AndWhereIf(bool condition, LambdaExpression expression)
        {
            if (condition)
                return Mutate(w =>
                w.AppendClause(string.Format("AND {0}", CypherWhereExpressionBuilder.BuildText(expression, w.CreateParameter, Client.CypherCapabilities, CamelCaseProperties))));
            return Mutate(w => w.Clone());
        }

        internal ICypherFluentQuery OrWhereIf(bool condition,LambdaExpression expression)
        {
            if (condition)
                return Mutate(w =>
                w.AppendClause(string.Format("OR {0}", CypherWhereExpressionBuilder.BuildText(expression, w.CreateParameter, Client.CypherCapabilities, CamelCaseProperties))));
            return Mutate(w => w.Clone());
        }

        public ICypherFluentQuery WhereIf(bool condition, string text)
        {
            if (condition)
                return Mutate(w =>
                w.AppendClause(string.Format("WHERE {0}", text)));
            return Mutate(w => w.Clone());
        }

        public ICypherFluentQuery WhereIf<T1>(bool condition, Expression<Func<T1, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2>(bool condition, Expression<Func<T1, T2, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3>(bool condition, Expression<Func<T1, T2, T3, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4>(bool condition,Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5>(bool condition,Expression<Func<T1, T2, T3, T4, T5, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression)
        {
            return WhereIf(condition, (LambdaExpression)expression);
        }
       
        public ICypherFluentQuery AndWhereIf(bool condition, string text)
        {
            if (condition)
                return Mutate(w =>
                w.AppendClause(string.Format("AND {0}", text)));
            return Mutate(w => w.Clone());
        }

        public ICypherFluentQuery AndWhereIf<T1>(bool condition,Expression<Func<T1, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2>(bool condition,Expression<Func<T1, T2, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3>(bool condition,Expression<Func<T1, T2, T3, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4>(bool condition,Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5>(bool condition,Expression<Func<T1, T2, T3, T4, T5, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            if (condition)
                return AndWhereIf(condition, (LambdaExpression)expression);
            return Mutate(w => w.Clone());
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression)
        {
            return AndWhereIf(condition, (LambdaExpression)expression);
        }
       
        public ICypherFluentQuery OrWhereIf(bool condition, string text)
        {
            if (condition)
                return Mutate(w =>
                w.AppendClause(string.Format("OR {0}", text)));
            return Mutate(w => w.Clone());
        }

        public ICypherFluentQuery OrWhereIf<T1>(bool condition,Expression<Func<T1, bool>> expression) 
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2>(bool condition,Expression<Func<T1, T2, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3>(bool condition,Expression<Func<T1, T2, T3, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4>(bool condition,Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5>(bool condition,Expression<Func<T1, T2, T3, T4, T5, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(bool condition,Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression)
        {
            return OrWhereIf(condition, (LambdaExpression)expression);
        }
    }
}
