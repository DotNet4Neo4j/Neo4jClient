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
            var newWriter = queryWriter.Clone();
            newWriter.ResultMode = resultMode;
            newWriter.AppendClause("RETURN " + statement);
            return new CypherFluentQuery<TResult>(Client, newWriter);
        }

        public ICypherFluentQuery<TResult> ReturnDistinct<TResult>(string identity)
        {
            return Mutate<TResult>(w => w.AppendClause("RETURN distinct " + identity));
        }

        ICypherFluentQuery<TResult> Return<TResult>(LambdaExpression expression)
        {
            var statement = CypherReturnExpressionBuilder.BuildText(expression);

            var newWriter = queryWriter.Clone();
            newWriter.ResultMode = CypherResultMode.Projection;
            newWriter.AppendClause("RETURN " + statement);

            return new CypherFluentQuery<TResult>(Client, newWriter);
        }

        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(LambdaExpression expression)
        {
            var statement = CypherReturnExpressionBuilder.BuildText(expression);

            var newWriter = queryWriter.Clone();
            newWriter.ResultMode = CypherResultMode.Projection;
            newWriter.AppendClause("RETURN distinct " + statement);

            return new CypherFluentQuery<TResult>(Client, newWriter);
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
