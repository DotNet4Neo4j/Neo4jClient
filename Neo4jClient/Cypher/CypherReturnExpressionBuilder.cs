using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClient.Cypher
{
    public class CypherReturnExpressionBuilder
    {
        public static string BuildText(LambdaExpression expression)
        {
            switch (expression.Body.NodeType)
            {
                case ExpressionType.MemberInit:
                    var memberInitExpression = (MemberInitExpression) expression.Body;
                    return BuildText(memberInitExpression);
                case ExpressionType.New:
                    var newExpression = (NewExpression) expression.Body;
                    return BuildText(newExpression);
                default:
                    throw new ArgumentException("The expression must be constructed as either an object initializer (for example: n => new MyResultType { Foo = n.Bar }), or an anonymous type (for example: n => new { Foo = n.Bar }).", "expression");
            }
        }

        /// <remarks>
        /// This build method caters to object initializers, like:
        /// 
        ///     new MyType { Foo = "Bar", Baz = "Qak" }
        /// 
        /// It does not however cater to anonymous types, as they don't compile
        /// down to traditional object initializers.
        /// 
        /// <see cref="BuildText(NewExpression)"/> caters to anonymous types.
        /// </remarks>
        static string BuildText(MemberInitExpression expression)
        {
            if (expression.NewExpression.Constructor.GetParameters().Any())
                throw new ArgumentException(
                    "The result type must be constructed using a parameterless constructor. For example: n => new MyResultType { Foo = n.Bar }",
                    "expression");

            var bindings = expression.Bindings;

            var bindingTexts = bindings.Select(binding =>
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                    throw new ArgumentException("All bindings must be assignments. For example: n => new MyResultType { Foo = n.Bar }", "expression");

                var memberAssignment = (MemberAssignment)binding;
                var memberExpression = (MemberExpression)UnwrapImplicitCasts(memberAssignment.Expression);
                return BuildText(memberExpression, binding.Member);
            });

            return string.Join(", ", bindingTexts.ToArray());
        }

        /// <remarks>
        /// This C#:
        /// 
        ///     new { Foo = "Bar", Baz = "Qak" }
        /// 
        /// translates to:
        /// 
        ///     new __SomeAnonymousType("Bar", "Qak")
        /// 
        /// which is then a NewExpression rather than a MemberInitExpression.
        /// 
        /// This is the scenario that this build method caters for.
        /// </remarks>
        static string BuildText(NewExpression expression)
        {
            if (expression.Arguments.Count != expression.Members.Count)
                throw new InvalidOperationException("Somehow we had a different number of members than arguments. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

            var bindingTexts = expression.Members.Select((member, index) =>
            {
                var argument = expression.Arguments[index];
                var unwrappedExpression = UnwrapImplicitCasts(argument);

                var memberExpression = unwrappedExpression as MemberExpression;
                if (memberExpression != null)
                    return BuildText(memberExpression, member);

                var methodCallExpression = unwrappedExpression as MethodCallExpression;
                if (methodCallExpression != null)
                    return BuildText(methodCallExpression, member);

                throw new NotSupportedException(string.Format("Expression of type {0} is not supported.", unwrappedExpression.GetType().FullName));
            });

            return string.Join(", ", bindingTexts.ToArray());
        }

        static string BuildText(MemberExpression memberExpression, MemberInfo member)
        {
            MethodCallExpression methodCallExpression;
            string memberName;
            MemberInfo memberInfo;
            if (memberExpression.NodeType == ExpressionType.MemberAccess && memberExpression.Expression.NodeType == ExpressionType.Call)
            {
                methodCallExpression = (MethodCallExpression) memberExpression.Expression;
                memberName = memberExpression.Member.Name;
                memberInfo = memberExpression.Member;
            }
            else if (memberExpression.NodeType == ExpressionType.MemberAccess && memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                var nextedExpression = ((MemberExpression) memberExpression.Expression);
                methodCallExpression = (MethodCallExpression) nextedExpression.Expression;
                memberName = nextedExpression.Member.Name;
                memberInfo = nextedExpression.Member;
            }
            else
            {
                throw new NotSupportedException(string.Format("The expression {0} is not supported", memberExpression));
            }
            var targetObject = (ParameterExpression) methodCallExpression.Object;

            if (targetObject == null)
                throw new InvalidOperationException(
                    "Somehow targetObject ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

            var bindingDeclaringType = member.DeclaringType;
            if (bindingDeclaringType == null)
                throw new InvalidOperationException(
                    "Somehow bindingDeclaringType ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");
            var bindingMemberName = member.Name;
            var isNullable = IsMemberNullable(bindingMemberName, bindingDeclaringType) || IsMemberNullable(memberName, memberInfo.DeclaringType);

            var optionalIndicator = isNullable ? "?" : "";

            return string.Format("{0}.{1}{2} AS {3}", targetObject.Name, memberName, optionalIndicator, bindingMemberName);
        }

        static string BuildText(MethodCallExpression expression, MemberInfo member)
        {
            var targetObject = (ParameterExpression)expression.Object;

            if (targetObject == null)
                throw new InvalidOperationException(
                    "Somehow targetObject ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

            var bindingDeclaringType = member.DeclaringType;
            if (bindingDeclaringType == null)
                throw new InvalidOperationException(
                    "Somehow bindingDeclaringType ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");
            var bindingMemberName = member.Name;
            var isNullable = IsMemberNullable(bindingMemberName, bindingDeclaringType);

            var optionalIndicator = isNullable ? "?" : "";

            return string.Format("{0}{1} AS {2}", targetObject.Name, optionalIndicator, bindingMemberName);
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
