using System.Linq.Expressions;

namespace Neo4jClient.Gremlin
{
    public struct Filter
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public ExpressionType? ExpressionType { get; set; }
    }
}
