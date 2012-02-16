using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class CypherWhereExpressionVisitor :  ExpressionVisitor
    {
        public StringBuilder TextOutput { get; private set; }
        public CypherWhereExpressionVisitor()
        {
            TextOutput = new StringBuilder();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            TextOutput.Append("(");
            Visit(node.Left);
 
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    TextOutput.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    TextOutput.Append(" OR ");
                    break;
                case ExpressionType.Not:
                    TextOutput.Append(" NOT ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    TextOutput.Append(" <= ");
                    break;
                case ExpressionType.LessThan:
                    TextOutput.Append(" < ");
                    break;
                case ExpressionType.Equal:
                    TextOutput.Append(" = ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    TextOutput.Append(" >= ");
                    break;
                case ExpressionType.GreaterThan:
                    TextOutput.Append(" > ");
                    break;
                case ExpressionType.NotEqual:
                    TextOutput.Append(" != ");
                    break;
            }
 
            Visit(node.Right);
            TextOutput.Append(")");
 
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if(node.Value is string)
                TextOutput.Append(string.Format("\"{0}\"",node.Value));
            else
                TextOutput.Append(node.Value);

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.MemberAccess && node.Expression.NodeType == ExpressionType.Parameter)
            {
                var parameter = ((ParameterExpression)node.Expression);
                TextOutput.Append(string.Format("{0}.{1}", parameter.Name, node.Member.Name));
            }
            else
            {
                TextOutput.Append(string.Format("{0}", node.Member.Name));
            }

            return node;
        }
    }
}