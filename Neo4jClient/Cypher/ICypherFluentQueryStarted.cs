using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryStarted : ICypherFluentQuery, ICypherFluentQueryReturnable
    {
        ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences);
        ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences);
        ICypherFluentQueryMatched Match(string matchText);

        ICypherFluentQueryWhere Where<T1>(Expression<Func<T1, bool>> whereClause);
    }
}
