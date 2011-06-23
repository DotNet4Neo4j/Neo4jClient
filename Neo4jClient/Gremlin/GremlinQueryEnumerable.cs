using System.Collections;
using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    internal class GremlinQueryEnumerable :
        IEnumerable<NodeReference>,
        IGremlinQuery
    {
        readonly string queryText;

        public GremlinQueryEnumerable(string queryText)
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