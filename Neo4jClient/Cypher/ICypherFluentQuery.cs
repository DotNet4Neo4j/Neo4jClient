using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQuery
    {
        CypherQuery Query { get; }
        void ExecuteWithoutResults();

        ICypherFluentQuery Start(string identity, string startText);
        ICypherFluentQuery Start(params ICypherStartBit[] startBits);
        ICypherFluentQuery Start(string identity, params NodeReference[] nodeReferences);
        ICypherFluentQuery Start(string identity, params RelationshipReference[] relationshipReferences);
        ICypherFluentQuery StartWithNodeIndexLookup(string identity, string indexName, string key, object value);
        ICypherFluentQuery StartWithNodeIndexLookup(string identity, string indexName, string parameterText);
        [Obsolete("Call Start with multiple components instead", true)]
        ICypherFluentQuery AddStartPoint(string identity, string startText);
        [Obsolete("Call Start with multiple components instead", true)]
        ICypherFluentQuery AddStartPoint(string identity, params NodeReference[] nodeReferences);
        [Obsolete("Call Start with multiple components instead", true)]
        ICypherFluentQuery AddStartPoint(string identity, params RelationshipReference[] relationshipReferences);
        [Obsolete("Call Start with multiple components instead", true)]
        ICypherFluentQuery AddStartPointWithNodeIndexLookup(string identity, string indexName, string key, object value);
        ICypherFluentQuery Match(params string[] matchText);
        ICypherFluentQuery Relate(string relateText);
        ICypherFluentQuery CreateUnique(string createUniqueText);
        ICypherFluentQuery Create(string createText, params object[] objects);
        ICypherFluentQuery Delete(string identities);
        ICypherFluentQuery With(string withText);
        ICypherFluentQuery Set(string setText);
        [Obsolete("Call AndWhere instead", true)]
        ICypherFluentQuery And();
        [Obsolete("Call OrWhere instead", true)]
        ICypherFluentQuery Or();
        ICypherFluentQuery Where(string text);
        ICypherFluentQuery Where<T1>(Expression<Func<T1, bool>> expression);
        ICypherFluentQuery Where<T1, T2>(Expression<Func<T1, T2, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression);
        ICypherFluentQuery Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression);
        ICypherFluentQuery OrWhere(string text);
        ICypherFluentQuery OrWhere<T1>(Expression<Func<T1, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2>(Expression<Func<T1, T2, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression);
        ICypherFluentQuery OrWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression);
        ICypherFluentQuery AndWhere(string text);
        ICypherFluentQuery AndWhere<T1>(Expression<Func<T1, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2>(Expression<Func<T1, T2, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression);
        ICypherFluentQuery AndWhere<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(string identity);
        ICypherFluentQuery<TResult> Return<TResult>(string identity, CypherResultMode resultMode);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(string identity);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
    }
}
