using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Neo4jClient.Cypher
{
    internal class CypherWhereExpressionVisitor : ExpressionVisitor
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

            var valueWrappedInParameter = createParameterCallback(node.Value);
            TextOutput.Append(valueWrappedInParameter);

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
            if (node.NodeType != ExpressionType.MemberAccess)
                throw new InvalidOperationException(string.Format(
                    "Node was a MemberExpression but NodeType was {0} instead of MemberAccess (expected). Full node was: {1}",
                    node.NodeType,
                    node));

            var isStaticMember = node.Expression == null;
            if (isStaticMember)
            {
                VisitStaticMember(node);
                return node;
            }

            var isParameterExpression = node.Expression.NodeType == ExpressionType.Parameter;
            var isParameterExpressionWrappedInConvert =
                node.Expression.NodeType == ExpressionType.Convert &&
                ((UnaryExpression)node.Expression).Operand.NodeType == ExpressionType.Parameter;

            if (isParameterExpression ||
                isParameterExpressionWrappedInConvert)
            {
                VisitParameterMember(node);
                return node;
            }

            var isConstantExpression = node.Expression.NodeType == ExpressionType.Constant;
            var isConstantExpressionWrappedInMemberAccess =
                node.Expression.NodeType == ExpressionType.MemberAccess &&
                ((MemberExpression) node.Expression).Expression.NodeType == ExpressionType.Constant;

            if (isConstantExpression ||
                isConstantExpressionWrappedInMemberAccess)
            {
                VisitConstantMember(node);
                return node;
            }

            throw new NotSupportedException(string.Format("Unhandled node type {0} in MemberExpression: {1}", node.NodeType, node));
        }

        void VisitStaticMember(MemberExpression node)
        {
            object value;
            switch (node.Member.MemberType)
            {
                case MemberTypes.Field:
                    value = ((FieldInfo) node.Member).GetValue(null);
                    break;
                case MemberTypes.Property:
                    value = ((PropertyInfo) node.Member).GetValue(null, null);
                    break;
                default:
                    throw new NotSupportedException(string.Format(
                        "Unhandled member type {0} in static member expression: {1}",
                        node.Member.MemberType,
                        node));
            }

            var valueWrappedInParameter = createParameterCallback(value);
            TextOutput.Append(valueWrappedInParameter);
        }

        void VisitParameterMember(MemberExpression node)
        {
            var identityExpression = node.Expression as ParameterExpression;
            if (identityExpression == null &&
                node.Expression.NodeType == ExpressionType.Convert)
                identityExpression = ((UnaryExpression) node.Expression).Operand as ParameterExpression;
            if (identityExpression == null)
                throw new InvalidOperationException("Failed to extract identity name from expression " + node);
            var identity = identityExpression.Name;

            var nullIdentifier = string.Empty;

            var propertyParent = node.Member.ReflectedType;
            var propertyType = propertyParent.GetProperty(node.Member.Name).PropertyType;

            if (
                (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof (Nullable<>))
                ||
                (propertyType == typeof (string))
                )
                nullIdentifier = "?";

            TextOutput.Append(string.Format("{0}.{1}{2}", identity, node.Member.Name, nullIdentifier));
        }

        void VisitConstantMember(MemberExpression node)
        {
            var data = node.Expression.NodeType == ExpressionType.Constant
                ? ParseValueFromExpression(node)
                : ParseValueFromExpression(node.Expression);

            var value = node.Expression.NodeType == ExpressionType.Constant
                ? data
                : data.GetType().GetProperty(node.Member.Name).GetValue(data, BindingFlags.Public, null, null, null);

            var valueWrappedInParameter = createParameterCallback(value);
            TextOutput.Append(valueWrappedInParameter);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert)
                return base.VisitUnary(node);

            throw new NotSupportedException("Unary expressions, like Where(f => !f.Foo), are not supported because these become ambiguous between C# and Cypher based on how Neo4j handles null values. Use a comparison instead, like Where(f => f.Foo == false).");
        }

        static object ParseValueFromExpression(Expression expression)
        {
            var lambdaExpression = Expression.Lambda(expression);
            return lambdaExpression.Compile().DynamicInvoke();
        }
    }
}