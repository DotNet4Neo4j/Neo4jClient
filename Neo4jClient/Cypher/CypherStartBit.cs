using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.Cypher
{
    [Obsolete("Use IGraphClient.Cypher.Start(new { foo = nodeRef1 }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
    public class CypherStartBit : ICypherStartBit
    {
        readonly string identifier;
        readonly string lookupType;
        readonly IEnumerable<long> lookupIds;

        public CypherStartBit(string identifier, string lookupType, IEnumerable<long> lookupIds)
        {
            this.identifier = identifier;
            this.lookupType = lookupType;
            this.lookupIds = lookupIds;
        }

        public CypherStartBit(string identifier, params NodeReference[] nodeReferences)
            : this(identifier, (IEnumerable<NodeReference>)nodeReferences)
        {
        }

        public CypherStartBit(string identifier, IEnumerable<NodeReference> nodeReferences)
        {
            this.identifier = identifier;
            lookupType = "node";
            lookupIds = nodeReferences.Select(r => r.Id).ToArray();
        }

        public CypherStartBit(string identifier, params RelationshipReference[] relationshipReferences)
            : this(identifier, (IEnumerable<RelationshipReference>)relationshipReferences)
        {
        }

        public CypherStartBit(string identifier, IEnumerable<RelationshipReference> relationshipReferences)
        {
            this.identifier = identifier;
            lookupType = "relationship";
            lookupIds = relationshipReferences.Select(r => r.Id).ToArray();
        }

        public string ToCypherText(Func<object, string> createParameterCallback)
        {
            var lookupIdParameterNames = lookupIds
                .Select(i => createParameterCallback(i))
                .ToArray();

            var lookupContent = string.Join(", ", lookupIdParameterNames);
            return string.Format("{0}={1}({2})",
                identifier,
                lookupType,
                lookupContent);
        }
    }
}
