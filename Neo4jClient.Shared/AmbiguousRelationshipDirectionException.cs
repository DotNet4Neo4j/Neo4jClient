using System;

namespace Neo4jClient
{
    public class AmbiguousRelationshipDirectionException : Exception
    {
        public AmbiguousRelationshipDirectionException()
            : base("The direction of the relationship is ambiguous as both node participants are valid as either source, or target nodes. Specify the relationship direction explicitly or ammend the relationship definition to be more restrictive around which node types are allowed as sources and targets.")
        {}
    }
}
