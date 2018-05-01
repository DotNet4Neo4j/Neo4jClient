using System;
using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;

namespace Neo4jClient.Serialization.BoltDriver
{
    /// <summary>
    /// Interface for the Bolt driver based deserializer
    /// </summary>
    public interface IDriverDeserializer<out TResult>
    {
        TResult Deserialize(IRecord record);
    }
}
