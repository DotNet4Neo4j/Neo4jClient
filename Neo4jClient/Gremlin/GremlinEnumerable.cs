using System.Collections;
using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    internal class GremlinEnumerable : IGremlinEnumerable
    {
        readonly string queryText;

        public GremlinEnumerable(string queryText)
        {
            this.queryText = queryText;
        }

        IEnumerator<NodeReference> IEnumerable<NodeReference>.GetEnumerator()
        {
            throw new System.NotImplementedException(queryText);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<NodeReference>)this).GetEnumerator();
        }

        string IGremlinQuery.QueryText
        {
            get { return queryText; }
        }
    }
}