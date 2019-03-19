using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Neo4jClient.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Cypher
{
    internal class CypherWhereExpressionVisitor : ExpressionVisitor
    {
        private const string NotEqual = " <> ";
        private const string Equal = " = ";
        private const string GreaterThan = " > ";
        private const string GreaterThanOrEqual = " >= ";
        private const string LessThan = " < ";
        private const string LessThanOrEqual = " <= ";

        private readonly Dictionary<string, Func<MethodCallExpression, Expression>> supportedMethodCalls;

        private string lastWrittenMemberName;

        private readonly Func<object, string> createParameterCallback;
        private readonly CypherCapabilities capabilities;
        private readonly bool camelCaseProperties;

        public StringBuilder TextOutput { get; private set; }

        public CypherWhereExpressionVisitor(Func<object, string> createParameterCallback, CypherCapabilities capabilities, bool camelCaseProperties)
        {
            this.createParameterCallback = createParameterCallback;
            this.capabilities = capabilities;
            this.camelCaseProperties = camelCaseProperties;
            TextOutput = new StringBuilder();

            supportedMethodCalls = new Dictionary<string, Func<MethodCallExpression, Expression>>()
            {
                { "StartsWith",  VisitStartsWithMethod },
                { "Contains",  VisitContainsMethod },
                { "EndsWith",  VisitEndsWithMethod },
                { "In", VisitInMethod },
                { "NotIn", VisitNotInMethod }
            };
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Func<MethodCallExpression, Expression> visitMethod;
            if (supportedMethodCalls.TryGetValue(node.Method.Name, out visitMethod))
            {
                return visitMethod(node);
            }

            return base.VisitMethodCall(node);
        }

        private Expression VisitInMethod(MethodCallExpression node)
        {
            TextOutput.Append("(");
            Visit(node.Arguments[0]);
            TextOutput.Append(" IN ");
            Visit(node.Arguments[1]);
            TextOutput.Append(")");

            return node;
        }

        private Expression VisitNotInMethod(MethodCallExpression node)
        {
            TextOutput.Append("NOT (");
            Visit(node.Arguments[0]);
            TextOutput.Append(" IN ");
            Visit(node.Arguments[1]);
            TextOutput.Append(")");

            return node;
        }

        private Expression VisitStartsWithMethod(MethodCallExpression node)
        {
            if (capabilities.SupportsStartsWith)
            {
                TextOutput.Append("(");
                Visit(node.Object);
                TextOutput.Append(" STARTS WITH ");
                Visit(node.Arguments[0]);
                TextOutput.Append(")");
            }
            else
            {
                throw new NotSupportedException("Neo4j doesn't support STARTS WITH in versions lower than 2.3.0. Instead you have to use a Lucene query like: WHERE n.Property =~ 'Tob.*'.");
            }


            return node;
        }

        private Expression VisitContainsMethod(MethodCallExpression node)
        {
            if (capabilities.SupportsStartsWith)
            {
                TextOutput.Append("(");
                Visit(node.Object);
                TextOutput.Append(" CONTAINS ");
                Visit(node.Arguments[0]);
                TextOutput.Append(")");
            }
            else
            {
                throw new NotSupportedException("Neo4j doesn't support CONTAINS in versions lower than 2.3.0. Instead you have to use a Lucene query like: WHERE n.Property =~ 'Tob.*'.");
            }


            return node;
        }

        private Expression VisitEndsWithMethod(MethodCallExpression node)
        {
            if (capabilities.SupportsStartsWith)
            {
                TextOutput.Append("(");
                Visit(node.Object);
                TextOutput.Append(" ENDS WITH ");
                Visit(node.Arguments[0]);
                TextOutput.Append(")");
            }
            else
            {
                throw new NotSupportedException("Neo4j doesn't support ENDS WITH in versions lower than 2.3.0. Instead you have to use a Lucene query like: WHERE n.Property =~ 'Tob.*'.");
            }


            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            TextOutput.Append("(");
            Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    TextOutput.Append(" AND ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    TextOutput.Append(" OR ");
                    break;
                case ExpressionType.Not:
                    TextOutput.Append(" NOT ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    TextOutput.Append(LessThanOrEqual);
                    break;
                case ExpressionType.LessThan:
                    TextOutput.Append(LessThan);
                    break;
                case ExpressionType.Equal:
                    TextOutput.Append(Equal);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    TextOutput.Append(GreaterThanOrEqual);
                    break;
                case ExpressionType.GreaterThan:
                    TextOutput.Append(GreaterThan);
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
                if (capabilities.SupportsNullComparisonsWithIsOperator)
                {
                    TextOutput.Append(" is not null");
                }
                else if (capabilities.SupportsHasFunction)
                {
                    TextOutput.Remove(TextOutput.ToString().LastIndexOf(lastWrittenMemberName, StringComparison.Ordinal), lastWrittenMemberName.Length);
                    TextOutput.Append(string.Format("has({0})", lastWrittenMemberName));
                }
                else
                {
                    TextOutput.Remove(TextOutput.ToString().LastIndexOf(lastWrittenMemberName, StringComparison.Ordinal), lastWrittenMemberName.Length);
                    TextOutput.Append(string.Format("exists({0})", lastWrittenMemberName));
                }
                return node;
            }

            if (node.Value == null && text.EndsWith(Equal))
            {
                TextOutput.Remove(TextOutput.ToString().LastIndexOf(Equal, StringComparison.Ordinal), Equal.Length);
                if (capabilities.SupportsNullComparisonsWithIsOperator)
                {
                    TextOutput.Append(" is null");
                }
                else if (capabilities.SupportsHasFunction)
                {
                    TextOutput.Remove(TextOutput.ToString().LastIndexOf(lastWrittenMemberName, StringComparison.Ordinal), lastWrittenMemberName.Length);
                    TextOutput.Append(string.Format("not(has({0}))", lastWrittenMemberName));
                }
                else
                {
                    TextOutput.Remove(TextOutput.ToString().LastIndexOf(lastWrittenMemberName, StringComparison.Ordinal), lastWrittenMemberName.Length);
                    TextOutput.Append(string.Format("not(exists({0}))", lastWrittenMemberName));
                }
                return node;
            }

            if (capabilities.SupportsPropertySuffixesForControllingNullComparisons && node.Value != null)
            {
                SwapNullQualifierFromDefaultTrueToDefaultFalseIfTextEndsWithAny(new[]
                    {
                        Equal,
                        GreaterThan,
                        GreaterThanOrEqual,
                        LessThan,
                        LessThanOrEqual
                    });
            }

            var valueWrappedInParameter = createParameterCallback(node.Value);
            TextOutput.Append(valueWrappedInParameter);

            return node;
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

            var isConstantExpression = IsConstantExpression(node);
            var isConstantExpressionWrappedInMemberAccess =
                node.Expression.NodeType == ExpressionType.MemberAccess &&
                ((MemberExpression)node.Expression).Expression.NodeType == ExpressionType.Constant;

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

            FieldInfo fInfo = node.Member as FieldInfo;
            PropertyInfo pInfo = node.Member as PropertyInfo;

            if (fInfo != null)
                value = fInfo.GetValue(null);
            else if (pInfo != null)
                value = pInfo.GetValue(null, null);
            else
            {
                throw new NotSupportedException(string.Format(
                        "Unhandled member type in static member expression: {0}",
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
                identityExpression = ((UnaryExpression)node.Expression).Operand as ParameterExpression;
            if (identityExpression == null)
                throw new InvalidOperationException("Failed to extract identity name from expression " + node);
            var identity = identityExpression.Name;

            var nullIdentifier = string.Empty;

            var propertyParent = node.Member.DeclaringType;
            var propertyType = propertyParent.GetProperty(node.Member.Name).PropertyType;

            if (capabilities.SupportsPropertySuffixesForControllingNullComparisons)
            {
                var isNullable = propertyType.GetTypeInfo().IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
                if (isNullable || propertyType == typeof(string)) nullIdentifier = "?";
            }

            var nodeMemberName = node.Member.GetNameUsingJsonProperty();
            lastWrittenMemberName = string.Format("{0}.{1}{2}", identity, CypherFluentQuery.ApplyCamelCase(camelCaseProperties, nodeMemberName), nullIdentifier);
            TextOutput.Append(lastWrittenMemberName);
        }


        void VisitConstantMember(MemberExpression node)
        {
            var value = GetConstantExpressionValue(node);

            // if the value is null, sending a parameter would return something we don't want.
            // A PropertyBag within the Neo4j server cannot have property with null value, that is, having a null
            // property is the same as not having the property.
            var text = TextOutput.ToString();
            if (value == null && text.EndsWith(NotEqual))
            {
                TextOutput.Remove(TextOutput.ToString().LastIndexOf(NotEqual, StringComparison.Ordinal), NotEqual.Length);
                if (capabilities.SupportsNullComparisonsWithIsOperator)
                {
                    TextOutput.Append(" is not null");
                }
                else if (capabilities.SupportsHasFunction)
                {
                    TextOutput.Remove(TextOutput.ToString().LastIndexOf(lastWrittenMemberName, StringComparison.Ordinal), lastWrittenMemberName.Length);
                    TextOutput.Append(string.Format("has({0})", lastWrittenMemberName));
                }
                else
                {
                    TextOutput.Remove(TextOutput.ToString().LastIndexOf(lastWrittenMemberName, StringComparison.Ordinal), lastWrittenMemberName.Length);
                    TextOutput.Append(string.Format("exists({0})", lastWrittenMemberName));
                }

                // no further processing is required
                return;
            }

            if (value == null && text.EndsWith(Equal))
            {
                TextOutput.Remove(TextOutput.ToString().LastIndexOf(Equal, StringComparison.Ordinal), Equal.Length);
                if (capabilities.SupportsNullComparisonsWithIsOperator)
                {
                    TextOutput.Append(" is null");
                }
                else if(capabilities.SupportsHasFunction)
                {
                    TextOutput.Remove(TextOutput.ToString().LastIndexOf(lastWrittenMemberName, StringComparison.Ordinal), lastWrittenMemberName.Length);
                    TextOutput.Append(string.Format("not(has({0}))", lastWrittenMemberName));
                }
                else
                {
                    TextOutput.Remove(TextOutput.ToString().LastIndexOf(lastWrittenMemberName, StringComparison.Ordinal), lastWrittenMemberName.Length);
                    TextOutput.Append(string.Format("not(exists({0}))", lastWrittenMemberName));
                }

                // no further processing is required
                return;
            }

            if (capabilities.SupportsPropertySuffixesForControllingNullComparisons && value != null)
            {
                SwapNullQualifierFromDefaultTrueToDefaultFalseIfTextEndsWithAny(new[]
                    {
                        Equal,
                        GreaterThan,
                        GreaterThanOrEqual,
                        LessThan,
                        LessThanOrEqual
                    });
            }

            var valueWrappedInParameter = createParameterCallback(value);
            TextOutput.Append(valueWrappedInParameter);
        }

        static object GetConstantExpressionValue(MemberExpression node)
        {
            if (node.Expression == null)
                return null;

            return Expression.Lambda(node).Compile().DynamicInvoke();
        }

        static bool IsConstantExpression(MemberExpression node)
        {
            if (node == null || node.Expression == null)
                return false;

            return node.Expression.NodeType == ExpressionType.Constant || IsConstantExpression(node.Expression as MemberExpression);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert)
                return base.VisitUnary(node);

            throw new NotSupportedException("Unary expressions, like Where(f => !f.Foo), are not supported because these become ambiguous between C# and Cypher based on how Neo4j handles null values. Use a comparison instead, like Where(f => f.Foo == false).");
        }

        void SwapNullQualifierFromDefaultTrueToDefaultFalseIfTextEndsWithAny(params string[] operators)
        {
            if (!capabilities.SupportsPropertySuffixesForControllingNullComparisons)
                throw new InvalidOperationException();

            var text = TextOutput.ToString();
            var @operator = operators.FirstOrDefault(text.EndsWith);
            if (@operator == null) return;
            TextOutput.Remove(TextOutput.ToString().LastIndexOf(@operator, StringComparison.Ordinal), @operator.Length);
            SwapNullQualifierFromDefaultTrueToDefaultFalse(TextOutput);
            TextOutput.Append(@operator);
        }

        void SwapNullQualifierFromDefaultTrueToDefaultFalse(StringBuilder text)
        {
            if (!capabilities.SupportsPropertySuffixesForControllingNullComparisons)
                throw new InvalidOperationException();

            if (!text.ToString().EndsWith("?"))
                return;
            TextOutput.Remove(TextOutput.Length - 1, 1);
            TextOutput.Append("!");
        }
    }
}