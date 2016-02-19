using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        internal ICypherFluentQuery Where(LambdaExpression expression)
        {
            return Mutate(w =>
                w.AppendClause("WHERE " + CypherWhereExpressionBuilder.BuildText(expression, w.CreateParameter, Client.CypherCapabilities, CamelCaseProperties)));
        }

        internal ICypherFluentQuery AndWhere(LambdaExpression expression)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("AND {0}", CypherWhereExpressionBuilder.BuildText(expression, w.CreateParameter, Client.CypherCapabilities, CamelCaseProperties))));
        }

        internal ICypherFluentQuery OrWhere(LambdaExpression expression)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("OR {0}", CypherWhereExpressionBuilder.BuildText(expression, w.CreateParameter, Client.CypherCapabilities, CamelCaseProperties))));
        }

        public ICypherFluentQuery Where(string text)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("WHERE {0}", text)));
        }

        public ICypherFluentQuery Where<T1>(Expression<Func<T1, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2>(Expression<Func<T1, T2, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }
       
        public ICypherFluentQuery AndWhere(string text)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("AND {0}", text)));
        }

        public ICypherFluentQuery AndWhere<T1>(Expression<Func<T1, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2>(Expression<Func<T1, T2, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression)
        {
            return AndWhere((LambdaExpression)expression);
        }
       
        public ICypherFluentQuery OrWhere(string text)
        {
            return Mutate(w =>
                w.AppendClause(string.Format("OR {0}", text)));
        }

        public ICypherFluentQuery OrWhere<T1>(Expression<Func<T1, bool>> expression) 
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2>(Expression<Func<T1, T2, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }

        public ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression)
        {
           return OrWhere((LambdaExpression)expression);
        }
    }
}
