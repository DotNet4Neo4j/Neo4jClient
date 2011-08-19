using System.Collections.Generic;

namespace Neo4jClient
{
    public class IndexEntry
    {
        public string Name { get; set; }
        public IEnumerable<KeyValuePair<string, object>> KeyValues { get; set; }
    }
}