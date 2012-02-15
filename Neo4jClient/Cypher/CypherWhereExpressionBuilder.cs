using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public enum ExpressionSide {Left, Right}

    public class CypherWhereExpressionBuilder
    {
        public static string BuildText(LambdaExpression expression)
        {
            var binaryExpression = (BinaryExpression)expression.Body;
            var expressionText = binaryExpression.ToString();

            expressionText = expressionText.Replace(ExpressionType.AndAlso.ToString(), "AND");
            expressionText = expressionText.Replace(ExpressionType.OrElse.ToString(), "OR");
            expressionText = expressionText.Replace(ExpressionType.Not.ToString(), "!");
            expressionText = expressionText.Replace(ExpressionType.NotEqual.ToString(), "!=");
            expressionText = expressionText.Replace("==", "=");

            return expressionText;
        }
    }
}
