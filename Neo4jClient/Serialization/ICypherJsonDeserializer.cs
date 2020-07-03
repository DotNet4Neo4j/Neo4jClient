using System.Collections.Generic;

namespace Neo4jClient.Serialization
{
    public interface ICypherJsonDeserializer<out TResult>
    {
        PartialDeserializationContext CheckForErrorsInTransactionResponse(string content);
        IEnumerable<TResult> Deserialize(string content, bool isHttp);
        IEnumerable<TResult> DeserializeFromTransactionPartialContext(PartialDeserializationContext context, bool isHttp);
    }
}