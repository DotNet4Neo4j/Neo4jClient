using System.Linq;

namespace Neo4jClient.Gremlin
{
    public static class IfThenElseStep
    {
        public static IGremlinRelationshipQuery IfThenElse(this IGremlinQuery baseQuery, IGremlinQuery ifExpression, IGremlinQuery ifThen, IGremlinQuery ifElse)
        {
            if (ifExpression.GetType() == typeof(IdentityPipe))
                ((IdentityPipe)ifExpression).Client = ifExpression.Client;

            if (ifExpression.GetType() == typeof(GremlinIterator))
                ((GremlinIterator)ifExpression).Client = ifExpression.Client;

            if (ifThen.GetType() == typeof(IdentityPipe))
                ((IdentityPipe)ifThen).Client = ifThen.Client;

            if (ifThen.GetType() == typeof(GremlinIterator))
                ((GremlinIterator)ifThen).Client = ifThen.Client;

            if (ifElse.GetType() == typeof(IdentityPipe))
                ((IdentityPipe)ifElse).Client = ifElse.Client;

            if (ifElse.GetType() == typeof(GremlinIterator))
                ((GremlinIterator)ifElse).Client = ifElse.Client;

            baseQuery = baseQuery.AddIfThenElseBlock(".ifThenElse{{0}}{{1}}{{2}}", ifExpression, ifThen, ifElse);
            return new GremlinRelationshipEnumerable(baseQuery);
        }
    }
}