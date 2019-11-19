using System;
using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
    internal class GremlinClient : IGremlinClient
    {
        readonly IGraphClient client;

        // This is internal for now because we ultimately want to remove the dependency
        // on IGraphClient once we move the Execute* methods to here
        internal GremlinClient(IGraphClient client)
        {
            this.client = client;
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public IGremlinQuery V
        {
            get { return new GremlinQuery(client, "g.V", new Dictionary<string, object>(), new List<string>()); }
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public IGremlinQuery E
        {
            get { return new GremlinQuery(client, "g.E", new Dictionary<string, object>(), new List<string>()); }
        }
    }
}
