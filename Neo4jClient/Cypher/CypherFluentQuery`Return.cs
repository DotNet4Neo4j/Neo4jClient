using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        public ICypherFluentQuery<TResult> Return<TResult>(string identity)
        {
            return Mutate<TResult>(w => w.AppendClause("RETURN " + identity));
        }

        public ICypherFluentQuery<TResult> Return<TResult>(string statement, CypherResultMode resultMode)
        {
            return Mutate<TResult>(w =>
            {
                w.ResultMode = resultMode;
                w.AppendClause("RETURN " + statement);
            });
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(string identity)
        {
            return Mutate<TResult>(w => w.AppendClause("RETURN distinct " + identity));
        }

        ICypherFluentQuery<TResult> Return<TResult>(LambdaExpression expression)
        {
            var statement = CypherReturnExpressionBuilder.BuildText(expression);

            return Mutate<TResult>(w =>
            {
                w.ResultMode = CypherResultMode.Projection;
                w.AppendClause("RETURN " + statement);
            });
        }

        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(LambdaExpression expression)
        {
            var statement = CypherReturnExpressionBuilder.BuildText(expression);

            return Mutate<TResult>(w =>
            {
                w.ResultMode = CypherResultMode.Projection;
                w.AppendClause("RETURN distinct " + statement);
            });
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }
    }
}
