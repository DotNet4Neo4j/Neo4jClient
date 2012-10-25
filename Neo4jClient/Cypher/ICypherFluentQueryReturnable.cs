using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryReturnable
    {
        ICypherFluentQueryMatched Delete(string identities);

        ICypherFluentQueryMatched Set(string setText);

        ICypherFluentQueryReturned<TResult> Return<TResult>(string identity);

        ICypherFluentQueryReturned<TResult> Return<TResult>(string identity, CypherResultMode resultMode);

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(string identity);

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, TResult>> expression);

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression);

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;

        ICypherFluentQueryReturned<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression)
            ;
    }
}
