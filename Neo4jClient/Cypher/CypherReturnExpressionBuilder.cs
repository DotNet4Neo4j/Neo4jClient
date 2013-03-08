using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClient.Cypher
{
    public class CypherReturnExpressionBuilder
    {
        // Terminology used in this file:
        //
        // - a "statement" is something like "x.Foo? AS Bar"
        // - "text" is a collection of statements, like "x.Foo? AS Bar, y.Baz as Qak"

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

            var bindingTexts = expression.Bindings.Select(binding =>
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                    throw new ArgumentException("All bindings must be assignments. For example: n => new MyResultType { Foo = n.Bar }", "expression");

                var memberAssignment = (MemberAssignment)binding;
                return BuildStatement(memberAssignment.Expression, binding.Member);
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
                return BuildStatement(argument, member);
            });

            return string.Join(", ", bindingTexts.ToArray());
        }

        static string BuildStatement(Expression sourceExpression, MemberInfo targetMember)
        {
            var unwrappedExpression = UnwrapImplicitCasts(sourceExpression);

            var memberExpression = unwrappedExpression as MemberExpression;
            if (memberExpression != null)
                return BuildStatement(memberExpression, targetMember);

            var methodCallExpression = unwrappedExpression as MethodCallExpression;
            if (methodCallExpression != null)
                return BuildStatement(methodCallExpression, targetMember);

            throw new NotSupportedException(string.Format(
                "Expression of type {0} is not supported.",
                unwrappedExpression.GetType().FullName));
        }

        static string BuildStatement(MemberExpression memberExpression, MemberInfo targetMember)
        {
            MethodCallExpression methodCallExpression;
            MemberInfo memberInfo;
            if (memberExpression.NodeType == ExpressionType.MemberAccess && memberExpression.Expression.NodeType == ExpressionType.Call)
            {
                methodCallExpression = (MethodCallExpression) memberExpression.Expression;
                memberInfo = memberExpression.Member;
            }
            else if (memberExpression.NodeType == ExpressionType.MemberAccess && memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                var nextedExpression = ((MemberExpression) memberExpression.Expression);
                methodCallExpression = (MethodCallExpression) nextedExpression.Expression;
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

            var isTargetMemberNullable = IsMemberNullable(targetMember);
            var isNullable = isTargetMemberNullable || IsMemberNullable(memberInfo);

            var optionalIndicator = isNullable ? "?" : "";

            return string.Format("{0}.{1}{2} AS {3}", targetObject.Name, memberInfo.Name, optionalIndicator, targetMember.Name);
        }

        static string BuildStatement(MethodCallExpression expression, MemberInfo targetMember)
        {
            var targetObject = (ParameterExpression)expression.Object;

            if (targetObject == null)
                throw new InvalidOperationException(
                    "Somehow targetObject ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

            var isNullable = IsMemberNullable(targetMember);

            var optionalIndicator = isNullable ? "?" : "";

            if (expression.Method.Name.Equals("CollectAs"))
                return string.Format("collect({0}) AS {1}", targetObject.Name, targetMember.Name);

            if (expression.Method.Name.Equals("CollectAsDistinct"))
                return string.Format("collect(distinct {0}) AS {1}", targetObject.Name, targetMember.Name);
            
            return string.Format("{0}{1} AS {2}", targetObject.Name, optionalIndicator, targetMember.Name);
        }

        static Expression UnwrapImplicitCasts(Expression expression)
        {
            if (expression is UnaryExpression)
            {
                expression = ((UnaryExpression) expression).Operand;
            }
            return expression;
        }

        static bool IsMemberNullable(MemberInfo memberInfo)
        {
            var declaringType = memberInfo.DeclaringType;
            if (declaringType == null)
                throw new InvalidOperationException(
                    "Somehow declaringType ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");
            return IsMemberNullable(memberInfo.Name, declaringType);
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
