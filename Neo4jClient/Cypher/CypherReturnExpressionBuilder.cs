using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neo4jClient.Serialization;
using Newtonsoft.Json;

namespace Neo4jClient.Cypher
{
    public class CypherReturnExpressionBuilder
    {
        internal const string ReturnExpressionCannotBeSerializedToCypherExceptionMessage = "The return expression that you have provided uses methods other than those defined by ICypherResultItem, Neo4jClient.Cypher.All or Neo4jClient.Cypher.Return. The return expression needs to be something that we can translate to Cypher, then send to the server to be executed. You can't use chains of methods, LINQ-to-objects, or other constructs like these. If you want to run client-side logic to reshape your data in .NET, use a Select call after the query has been executed, like .Return(…).Results.Select(r => …). This technique maintains a clear separation between what is being executed server-side (in Neo4j, via Cypher) versus client-side (back in .NET).";

        internal const string ReturnExpressionCannotBeStruct = "The expression must be constructed from an object initializer with a constructor without arguments. This means that structs cannot be used as part of the return expression.";

        internal const string ReturnExpressionShouldBeOneOfExceptionMessage = "The expression must be constructed as either an object initializer (for example: n => new MyResultType { Foo = n.Bar }), an anonymous type initializer (for example: n => new { Foo = n.Bar }), a method call (for example: n => n.Count()), or a member accessor (for example: n => n.As<Foo>().Bar). You cannot supply blocks of code (for example: n => { var a = n + 1; return a; }) or use constructors with arguments (for example: n => new Foo(n)). If you're in F#, tuples are also supported.";

        internal const string ReturnAsTypeShouldBeOneOfExceptionMessage = "You've called As<{0}>() in your return clause, where {0} is not a supported type. It must be a simple type (like int, string, or long), a class with a default constructor (so that we can deserialize into it), RelationshipInstance, RelationshipInstance<T>, list of RelationshipInstance, or list of RelationshipInstance<T>.";

        internal const string CollectAsShouldNotBeNodeTExceptionMessage = "You've called CollectAs<Node<T>>(), however this method already wraps the type in Node<>. Your current code would result in Node<Node<T>>, which is invalid. Use CollectAs<T>() instead.";

        internal const string CollectAsDistinctShouldNotBeNodeTExceptionMessage = "You've called CollectAsDistinct<Node<T>>(), however this method already wraps the type in Node<>. Your current code would result in Node<Node<T>>, which is invalid. Use CollectAsDistinct<T>() instead.";

        internal const string UnsupportedBinaryExpressionExceptionMessageFormat = "We don't currently support {0} as an expression. Please raise an issue at https://github.com/Readify/Neo4jClient including your query code.";

        internal const string UnsupportedBinaryExpressionComparisonExceptionMessage = "We don't currently support anything other than null for a binary expression comparison. Please raise an issue at https://github.com/Readify/Neo4jClient including your query code.";

        // Terminology used in this file:
        //
        // - a "statement" is something like "x.Foo? AS Bar"
        // - "text" is a collection of statements, like "x.Foo? AS Bar, y.Baz as Qak"

        public static ReturnExpression BuildText(
            LambdaExpression expression,
            CypherCapabilities capabilities,
            IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse,
            bool camelCaseProperties = false)
        {
            capabilities = capabilities ?? CypherCapabilities.Default;

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
                    text = BuildText(memberInitExpression, capabilities, jsonConvertersThatTheDeserializerWillUse,camelCaseProperties);
                    return new ReturnExpression {Text = text, ResultMode = CypherResultMode.Projection};
                case ExpressionType.New:
                    var newExpression = (NewExpression) body;
                    text = BuildText(newExpression, capabilities, jsonConvertersThatTheDeserializerWillUse, camelCaseProperties);
                    return new ReturnExpression {Text = text, ResultMode = CypherResultMode.Projection};
                case ExpressionType.Call:
                    var methodCallExpression = (MethodCallExpression) body;
                    text = BuildText(methodCallExpression, capabilities, jsonConvertersThatTheDeserializerWillUse);
                    return new ReturnExpression {Text = text, ResultMode = CypherResultMode.Set};
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression) body;
                    text = BuildText(memberExpression, capabilities, jsonConvertersThatTheDeserializerWillUse, camelCaseProperties);
                    return new ReturnExpression { Text = text, ResultMode = CypherResultMode.Set };
                default:
                    throw new ArgumentException(ReturnExpressionShouldBeOneOfExceptionMessage, "expression");
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
        /// <see cref="BuildText(NewExpression, CypherCapabilities, IEnumerable&lt;JsonConverter&gt;)"/> caters to anonymous types.
        /// </remarks>
        static string BuildText(
            MemberInitExpression expression,
            CypherCapabilities capabilities,
            IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse,
            bool camelCaseProperties)
        {
            if (expression.NewExpression.Constructor == null)
            {
                throw new ArgumentException(ReturnExpressionCannotBeStruct);
            }

            if (expression.NewExpression.Constructor.GetParameters().Any())
                throw new ArgumentException(
                    "The result type must be constructed using a parameterless constructor. For example: n => new MyResultType { Foo = n.Bar }",
                    "expression");

            var bindingTexts = expression.Bindings.Select(binding =>
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                    throw new ArgumentException("All bindings must be assignments. For example: n => new MyResultType { Foo = n.Bar }", "expression");

                var memberAssignment = (MemberAssignment)binding;
                return BuildStatement(memberAssignment.Expression, binding.Member, capabilities, jsonConvertersThatTheDeserializerWillUse,camelCaseProperties);
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
        static string BuildText(
            NewExpression expression,
            CypherCapabilities capabilities,
            IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse,
            bool camelCaseProperties)
        {
            var constructor = expression.Constructor;
            if (constructor == null)
            {
                throw new ArgumentException(ReturnExpressionCannotBeStruct);
            }

            var resultingType = constructor.DeclaringType;
            var quacksLikeAnAnonymousType =
                resultingType != null &&
                resultingType.IsSpecialName &&
                resultingType.IsValueType &&
                resultingType.IsNestedPrivate &&
                !resultingType.IsGenericType;
            var expressionMembers = expression.Members;
            if (expressionMembers == null && !quacksLikeAnAnonymousType)
            {
                // expression.Members is null for Tuples and Record types generated by F#
                // ref: https://fsharppowerpack.codeplex.com/workitem/4572
                var reflectedMembers = resultingType.GetProperties(); //resultingType.GetMembers() gets all members, we only want properties 
                if (reflectedMembers == null || !resultingType.FullName.StartsWith("System.Tuple`"))
                {
                    throw new ArgumentException(ReturnExpressionShouldBeOneOfExceptionMessage, "expression");
                }
                expressionMembers = new System.Collections.ObjectModel.ReadOnlyCollection<MemberInfo>(reflectedMembers);
            }

            if (expressionMembers == null)
            {
                throw new ArgumentException(ReturnExpressionShouldBeOneOfExceptionMessage, "expression");
            }

            if (expression.Arguments.Count != expressionMembers.Count)
                throw new InvalidOperationException("Somehow we had a different number of members than arguments. We weren't expecting this to happen. Please raise an issue at http://hg.readify.net/neo4jclient including your query code.");

            var bindingTexts = expressionMembers.Select((member, index) =>
            {
                var argument = expression.Arguments[index];
                return BuildStatement(argument, member, capabilities, jsonConvertersThatTheDeserializerWillUse,camelCaseProperties);
            });

            return string.Join(", ", bindingTexts.ToArray());
        }

        /// <remarks>
        /// This build method caters to expressions like: <code>item => item.Count()</code>
        /// </remarks>
        static string BuildText(
            MethodCallExpression expression,
            CypherCapabilities capabilities,
            IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse)
        {
            return BuildStatement(expression, false, capabilities, jsonConvertersThatTheDeserializerWillUse);
        }

        /// <remarks>
        /// This build method caters to expressions like: <code>item => item.As&lt;Foo&gt;().Bar</code>
        /// </remarks>
        static string BuildText(
            MemberExpression expression,
            CypherCapabilities capabilities,
            IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse, 
            bool camelCaseProperties)
        {
            var innerExpression = expression.Expression as MethodCallExpression;
            if (innerExpression == null ||
                innerExpression.Method.DeclaringType != typeof(ICypherResultItem) ||
                innerExpression.Method.Name != "As")
                throw new ArgumentException("Member expressions are only supported off ICypherResultItem.As<TData>(). For example: Return(foo => foo.As<Bar>().Baz).", "expression");

            var baseStatement = BuildStatement(innerExpression, false, capabilities, jsonConvertersThatTheDeserializerWillUse);
            var statement = string.Format("{0}.{1}", baseStatement, CypherFluentQuery.ApplyCamelCase(camelCaseProperties, expression.Member.Name));

            return statement;
        }

        static string BuildStatement(
            Expression sourceExpression,
            MemberInfo targetMember,
            CypherCapabilities capabilities,
            IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse,
            bool camelCaseProperties)
        {
            var unwrappedExpression = UnwrapImplicitCasts(sourceExpression);

            var memberExpression = unwrappedExpression as MemberExpression;
            if (memberExpression != null)
                return BuildStatement(memberExpression, targetMember, capabilities, camelCaseProperties);

            var methodCallExpression = unwrappedExpression as MethodCallExpression;
            if (methodCallExpression != null)
                return BuildStatement(methodCallExpression, targetMember, capabilities, jsonConvertersThatTheDeserializerWillUse);

            var binaryExpression = unwrappedExpression as BinaryExpression;
            if(binaryExpression != null)
                return BuildStatement(binaryExpression, targetMember);

            throw new NotSupportedException(string.Format(
                "Expression of type {0} is not supported.",
                unwrappedExpression.GetType().FullName));
        }

        static string BuildStatement(
            BinaryExpression binaryExpression,
            MemberInfo targetMember)
        {
            string expression;
            switch (binaryExpression.NodeType)
            {
                case ExpressionType.NotEqual:
                    expression = "IS NOT";
                    break;
                case ExpressionType.Equal:
                    expression = "IS";
                    break;
                default:
                    throw new NotSupportedException(string.Format(UnsupportedBinaryExpressionExceptionMessageFormat, binaryExpression.NodeType));
            }

            var comparison = (ConstantExpression) binaryExpression.Right;
            if(comparison.Value != null)
                throw new NotSupportedException(UnsupportedBinaryExpressionComparisonExceptionMessage);

            var targetObject = (ParameterExpression)binaryExpression.Left;
            return string.Format("{0} {1} NULL AS {2}", targetObject.Name, expression, targetMember.Name);
        }

        static string BuildStatement(
            MemberExpression memberExpression,
            MemberInfo targetMember,
            CypherCapabilities capabilities,
            bool camelCaseProperties)
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
                    "Somehow targetObject ended up as null. We weren't expecting this to happen. Please raise an issue at https://github.com/Readify/Neo4jClient including your query code.");

            var optionalIndicator = "";
            if (capabilities.SupportsPropertySuffixesForControllingNullComparisons)
            {
                var isTargetMemberNullable = IsMemberNullable(targetMember);
                var isNullable = isTargetMemberNullable || IsMemberNullable(memberInfo);
                if (isNullable) optionalIndicator = "?";
            }

            return string.Format("{0}.{1}{2} AS {3}", targetObject.Name, CypherFluentQuery.ApplyCamelCase(camelCaseProperties, memberInfo.Name), optionalIndicator, targetMember.Name);
        }

        static string BuildStatement(
            MethodCallExpression expression,
            MemberInfo targetMember,
            CypherCapabilities capabilities,
            IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse)
        {
            var isNullable = IsMemberNullable(targetMember);
            var statement = BuildStatement(expression, isNullable, capabilities, jsonConvertersThatTheDeserializerWillUse);
            statement = statement + " AS " + targetMember.Name;
            return statement;
        }

        static string BuildStatement(
            MethodCallExpression expression,
            bool isNullable,
            CypherCapabilities capabilities,
            IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse)
        {
            string statement;
            if (expression.Method.DeclaringType == typeof(ICypherResultItem) || expression.Method.DeclaringType == typeof(IFluentCypherResultItem))
                statement = BuildCypherResultItemStatement(expression, isNullable, capabilities, jsonConvertersThatTheDeserializerWillUse);
            else if (expression.Method.DeclaringType == typeof(All))
                statement = BuildCypherAllStatement(expression);
            else if (expression.Method.DeclaringType == typeof(Return))
                statement = BuildCypherReturnStatement(expression);
            else
                throw new ArgumentException(ReturnExpressionCannotBeSerializedToCypherExceptionMessage);

            return statement;
        }

        static string BuildCypherResultItemStatement(
            MethodCallExpression expression,
            bool isNullable,
            CypherCapabilities capabilities,
            IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse)
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

            var optionalIndicator = capabilities.SupportsPropertySuffixesForControllingNullComparisons && isNullable ? "?" : "";
            string finalStatement;
            var methodName = expression.Method.Name;
            var singleGenericArgument = expression.Method.IsGenericMethod
                ? expression.Method.GetGenericArguments().Single()
                : null;

            switch (methodName)
            {
                case "As":
                case "Node":
                    Debug.Assert(singleGenericArgument != null);
                    if (!IsSupportedForAs(singleGenericArgument, jsonConvertersThatTheDeserializerWillUse))
                        throw new ArgumentException(string.Format(ReturnAsTypeShouldBeOneOfExceptionMessage, singleGenericArgument.Name), "expression");
                    finalStatement = string.Format("{0}{1}", targetObject.Name, optionalIndicator);
                    break;
                case "CollectAs":
                    if (IsNodeOfT(singleGenericArgument))
                        throw new ArgumentException(CollectAsShouldNotBeNodeTExceptionMessage, "expression");
                    finalStatement = string.Format("collect({0})", targetObject.Name);
                    break;
                case "CollectAsDistinct":
                    if (IsNodeOfT(singleGenericArgument))
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
                case "Labels":
                    finalStatement = string.Format("labels({0})", targetObject.Name);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected ICypherResultItem method definition, ICypherResultItem." + methodName);
            }

            statement = statement != null
                ? string.Format(statement, finalStatement)
                : finalStatement;

            return statement;
        }

        static bool IsNodeOfT(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Node<>);
        }

        static bool IsSupportedForAs(Type type, IEnumerable<JsonConverter> jsonConvertersThatTheDeserializerWillUse)
        {
            if (type == null) throw new ArgumentNullException("type");

            var jsonConvertersThatTheDeserializerWillUseAsArray = jsonConvertersThatTheDeserializerWillUse != null
                ? jsonConvertersThatTheDeserializerWillUse.ToArray() : null;

            if (IsSupportedElementForAs(type, jsonConvertersThatTheDeserializerWillUseAsArray))
                return true;

            if (type.IsArray)
                return IsSupportedElementForAs(type.GetElementType(), jsonConvertersThatTheDeserializerWillUseAsArray);

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof (IEnumerable<>)
                    || genericTypeDefinition == typeof (ICollection<>)
                    || genericTypeDefinition == typeof (IList<>))
                {
                    var genericArguments = type.GetGenericArguments();
                    if (genericArguments.Length == 1)
                    {
                        var onlyGenericArgument = genericArguments[0];
                        if (IsSupportedElementForAs(onlyGenericArgument, jsonConvertersThatTheDeserializerWillUseAsArray))
                            return true;
                    }
                }
            }

            return false;
        }

        static bool IsSupportedElementForAs(Type type, IList<JsonConverter> jsonConvertersThatTheDeserializerWillUse)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (TypeConverterBasedJsonConverter.BuiltinTypes.Contains(type))
                return true;

            jsonConvertersThatTheDeserializerWillUse = jsonConvertersThatTheDeserializerWillUse ?? GraphClient.DefaultJsonConverters;
            if (jsonConvertersThatTheDeserializerWillUse.Any(c => c.CanConvert(type)))
                return true;

            var hasDefaultConstructor = type.GetConstructor(Type.EmptyTypes) != null;
            if (hasDefaultConstructor)
                return true;

            if (type == typeof(RelationshipInstance))
                return true;

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if ((genericTypeDefinition == typeof(RelationshipInstance<>)
                     || genericTypeDefinition == typeof(Node<>)))
                    return true;
            }

            return false;
        }

        static WrappedFunctionCall BuildWrappedFunction(MethodCallExpression methodCallExpression)
        {
            var targetObject = ((MethodCallExpression) methodCallExpression.Object);
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
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return true;
            if (type == typeof(string)) return true;
            return false;
        }
    }
}
