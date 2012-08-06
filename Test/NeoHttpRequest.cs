using System.Collections.Generic;
using RestSharp;

namespace Neo4jClient.Test
{
    public class NeoHttpRequest : RestRequest, IMockRequestDefinition
    {
        public object Body
        {
            set { AddBody(value); }
        }

        IEnumerable<Parameter> IMockRequestDefinition.Parameters
        {
            get { return Parameters; }
        }
    }
}
