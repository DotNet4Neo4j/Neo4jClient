using System;

namespace Neo4jClient.Cypher
{
    [Obsolete("Use StartBit instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
    public interface ICypherStartBit
    {
        string ToCypherText(Func<object, string> createParameterCallback);
    }
}
