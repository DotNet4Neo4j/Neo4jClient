using System;
using System.Linq;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public class CypherReturnExpressionBuilder
    {
        public static string BuildText<TResult>(Expression<Func<ICypherResultItem, TResult>> expression)
            where TResult : new()
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

                var memberAssigment = (MemberAssignment)binding;
                var memberExpression = (MemberExpression)memberAssigment.Expression;
                var methodCallExpression = (MethodCallExpression)memberExpression.Expression;
                var targetObject = (ParameterExpression)methodCallExpression.Object;

                if (targetObject == null)
                    throw new InvalidOperationException("Somehow targetObject ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

                var memberName = memberExpression.Member.Name;
                var declaringType = memberExpression.Member.DeclaringType;
                if (declaringType == null)
                    throw new InvalidOperationException("Somehow declaringType ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

                var isNullable = IsMemberNullable(memberName, declaringType);

                var optionalIndicator = isNullable ? "?" : "";

                return string.Format("{0}.{1}{2} AS {3}", targetObject.Name, memberName, optionalIndicator, binding.Member.Name);
            });

            return string.Join(", ", bindingTexts.ToArray());
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
                memberType.IsGenericType &&
                memberType.GetGenericTypeDefinition() == typeof (Nullable<>);
            return isNullable;
        }
    }
}
