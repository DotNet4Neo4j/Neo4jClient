using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        public ICypherFluentQueryReturned<TResult> Return<TResult>(string identity)
    where TResult : new()
        {
            var newBuilder = Builder.SetReturn(identity, false);
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(string identity)
            where TResult : new()
        {
            var newBuilder = Builder.SetReturn(identity, true);
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        ICypherFluentQueryReturned<TResult> Return<TResult>(LambdaExpression expression)
            where TResult : new()
        {
            var newBuilder = Builder.SetReturn(expression, false);
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(LambdaExpression expression)
            where TResult : new()
        {
            var newBuilder = Builder.SetReturn(expression, true);
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
    where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return Return<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            return ReturnDistinct<TResult>((LambdaExpression)expression);
        }
    }
}