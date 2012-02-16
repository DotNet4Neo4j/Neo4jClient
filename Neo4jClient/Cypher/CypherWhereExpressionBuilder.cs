using System.Diagnostics;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public class CypherWhereExpressionBuilder
    {
        public static string BuildText(LambdaExpression expression)
        {
            var myVisitor = new CypherWhereExpressionVisitor();
            myVisitor.Visit(expression);
            return myVisitor.TextOutput.ToString();
        }
    }
}
