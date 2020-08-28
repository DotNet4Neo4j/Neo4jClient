using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Serialization;
using Neo4jClient.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient
{
    public interface IGraphClient : ICypherGraphClient
    {
        event OperationCompletedEventHandler OperationCompleted;
        
        /// <summary>
        /// Sets the default database to use. You can still override with the <see cref="ICypherFluentQuery.WithDatabase"/> method on a per query basis.
        /// </summary>
        /// <remarks>The default if this is not set, is <c>"neo4j"</c></remarks>
        string DefaultDatabase { get; set; }

        // ReSharper disable once InconsistentNaming
        CypherCapabilities CypherCapabilities { get; }

        Uri RootEndpoint { get; }

        Version ServerVersion { get; }

        Uri TransactionEndpoint { get; }

        ISerializer Serializer { get; }

        ExecutionConfiguration ExecutionConfiguration { get; }

        bool IsConnected { get; }

        Task ConnectAsync(NeoServerConfiguration configuration = null);

        List<JsonConverter> JsonConverters { get; }
        DefaultContractResolver JsonContractResolver { get; set; }
        Uri GetTransactionEndpoint(string database, bool autoCommit);
        ITransactionalGraphClient Tx { get; }
    }
}
