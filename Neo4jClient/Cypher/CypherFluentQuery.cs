using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{Query.QueryText}")]
    public class CypherFluentQuery :
        ICypherFluentQueryPreStart,
        ICypherFluentQueryStarted,
        ICypherFluentQueryReturned,
        ICypherFluentQueryMatched
    {
        readonly CypherQueryBuilder builder;

        public CypherFluentQuery(IGraphClient client)
        {
            builder = new CypherQueryBuilder();
        }

        public ICypherFluentQueryStarted Start(string identity, params NodeReference[] nodeReferences)
        {
            builder.AddStartBit(identity, nodeReferences);
            return this;
        }

        public ICypherFluentQueryStarted Start(string identity, params RelationshipReference[] relationshipReferences)
        {
            builder.AddStartBit(identity, relationshipReferences);
            return this;
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences)
        {
            builder.AddStartBit(identity, nodeReferences);
            return this;
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences)
        {
            builder.AddStartBit(identity, relationshipReferences);
            return this;
        }

        public ICypherFluentQueryMatched Match(string matchText)
        {
            builder.MatchText = matchText;
            return this;
        }

        public ICypherFluentQueryReturned Return(params string[] identities)
        {
            builder.SetReturn(identities, false);
            return this;
        }

        public ICypherFluentQueryReturned ReturnDistinct(params string[] identities)
        {
            builder.SetReturn(identities, true);
            return this;
        }

        public ICypherFluentQueryReturned Return<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            where TResult : new()
        {
            builder.SetReturn(expression);
            return this;
        }

        public ICypherFluentQueryReturned Limit(int? limit)
        {
            builder.Limit = limit;
            return this;
        }

        public ICypherQuery Query
        {
            get { return builder.ToQuery(); }
        }
    }
}
