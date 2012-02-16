using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryWhere : ICypherFluentQuery, ICypherFluentQueryReturnable
    {
        ICypherFluentQueryWhere Where(string whereClause);
        ICypherFluentQueryWhere Where<T1>(Expression<Func<T1, bool>> whereClause);
    }
}