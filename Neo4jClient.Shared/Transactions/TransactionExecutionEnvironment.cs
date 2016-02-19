using System;
using System.Collections.Generic;
using Neo4jClient.Execution;
using Neo4jClient.Serialization;
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

        public TransactionExecutionEnvironment(ExecutionConfiguration executionConfiguration)
        {
            UserAgent = executionConfiguration.UserAgent;
            UseJsonStreaming = executionConfiguration.UseJsonStreaming;
            Username = executionConfiguration.Username;
            Password = executionConfiguration.Password;
            JsonConverters = executionConfiguration.JsonConverters;
        }

    }

    internal interface ITransactionExecutionEnvironment
    {
        Uri TransactionBaseEndpoint { get; }
        int TransactionId { get; }
        bool UseJsonStreaming { get; set; }
        string UserAgent { get; set; }
        IEnumerable<JsonConverter> JsonConverters { get; set; }
        string Username { get; set; }
        string Password { get; set; }
    }
}
