using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient
{
    public interface IGraphClient : ICypherGraphClient
    {
        event OperationCompletedEventHandler OperationCompleted;

        CypherCapabilities CypherCapabilities { get; }

        Version ServerVersion { get; }

        Uri RootEndpoint { get; }

        Uri CypherEndpoint { get; }

        //Uri BoltEndpoint { get; }

        ISerializer Serializer { get; }

        ExecutionConfiguration ExecutionConfiguration { get; }

        bool IsConnected { get; }

        Task ConnectAsync(NeoServerConfiguration configuration = null);

      

        List<JsonConverter> JsonConverters { get; }
        DefaultContractResolver JsonContractResolver { get; set; }
    }
}
