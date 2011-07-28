using System;
using System.Linq.Expressions;

namespace Neo4jClient.Gremlin
{
    internal struct TypeFilter
    {
        public ExpressionType ExpressionType { get; set; }
        public Type Type { get; set; }
        public string FilterFormat { get; set; }
    }
}