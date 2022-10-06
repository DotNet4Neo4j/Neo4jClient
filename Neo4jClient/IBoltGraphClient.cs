using System;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Serialization;

namespace Neo4jClient
{
    public interface IBoltGraphClient : ICypherGraphClient
    {
        event OperationCompletedEventHandler OperationCompleted;

        CypherCapabilities CypherCapabilities { get; }
        
        Version ServerVersion { get; }

        ISerializer Serializer { get; }

        ExecutionConfiguration ExecutionConfiguration { get; }

        bool IsConnected { get; }

        Task ConnectAsync(NeoServerConfiguration configuration = null);

        /// <summary>
        /// Indicates if client should use the native driver date time types, instead of serialized datetime strings.
        /// </summary>
        bool UseDriverDateTypes { get; }
    }
}