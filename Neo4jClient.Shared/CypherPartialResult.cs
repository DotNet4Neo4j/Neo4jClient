using System.Net.Http;
using Neo4jClient.Serialization;

namespace Neo4jClient
{
    /// <summary>
    /// This class is only used to hold partial results on the execution of a cypher query.
    /// Depending on whether we are running inside a transaction, the result string is already
    /// deserialized (because it has to be checked for errors), or we need the full HttpResponseMessage
    /// object.
    /// </summary>
    internal class CypherPartialResult
    {
        public HttpResponseMessage ResponseObject { get; set; }
        public PartialDeserializationContext DeserializationContext { get; set; }
    }
}
