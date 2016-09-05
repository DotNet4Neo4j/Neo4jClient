using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient.Transactions
{
    internal interface ITransactionExecutionEnvironment
    {
        Uri TransactionBaseEndpoint { get; }
        int TransactionId { get; }
        bool UseJsonStreaming { get; set; }
        string UserAgent { get; set; }
        IEnumerable<JsonConverter> JsonConverters { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        Guid ResourceManagerId { get; set; }
    }
}