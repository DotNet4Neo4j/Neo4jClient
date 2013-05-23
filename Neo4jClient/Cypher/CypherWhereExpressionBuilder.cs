using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public class CypherWhereExpressionBuilder
    {
        public static string BuildText(LambdaExpression expression, Func<object, string> createParameterCallback)
        {
            if (expression.NodeType == ExpressionType.Lambda &&
                expression.Body.NodeType == ExpressionType.MemberAccess)
                throw new NotSupportedException("Member access expressions, like Where(f => f.Foo), are not yet supported. Use a comparison instead, like Where(f => f.Foo == true).");

            var myVisitor = new CypherWhereExpressionVisitor(createParameterCallback);
            myVisitor.Visit(expression);
            return myVisitor.TextOutput.ToString();
        }
    }
}
