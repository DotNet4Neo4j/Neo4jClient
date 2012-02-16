using System.Collections.Generic;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public class CypherWhereExpressionBuilder
    {
        public static string BuildText(LambdaExpression expression, IDictionary<string, object> paramsDictionary)
        {
            var myVisitor = new CypherWhereExpressionVisitor(paramsDictionary);
            myVisitor.Visit(expression);
            return myVisitor.TextOutput.ToString();
        }
    }
}
