using System;
using System.Linq.Expressions;

namespace Neo4jClient.Gremlin
{
    internal struct TypeFilter
    {
        public ExpressionType ExpressionType { get; set; }
        public Type Type { get; set; }
        public string FilterFormat { get; set; }

        public string NullFilterExpression
        {
            get
            {
                switch (ExpressionType)
                {
                    case ExpressionType.Equal:
                        return "it.'{0}' == null";
                    case ExpressionType.NotEqual:
                        return "it.'{0}' != null";
                    default:
                        throw new NotSupportedException(string.Format("Expression is not supported when comparing with a null value."));
                }
            }
        }
    }
}