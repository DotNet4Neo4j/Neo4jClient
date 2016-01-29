using System.Linq.Expressions;
using Microsoft.VisualBasic.CompilerServices;

namespace Neo4jClient.Cypher
{
    internal class VbCompareReplacer : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (!(node is BinaryExpression))
            {
                return base.Visit(node);
            }

            var binaryExpression = (BinaryExpression) node;

            if (!(binaryExpression.Left is MethodCallExpression))
            {
                return base.Visit(node);
            }

            dynamic method = (MethodCallExpression) binaryExpression.Left;

            if (!(method.Method.DeclaringType == typeof (Operators) && method.Method.Name == "CompareString"))
            {
                return base.Visit(node);
            }

            dynamic left = method.Arguments[0];
            dynamic right = method.Arguments[1];

            return binaryExpression.NodeType == ExpressionType.Equal ? Expression.Equal(left, right) : Expression.NotEqual(left, right);
        }
    }
}