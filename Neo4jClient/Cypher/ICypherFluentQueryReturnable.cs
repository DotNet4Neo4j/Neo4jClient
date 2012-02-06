using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryReturnable
    {
        ICypherFluentQueryReturned Return(params string[] identities);
        ICypherFluentQueryReturned ReturnDistinct(params string[] identities);
        ICypherFluentQueryReturned Return<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            where TResult : new();
        ICypherFluentQueryReturned ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            where TResult : new();
    }
}