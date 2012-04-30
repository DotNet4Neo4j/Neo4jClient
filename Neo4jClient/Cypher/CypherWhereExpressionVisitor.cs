using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class CypherWhereExpressionVisitor : ExpressionVisitor
    {
        const string NotEqual = " != ";
        const string Equal = " = ";
        readonly IDictionary<string, object> paramsDictionary;
        public StringBuilder TextOutput { get; private set; }
        public CypherWhereExpressionVisitor(IDictionary<string, object> paramsDictionary)
        {
            this.paramsDictionary = paramsDictionary ?? new Dictionary<string, object>();
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
                RemoveNullQualifier(TextOutput);
                return node;
            }

            if (node.Value == null && text.EndsWith(Equal))
            {
                TextOutput.Remove(TextOutput.ToString().LastIndexOf(Equal, StringComparison.Ordinal), Equal.Length);
                RemoveNullQualifier(TextOutput);
                TextOutput.Append(" is null");
                return node;
            }

            var nextParameterName = CypherQueryBuilder.CreateParameter(paramsDictionary, node.Value);
            TextOutput.Append(nextParameterName);
            return node;
        }

        void RemoveNullQualifier(StringBuilder text)
        {
            if (text.ToString().EndsWith("?"))
                TextOutput.Remove(TextOutput.ToString().LastIndexOf("?", StringComparison.Ordinal), 1);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
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

                var nextParameterName = CypherQueryBuilder.CreateParameter(paramsDictionary, value);
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