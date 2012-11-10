using System;

namespace Neo4jClient.Cypher
{
    public interface ICypherStartBit
    {
        string ToCypherText(Func<object, string> createParameterCallback);
    }
}
