using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public partial interface ICypherFluentQuery
    {
        ICypherFluentQuery WhereIf(bool condition, string text);
        ICypherFluentQuery WhereIf<T1>(bool condition, Expression<Func<T1, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2>(bool condition, Expression<Func<T1, T2, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3>(bool condition, Expression<Func<T1, T2, T3, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4>(bool condition, Expression<Func<T1, T2, T3, T4, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5>(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression);
        ICypherFluentQuery WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression);
        ICypherFluentQuery OrWhereIf(bool condition, string text);
        ICypherFluentQuery OrWhereIf<T1>(bool condition, Expression<Func<T1, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2>(bool condition, Expression<Func<T1, T2, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3>(bool condition, Expression<Func<T1, T2, T3, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4>(bool condition, Expression<Func<T1, T2, T3, T4, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5>(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression);
        ICypherFluentQuery OrWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression);
        ICypherFluentQuery AndWhereIf(bool condition, string text);
        ICypherFluentQuery AndWhereIf<T1>(bool condition, Expression<Func<T1, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2>(bool condition, Expression<Func<T1, T2, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3>(bool condition, Expression<Func<T1, T2, T3, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4>(bool condition, Expression<Func<T1, T2, T3, T4, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5>(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression);
        ICypherFluentQuery AndWhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression);
    }
}
