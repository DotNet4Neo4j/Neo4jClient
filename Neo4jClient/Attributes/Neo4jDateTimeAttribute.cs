using System;

namespace Neo4jClient
{
    /// <summary>
    /// If this is used on a <see cref="System.DateTime"/> property - it will be serialized as a Neo4j Date object rather than a string
    /// </summary>
    public class Neo4jDateTimeAttribute : Attribute
    {
    }
}
