using System;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        public ICypherFluentQuery With(string withText)
        {
            return Mutate(w => w.AppendClause(string.Format("WITH {0}", withText)));
        }

        ICypherFluentQuery<TResult> With<TResult>(LambdaExpression expression)
        {
            var expressionBuilder = new CypherWithExpressionBuilder(Client.CypherCapabilities, CamelCaseProperties);
            var withExpression = expressionBuilder.BuildText(expression);

            return Mutate<TResult>(w =>
            {
                w.ResultMode = withExpression.ResultMode;
                w.AppendClause("WITH " + withExpression.Text);
            });
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery With<TResult>(Expression<Func<TResult>> expression)
        {
            return With<TResult>((LambdaExpression)expression);
        }
    }
}
