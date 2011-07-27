using System.Linq.Expressions;

namespace Neo4jClient.Gremlin
{
    internal struct Filter
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public ExpressionType ExpressionType { get; set; }
    }
}
