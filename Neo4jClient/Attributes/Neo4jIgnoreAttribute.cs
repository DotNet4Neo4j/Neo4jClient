using System;

namespace Neo4jClient
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class Neo4jIgnoreAttribute : Attribute
    {
    }
}