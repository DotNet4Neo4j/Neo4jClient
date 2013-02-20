using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        internal ICypherFluentQuery Where(LambdaExpression expression)
        {
            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause(string.Format("WHERE {0}", CypherWhereExpressionBuilder.BuildText(expression, w.CreateParameter))));
            return new CypherFluentQuery(Client, newBuilder);
        }

        [Obsolete("Call AndWhere instead", true)]
        ICypherFluentQuery ICypherFluentQuery.And()
        {
            throw new NotSupportedException();
        }

        internal ICypherFluentQuery AndWhere(LambdaExpression expression)
        {
            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause(string.Format("AND {0}", CypherWhereExpressionBuilder.BuildText(expression, w.CreateParameter))));
            return new CypherFluentQuery(Client, newBuilder);
        }

        [Obsolete("Call OrWhere instead", true)]
        ICypherFluentQuery ICypherFluentQuery.Or()
        {
            throw new NotSupportedException();
        }
       
        internal ICypherFluentQuery OrWhere(LambdaExpression expression)
        {
            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause(string.Format("OR {0}", CypherWhereExpressionBuilder.BuildText(expression, w.CreateParameter))));
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQuery Where(string text)
        {
            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause(string.Format("WHERE {0}", text)));
            return new CypherFluentQuery(Client, newBuilder);
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
            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause(string.Format("AND {0}", text)));
            return new CypherFluentQuery(Client, newBuilder);
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
            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause(string.Format("OR {0}", text)));
            return new CypherFluentQuery(Client, newBuilder);
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