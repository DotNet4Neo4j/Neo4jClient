using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    internal struct FormattedFilter
    {
        public string FilterText { get; set; }
        public IDictionary<string, object> FilterParameters { get; set; }
    }
}
