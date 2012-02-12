using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    internal class CypherStartBit
    {
        readonly string identity;
        readonly string lookupType;
        readonly IEnumerable<int> lookupIds;

        public CypherStartBit(string identity, string lookupType, IEnumerable<int> lookupIds)
        {
            this.identity = identity;
            this.lookupType = lookupType;
            this.lookupIds = lookupIds;
        }

        public string Identity
        {
            get { return identity; }
        }

        public string LookupType
        {
            get { return lookupType; }
        }

        public IEnumerable<int> LookupIds
        {
            get { return lookupIds; }
        }
    }
}