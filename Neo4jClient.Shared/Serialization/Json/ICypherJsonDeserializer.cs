using System.Collections.Generic;

namespace Neo4jClient.Serialization.Json
{
    public interface ICypherJsonDeserializer<out TResult>
    {
        PartialDeserializationContext CheckForErrorsInTransactionResponse(string content);
        IEnumerable<TResult> Deserialize(string content);
        IEnumerable<TResult> DeserializeFromTransactionPartialContext(PartialDeserializationContext context);
    }
}