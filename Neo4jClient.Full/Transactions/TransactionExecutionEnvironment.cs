using System;
using System.Collections.Generic;
using Neo4jClient.Execution;
using Newtonsoft.Json;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Because the resource manager is held in another application domain, the transaction execution environment
    /// has to be serialized to cross app domain boundaries.
    /// </summary>
    internal class TransactionExecutionEnvironment : MarshalByRefObject, ITransactionExecutionEnvironment
    {
        public Uri TransactionBaseEndpoint { get; set; }
        public int TransactionId { get; set; }
        public bool UseJsonStreaming { get; set; }
        public string UserAgent { get; set; }
        public IEnumerable<JsonConverter> JsonConverters { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid ResourceManagerId { get; set; }

        public TransactionExecutionEnvironment(ExecutionConfiguration executionConfiguration)
        {
            UserAgent = executionConfiguration.UserAgent;
            UseJsonStreaming = executionConfiguration.UseJsonStreaming;
            Username = executionConfiguration.Username;
            Password = executionConfiguration.Password;
            JsonConverters = executionConfiguration.JsonConverters;
            ResourceManagerId = executionConfiguration.ResourceManagerId;
        }

    }
}
