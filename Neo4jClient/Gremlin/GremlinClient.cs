using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    internal class GremlinClient : IGremlinClient
    {
        readonly IGraphClient client;

        // This is internal for now because we ultimately want to remove the dependency
        // on IGraphClient once we move the Execute* methods to here
        internal GremlinClient(IGraphClient client)
        {
            this.client = client;
        }

        public IGremlinQuery V
        {
            get { return new GremlinQuery(client, "g.V", new Dictionary<string, object>(), new List<string>()); }
        }

        public IGremlinQuery E
        {
            get { return new GremlinQuery(client, "g.E", new Dictionary<string, object>(), new List<string>()); }
        }
    }
}
