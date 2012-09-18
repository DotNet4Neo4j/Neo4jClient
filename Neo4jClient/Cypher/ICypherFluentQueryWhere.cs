using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryWhere : ICypherFluentQuery, ICypherFluentQueryReturnable
    {
        ICypherFluentQueryMatched CreateUnique(string createUniqueText);
        ICypherFluentQueryWhere And();
        ICypherFluentQueryWhere Or();
        ICypherFluentQueryWhere Where(string text);
        ICypherFluentQueryWhere Where<T1>(Expression<Func<T1, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2>(Expression<Func<T1, T2, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12,T13, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13,T14, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression);
        ICypherFluentQueryWhere Where<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression);
    }
}