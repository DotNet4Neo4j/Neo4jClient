using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neo4jClient.Extensions;

namespace Neo4jClient.Cypher
{
    public class CypherWithExpressionBuilder
    {
        internal const string WithExpressionCannotBeSerializedToCypherExceptionMessage = "The With expression that you have provided uses methods other than those defined by ICypherResultItem or Neo4jClient.Cypher.All. The With expression needs to be something that we can translate to Cypher, then send to the server to be executed. You can't use chains of methods, LINQ-to-objects, or other constructs like these.";

        internal const string WithExpressionShouldBeOneOfExceptionMessage = "The expression must be constructed as either an anonymous type initializer (for example: n => new { Foo = n }), an object initializer (for example: n => new MyResultType { Foo = n.Bar }), or a method call (for example: n => n.Count()), or a member accessor (for example: n => n.As<Foo>().Bar). You cannot supply blocks of code (for example: n => { var a = n + 1; return a; }) or use constructors with arguments (for example: n => new Foo(n)).";

        internal const string CollectAsShouldNotBeNodeTExceptionMessage = "You've called CollectAs<Node<T>>(), however this method already wraps the type in Node<>. Your current code would result in Node<Node<T>>, which is invalid. Use CollectAs<T>() instead.";

        internal const string CollectAsDistinctShouldNotBeNodeTExceptionMessage = "You've called CollectAsDistinct<Node<T>>(), however this method already wraps the type in Node<>. Your current code would result in Node<Node<T>>, which is invalid. Use CollectAsDistinct<T>() instead.";

        // Terminology used in this file:
        //
        // - a "statement" is something like "x.Foo? AS Bar"
        // - "text" is a collection of statements, like "x.Foo? AS Bar, y.Baz as Qak"

        private readonly CypherCapabilities capabilities;
        private readonly bool camelCaseProperties;

        public CypherWithExpressionBuilder(CypherCapabilities capabilities, bool camelCaseProperties)
        {
            this.capabilities = capabilities ?? CypherCapabilities.Default;
            this.camelCaseProperties = camelCaseProperties;
        }

        public ReturnExpression BuildText(LambdaExpression expression)
        {
            var body = expression.Body;

            if (body.NodeType == ExpressionType.Convert &&
                body is UnaryExpression)
            {
                body = ((UnaryExpression)expression.Body).Operand;
            }

            string text;
            switch (body.NodeType)
            {
                case ExpressionType.MemberInit:
                    var memberInitExpression = (MemberInitExpression) body;
                    text = BuildText(memberInitExpression);
                    return new ReturnExpression {Text = text, ResultMode = CypherResultMode.Projection};
                case ExpressionType.New:
                    var newExpression = (NewExpression) body;
                    text = BuildText(newExpression);
                    return new ReturnExpression {Text = text, ResultMode = CypherResultMode.Projection};
                case ExpressionType.Call:
                    var methodCallExpression = (MethodCallExpression) body;
                    text = BuildText(methodCallExpression);
                    return new ReturnExpression {Text = text, ResultMode = CypherResultMode.Set};
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression) body;
                    text = BuildText(memberExpression);
                    return new ReturnExpression { Text = text, ResultMode = CypherResultMode.Set };
                default:
                    throw new ArgumentException(WithExpressionShouldBeOneOfExceptionMessage, "expression");
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
        string BuildText(MemberInitExpression expression)
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
        string BuildText(NewExpression expression)
        {
            var resultingType = expression.Constructor.DeclaringType;
            Debug.Assert(resultingType != null, "resultingType != null");

            var typeInfo = resultingType.GetTypeInfo();

            var quacksLikeAnAnonymousType =
                typeInfo.IsSpecialName &&
                typeInfo.IsValueType &&
                typeInfo.IsNestedPrivate &&
                !typeInfo.IsGenericType;
            if (expression.Members == null && !quacksLikeAnAnonymousType)
                throw new ArgumentException(WithExpressionShouldBeOneOfExceptionMessage, "expression");

            if (expression.Arguments.Count != expression.Members.Count)
                throw new InvalidOperationException("Somehow we had a different number of members than arguments. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

            var bindingTexts = expression.Members.Select((member, index) =>
            {
                var argument = expression.Arguments[index];
                return BuildStatement(argument, member);
            });

            return string.Join(", ", bindingTexts.ToArray());
        }

        /// <remarks>
        /// This build method caters to expressions like: <code>item => item.Count()</code>
        /// </remarks>
        string BuildText(MethodCallExpression expression)
        {
            return BuildStatement(expression, false);
        }

        /// <remarks>
        /// This build method caters to expressions like: <code>item => item.As&lt;Foo&gt;().Bar</code>
        /// </remarks>
        string BuildText(MemberExpression expression)
        {
            var innerExpression = expression.Expression as MethodCallExpression;
            if (innerExpression == null ||
                innerExpression.Method.DeclaringType != typeof(ICypherResultItem) ||
                innerExpression.Method.Name != "As")
                throw new ArgumentException("Member expressions are only supported off ICypherResultItem.As<TData>(). For example: Return(foo => foo.As<Bar>().Baz).", "expression");

            var baseStatement = BuildStatement(innerExpression, false);
            var statement = string.Format("{0}.{1}", baseStatement, CypherFluentQuery.ApplyCamelCase(camelCaseProperties, expression.Member.GetNameUsingJsonProperty()));

            return statement;
        }

        string BuildStatement(Expression sourceExpression, MemberInfo targetMember)
        {
            var unwrappedExpression = UnwrapImplicitCasts(sourceExpression);

            var memberExpression = unwrappedExpression as MemberExpression;
            if (memberExpression != null)
                return BuildStatement(memberExpression, targetMember);

            var methodCallExpression = unwrappedExpression as MethodCallExpression;
            if (methodCallExpression != null)
                return BuildStatement(methodCallExpression, targetMember);

            var constantExpression = unwrappedExpression as ConstantExpression;
            if (constantExpression != null)
                return BuildStatement(constantExpression, targetMember);

            var parameterExpression = unwrappedExpression as ParameterExpression;
            if (parameterExpression != null)
                return BuildStatement(parameterExpression, targetMember);

            throw new NotSupportedException(string.Format(
                "Expression of type {0} is not supported.",
                unwrappedExpression.GetType().FullName));
        }

        string BuildStatement(MemberExpression memberExpression, MemberInfo targetMember)
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

            var optionalIndicator = string.Empty;
            if (capabilities.SupportsPropertySuffixesForControllingNullComparisons)
            {
                var isTargetMemberNullable = IsMemberNullable(targetMember);
                var isNullable = isTargetMemberNullable || IsMemberNullable(memberInfo);
                if (isNullable) optionalIndicator = "?";
            }

            return string.Format("{0}.{1}{2} AS {3}", targetObject.Name, CypherFluentQuery.ApplyCamelCase(camelCaseProperties, memberInfo.Name), optionalIndicator, targetMember.Name);
        }

        static string BuildStatement(MethodCallExpression expression, MemberInfo targetMember)
        {
            var isNullable = IsMemberNullable(targetMember);
            var statement = BuildStatement(expression, isNullable);
            statement = statement + " AS " + targetMember.Name;
            return statement;
        }

        static string BuildStatement(MethodCallExpression expression, bool isNullable)
        {
            string statement;
            if (expression.Method.DeclaringType == typeof(ICypherResultItem) || expression.Method.DeclaringType == typeof(IFluentCypherResultItem))
                statement = BuildCypherResultItemStatement(expression, isNullable);
            else if (expression.Method.DeclaringType == typeof(All))
                statement = BuildCypherAllStatement(expression);
            else if (expression.Method.DeclaringType == typeof(Return))
                statement = BuildCypherReturnStatement(expression);
            else
                throw new ArgumentException(WithExpressionCannotBeSerializedToCypherExceptionMessage);

            return statement;
        }

        static string BuildStatement(ConstantExpression expression, MemberInfo targetMember)
        {
            var statement = expression.Value + " AS " + targetMember.Name;
            return statement;
        }

        static string BuildStatement(ParameterExpression expression, MemberInfo targetMember)
        {
            var statement = expression.Name;
            if (!statement.Equals(targetMember.Name, StringComparison.OrdinalIgnoreCase))
                statement += " AS " + targetMember.Name;
            return statement;
        }

        static string BuildCypherResultItemStatement(MethodCallExpression expression, bool isNullable)
        {
            Debug.Assert(expression.Object != null, "expression.Object != null");

            string statement = null;
            var targetObject = expression.Object as ParameterExpression;

            if (expression.Object.Type == typeof (IFluentCypherResultItem))
            {
                var wrappedFunctionCall = BuildWrappedFunction(expression);
                statement = wrappedFunctionCall.StatementFormat;
                targetObject = (ParameterExpression)wrappedFunctionCall.InnerExpression;
            }

            if (targetObject == null)
                throw new InvalidOperationException(
                    "Somehow targetObject ended up as null. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

            var optionalIndicator = isNullable ? "?" : "";
            string finalStatement;
            var methodName = expression.Method.Name;
            switch (methodName)
            {
                case "As":
                case "Node":
                    finalStatement = string.Format("{0}{1}", targetObject.Name, optionalIndicator);
                    break;
                case "CollectAs":
                    if (IsNodeOfT(expression.Method))
                        throw new ArgumentException(CollectAsShouldNotBeNodeTExceptionMessage, "expression");
                    finalStatement = string.Format("collect({0})", targetObject.Name);
                    break;
                case "CollectAsDistinct":
                    if (IsNodeOfT(expression.Method))
                        throw new ArgumentException(CollectAsDistinctShouldNotBeNodeTExceptionMessage, "expression");
                    finalStatement = string.Format("collect(distinct {0})", targetObject.Name);
                    break;
                case "Count":
                    finalStatement = string.Format("count({0})", targetObject.Name);
                    break;
                case "CountDistinct":
                    finalStatement = string.Format("count(distinct {0})", targetObject.Name);
                    break;
                case "Id":
                    finalStatement = string.Format("id({0})", targetObject.Name);
                    break;
                case "Length":
                    finalStatement = string.Format("length({0})", targetObject.Name);
                    break;
                case "Type":
                    finalStatement = string.Format("type({0})", targetObject.Name);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected ICypherResultItem method definition, ICypherResultItem." + methodName);
            }

            statement = statement != null
                ? string.Format(statement, finalStatement)
                : finalStatement;

            return statement;
        }

        static bool IsNodeOfT(MethodInfo methodInfo)
        {
            if (!methodInfo.IsGenericMethod) throw new InvalidOperationException("Expected generic method, but it wasn't.");
            var methodType = methodInfo.GetGenericArguments().Single();
            return methodType.GetTypeInfo().IsGenericType && methodType.GetGenericTypeDefinition() == typeof(Node<>);
        }

        static WrappedFunctionCall BuildWrappedFunction(MethodCallExpression methodCallExpression)
        {
            var targetObject = ((MethodCallExpression)methodCallExpression.Object);
            Debug.Assert(targetObject != null, "targetObject != null");

            string statementFormat;
            var methodName = targetObject.Method.Name;
            switch (methodName)
            {
                case "Head":
                    statementFormat = "head({0})";
                    break;
                case "Last":
                    statementFormat = "last({0})";
                    break;
                default:
                    throw new InvalidOperationException("Unexpected IFluentCypherResultItem method definition, IFluentCypherResultItem." + methodName);
            }

            return new WrappedFunctionCall
            {
                StatementFormat = statementFormat,
                InnerExpression = targetObject.Object
            };
        }

        class WrappedFunctionCall
        {
            public string StatementFormat { get; set; }
            public Expression InnerExpression { get; set; }
        }

        static string BuildCypherAllStatement(MethodCallExpression expression)
        {
            var methodName = expression.Method.Name;
            switch (methodName)
            {
                case "Count":
                    return "count(*)";
                default:
                    throw new InvalidOperationException("Unexpected All method definition, All." + methodName);
            }
        }

        static string BuildCypherReturnStatement(MethodCallExpression expression)
        {
            var methodName = expression.Method.Name;
            switch (methodName)
            {
                case "As":
                    var cypherTextExpression = expression.Arguments.Single();
                    var cypherText = Expression.Lambda<Func<string>>(cypherTextExpression).Compile()();
                    return cypherText;
                default:
                    throw new InvalidOperationException("Unexpected Return method definition, Return." + methodName);
            }
        }

        static Expression UnwrapImplicitCasts(Expression expression)
        {
            if (expression is UnaryExpression)
            {
                expression = ((UnaryExpression)expression).Operand;
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
            Type memberType = null;

            var propertyInfo = declaringType.GetProperty(memberName);
            if (propertyInfo != null)
                memberType = propertyInfo.PropertyType;

            if (memberType == null)
            {
                var fieldInfo = declaringType.GetField(memberName);
                if (fieldInfo != null)
                    memberType = fieldInfo.FieldType;
            }

            return IsTypeNullable(memberType);
        }

        static bool IsTypeNullable(Type type)
        {
            if (type == null) return false;
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return true;
            if (type == typeof(string)) return true;
            return false;
        }
    }
}