using System;
using System.Linq;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public class CypherReturnExpressionBuilder
    {
        public static string BuildText(LambdaExpression expression)
        {
            if (expression.Body.NodeType != ExpressionType.MemberInit)
                throw new ArgumentException("The expression must be constructed as an object initialized. For example: n => new MyResultType { Foo = n.Bar }", "expression");

            var memberInitExpression = (MemberInitExpression) expression.Body;
            if (memberInitExpression.NewExpression.Constructor.GetParameters().Any())
                throw new ArgumentException("The result type must be constructed using a parameterless constructor. For example: n => new MyResultType { Foo = n.Bar }", "expression");

            var bindings = memberInitExpression.Bindings;

            var bindingTexts = bindings.Select(binding =>
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                    throw new ArgumentException("All bindings must be assignments. For example: n => new MyResultType { Foo = n.Bar }", "expression");

                var memberAssignment = (MemberAssignment)binding;
                var memberExpression = (MemberExpression)UnwrapImplicitCasts(memberAssignment.Expression);
                var methodCallExpression = (MethodCallExpression)memberExpression.Expression;
                var targetObject = (ParameterExpression)methodCallExpression.Object;

                if (targetObject == null)
                    throw new InvalidOperationException("Somehow targetObject ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

                var bindingDeclaringType = binding.Member.DeclaringType;
                if (bindingDeclaringType == null)
                    throw new InvalidOperationException("Somehow bindingDeclaringType ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");
                var bindingMemberName = binding.Member.Name;
                var isNullable = IsMemberNullable(bindingMemberName, bindingDeclaringType);

                var optionalIndicator = isNullable ? "?" : "";

                return string.Format("{0}.{1}{2} AS {3}", targetObject.Name, memberExpression.Member.Name, optionalIndicator, bindingMemberName);
            });

            return string.Join(", ", bindingTexts.ToArray());
        }

        static Expression UnwrapImplicitCasts(Expression expression)
        {
            if (expression is UnaryExpression)
            {
                expression = ((UnaryExpression) expression).Operand;
            }
            return expression;
        }

        static bool IsMemberNullable(string memberName, Type declaringType)
        {
            var propertyInfo = declaringType.GetProperty(memberName);
            var fieldInfo = declaringType.GetField(memberName);
            Type memberType = null;
            if (propertyInfo != null)
                memberType = propertyInfo.PropertyType;
            else if (fieldInfo != null)
                memberType = fieldInfo.FieldType;
            var isNullable =
                memberType != null &&
                ((memberType.IsGenericType &&
                memberType.GetGenericTypeDefinition() == typeof (Nullable<>)) || memberType == typeof(string));
            return isNullable;
        }
    }
}
