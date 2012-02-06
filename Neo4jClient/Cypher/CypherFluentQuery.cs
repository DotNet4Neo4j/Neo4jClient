using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{Query.QueryText}")]
    public class CypherFluentQuery :
        ICypherFluentQueryPreStart,
        ICypherFluentQueryStarted,
        ICypherFluentQueryMatched
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
            Builder.AddStartBit(identity, nodeReferences);
            return this;
        }

        public ICypherFluentQueryStarted Start(string identity, params RelationshipReference[] relationshipReferences)
        {
            Builder.AddStartBit(identity, relationshipReferences);
            return this;
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences)
        {
            Builder.AddStartBit(identity, nodeReferences);
            return this;
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences)
        {
            Builder.AddStartBit(identity, relationshipReferences);
            return this;
        }

        public ICypherFluentQueryMatched Match(string matchText)
        {
            Builder.MatchText = matchText;
            return this;
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(string identity)
            where TResult : new()
        {
            Builder.SetReturn(identity, false);
            return new CypherFluentQuery<TResult>(Client, Builder);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(string identity)
            where TResult : new()
        {
            Builder.SetReturn(identity, true);
            return new CypherFluentQuery<TResult>(Client, Builder);
        }

        public ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            Builder.SetReturn(expression, false);
            return new CypherFluentQuery<TResult>(Client, Builder);
        }

        public ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            Builder.SetReturn(expression, true);
            return new CypherFluentQuery<TResult>(Client, Builder);
        }

        public ICypherQuery Query
        {
            get { return Builder.ToQuery(); }
        }
    }
}
