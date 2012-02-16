using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        internal ICypherFluentQueryWhere Where(LambdaExpression expression)
        {
            var newBuilder = Builder.SetWhere(expression);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryWhere Where(string text)
        {
            var newBuilder = Builder.SetWhere(text);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryWhere Where<T1>(Expression<Func<T1, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }

        public ICypherFluentQueryWhere Where<T1, T2>(Expression<Func<T1, T2, bool>> expression)
        {
            return Where((LambdaExpression)expression);
        }
    }
}