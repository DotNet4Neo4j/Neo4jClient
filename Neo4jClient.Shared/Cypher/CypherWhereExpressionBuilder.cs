using System;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public class CypherWhereExpressionBuilder
    {
        public static string BuildText(
            LambdaExpression expression,
            Func<object, string> createParameterCallback,
            CypherCapabilities capabilities = null, 
            bool camelCaseProperties = false)
        {
            capabilities = capabilities ?? CypherCapabilities.Default;

            if (expression.NodeType == ExpressionType.Lambda &&
                expression.Body.NodeType == ExpressionType.MemberAccess)
                throw new NotSupportedException("Member access expressions, like Where(f => f.Foo), are not supported because these become ambiguous between C# and Cypher based on how Neo4j handles null values. Use a comparison instead, like Where(f => f.Foo == true).");

            var myVisitor = new CypherWhereExpressionVisitor(createParameterCallback, capabilities, camelCaseProperties);
            myVisitor.Visit(new VbCompareReplacer().Visit(expression));
            return myVisitor.TextOutput.ToString();
        }
    }
}
