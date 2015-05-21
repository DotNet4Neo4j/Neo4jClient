using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Neo4jClient.Cypher
{
    internal class VbCompareReplacer : ExpressionVisitor
    {

        public override Expression Visit(Expression node)
        {
            if (!(node is BinaryExpression))
            {
                return base.Visit(node);
            }

            BinaryExpression binaryExpression = (BinaryExpression)node;

            if (!(binaryExpression.Left is MethodCallExpression))
            {
                return base.Visit(node);
            }

            MethodCallExpression m = (MethodCallExpression)binaryExpression.Left;
            dynamic method = (MethodCallExpression)binaryExpression.Left;

            if (!(method.Method.DeclaringType == typeof(Microsoft.VisualBasic.CompilerServices.Operators) && method.Method.Name == "CompareString"))
            {
                return base.Visit(node);
            }

            dynamic left = method.Arguments[0];
            dynamic right = method.Arguments[1];

            return binaryExpression.NodeType == ExpressionType.Equal ? Expression.Equal(left, right) : Expression.NotEqual(left, right);
        }

    }
}
