using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public class CypherFluentQuery :
        ICypherFluentQueryPreStart,
        ICypherFluentQueryStarted,
        ICypherFluentQueryMatched,
        IAttachedReference
    {
        protected readonly IGraphClient Client;
        protected readonly CypherQueryBuilder Builder;

        public CypherFluentQuery(IGraphClient client)
            : this(client, new CypherQueryBuilder())
        {
        }

        protected CypherFluentQuery(IGraphClient client, CypherQueryBuilder builder)
        {
            Client = client;
            Builder = builder;
        }

        public ICypherFluentQueryStarted Start(string identity, params NodeReference[] nodeReferences)
        {
            var newBuilder = Builder.AddStartBit(identity, nodeReferences);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryStarted Start(string identity, params RelationshipReference[] relationshipReferences)
        {
            var newBuilder = Builder.AddStartBit(identity, relationshipReferences);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences)
        {
            var newBuilder = Builder.AddStartBit(identity, nodeReferences);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences)
        {
            var newBuilder = Builder.AddStartBit(identity, relationshipReferences);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryMatched Match(string matchText)
        {
            var newBuilder = Builder.SetMatchText(matchText);
            return new CypherFluentQuery(Client, newBuilder);
        }

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

        public ICypherQuery Query
        {
            get { return Builder.ToQuery(); }
        }

        IGraphClient IAttachedReference.Client
        {
            get { return Client; }
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
