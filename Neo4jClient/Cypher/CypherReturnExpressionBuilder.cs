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

                // TODO: Store this for ordering later
                //binding.Member.Name

                return targetObject.Name + "." + memberExpression.Member.Name;
            });

            return string.Join(", ", bindingTexts.ToArray());
        }
    }
}
