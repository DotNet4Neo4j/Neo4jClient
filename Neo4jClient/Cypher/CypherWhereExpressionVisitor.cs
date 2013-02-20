using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class CypherWhereExpressionVisitor : ExpressionVisitor
    {
        const string NotEqual = " <> ";
        const string Equal = " = ";
        readonly Func<object, string> createParameterCallback;
        public StringBuilder TextOutput { get; private set; }

        public CypherWhereExpressionVisitor(Func<object, string> createParameterCallback)
        {
            this.createParameterCallback = createParameterCallback;
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
                    TextOutput.Append(Equal);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    TextOutput.Append(" >= ");
                    break;
                case ExpressionType.GreaterThan:
                    TextOutput.Append(" > ");
                    break;
                case ExpressionType.NotEqual:
                    TextOutput.Append(NotEqual);
                    break;

                default:
                    throw new NotSupportedException(string.Format("Expression type {0} is not supported.", node.NodeType));
            }
 
            Visit(node.Right);
            TextOutput.Append(")");
 
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var text = TextOutput.ToString();
            if (node.Value == null && text.EndsWith(NotEqual))
            {
                TextOutput.Remove(TextOutput.ToString().LastIndexOf(NotEqual, StringComparison.Ordinal), NotEqual.Length);
                TextOutput.Append(" is not null");
                return node;
            }

            if (node.Value == null && text.EndsWith(Equal))
            {
                TextOutput.Remove(TextOutput.ToString().LastIndexOf(Equal, StringComparison.Ordinal), Equal.Length);
                TextOutput.Append(" is null");
                return node;
            }

            if (node.Value != null && text.EndsWith(Equal))
            {
                TextOutput.Remove(TextOutput.ToString().LastIndexOf(Equal, StringComparison.Ordinal), Equal.Length);
                SwapNullQualifierFromDefaultTrueToDefaultFalse(TextOutput);
                TextOutput.Append(Equal);
            }

            var nextParameterName = createParameterCallback(node.Value);
            TextOutput.Append(nextParameterName);
            return node;
        }

        void SwapNullQualifierFromDefaultTrueToDefaultFalse(StringBuilder text)
        {
            if (!text.ToString().EndsWith("?"))
                return;
            TextOutput.Remove(TextOutput.Length - 1, 1);
            TextOutput.Append("!");
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.MemberAccess &&
                node.Expression == null)
                // It's a static member
            {
                object value;
                switch (node.Member.MemberType)
                {
                    case MemberTypes.Field:
                        value = ((FieldInfo)node.Member).GetValue(null);
                        break;
                    case MemberTypes.Property:
                        value = ((PropertyInfo)node.Member).GetValue(null, null);
                        break;
                    default:
                        throw new NotImplementedException(string.Format("We haven't implemented support for reading static {0} yet", node.Member.MemberType));
                }

                var nextParameterName = createParameterCallback(value);
                TextOutput.Append(string.Format("{0}", nextParameterName));

                return node;
            }

            if (node.NodeType == ExpressionType.MemberAccess && node.Expression.NodeType == ExpressionType.Parameter)
            {
                var parameter = ((ParameterExpression)node.Expression);

                var nullIdentifier = string.Empty;

                var propertyParent = node.Member.ReflectedType;
                var propertyType = propertyParent.GetProperty(node.Member.Name).PropertyType;

                if (
                    (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) 
                    ||
                    (propertyType == typeof(string))
                    )
                    nullIdentifier = "?";

                TextOutput.Append(string.Format("{0}.{1}{2}", parameter.Name, node.Member.Name, nullIdentifier));
            }
            else if (
                (node.NodeType == ExpressionType.MemberAccess && node.Expression.NodeType == ExpressionType.Constant)
                || 
                (node.NodeType == ExpressionType.MemberAccess && node.Expression.NodeType == ExpressionType.MemberAccess
                && ((MemberExpression)node.Expression).Expression.NodeType == ExpressionType.Constant))
            {

                var data = node.Expression.NodeType == ExpressionType.Constant ? ParseValueFromExpression(node) :
                ParseValueFromExpression(node.Expression);

                var value = node.Expression.NodeType == ExpressionType.Constant ? data : data.GetType().GetProperty(node.Member.Name).GetValue(data, BindingFlags.Public, null, null, null);

                var nextParameterName = createParameterCallback(value);
                TextOutput.Append(string.Format("{0}", nextParameterName));
            }
            else
            {
                TextOutput.Append(string.Format("{0}", node.Member.Name));
            }

            return node;
        }

        static object ParseValueFromExpression(Expression expression)
        {
            var lambdaExpression = Expression.Lambda(expression);
            return lambdaExpression.Compile().DynamicInvoke();
        }
    }
}