using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Neo4jClient.Cypher
{
    public class CypherWhereExpressionBuilder
    {
        public static string BuildText(LambdaExpression expression, Func<object, string> createParameterCallback)
        {
            var myVisitor = new CypherWhereExpressionVisitor(createParameterCallback);
            myVisitor.Visit(expression);
            return myVisitor.TextOutput.ToString();
        }
    }
}
