using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        internal const string IdentityLooksLikeAFunctionExceptionMessage = "The overload you have called takes an identity (for example: foo), but it looks like you've tried to pass in a function all (for example: count(foo)). You should use .Return(foo => foo.Count()) instead. If the function you want isn't available in the managed wrapper, you can use Return(() => Return.As<Foo>(\"function(foo)\")). If you want to return multiple columns, use an anonymous type: Return((foo, bar) => new { Foo = foo.As<Foo>(), BarCount = bar.Count() }).";
        internal const string IdentityLooksLikeAMultiColumnStatementExceptionMessage = "The overload you have called takes an identity (for example: foo), but it looks like you've tried to pass in a multiple column statement (for example: foo,bar). If you want to return multiple columns, use an anonymous type: Return((foo, bar) => new { Foo = foo.As<Foo>(), BarCount = bar.Count() }). If the function you want isn't available in the managed wrapper, you can use Return(() => new { Foo = Return.As<Bar>(\"function(foo)\") }).";

        static readonly Regex LooksLikeAFunctionCallExpression = new Regex(@"^\w+\([^\)]*?\)$");

        public ICypherFluentQuery<TResult> Return<TResult>(string identity)
        {
            if (identity.Contains("(") &&
                LooksLikeAFunctionCallExpression.IsMatch(identity))
                throw new ArgumentException(IdentityLooksLikeAFunctionExceptionMessage, "identity");

            if (identity.Contains(","))
                throw new ArgumentException(IdentityLooksLikeAMultiColumnStatementExceptionMessage, "identity");

            if (identity.Any(c => !char.IsLetterOrDigit(c)))
                identity = string.Format("`{0}`", identity);

            return Mutate<TResult>(w => w.AppendClause("RETURN " + identity));
        }

        [Obsolete("This overload will be removed in future versions because the result mode should all be managed automatically. If there's a specific reason for why you are using this, raise an issue at https://bitbucket.org/Readify/neo4jclient/issues/new so we can fix it before we remove this overload. If you want to create a projection, you should be using the lambda overload instead. See the 'Using Functions in Return Clauses' and 'Using Custom Text in Return Clauses' sections of https://bitbucket.org/Readify/neo4jclient/wiki/cypher for details of how to do this.")]
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
            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression);

            return Mutate<TResult>(w =>
            {
                w.ResultMode = returnExpression.ResultMode;
                w.AppendClause("RETURN " + returnExpression.Text);
            });
        }

        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(LambdaExpression expression)
        {
            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression);

            return Mutate<TResult>(w =>
            {
                w.ResultMode = returnExpression.ResultMode;
                w.AppendClause("RETURN distinct " + returnExpression.Text);
            });
        }

        public ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<TResult>> expression)
        {
            return Return<TResult>((LambdaExpression)expression);
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
