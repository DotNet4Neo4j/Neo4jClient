using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryReturnable
    {
        ICypherFluentQueryReturned<TResult> Return<TResult>(string identity)
            where TResult : new();

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(string identity)
            where TResult : new();

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            where TResult : new();

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            where TResult : new();
    }
}