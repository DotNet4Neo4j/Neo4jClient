using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Neo4jClient.Gremlin;

namespace Neo4jClient
{
    public interface IGraphClient
    {
        RootNode RootNode { get; }

        NodeReference<TNode> Create<TNode>(TNode node, IEnumerable<IRelationshipAllowingParticipantNode<TNode>> relationships, IEnumerable<IndexEntry> indexEntries)
            where TNode : class;

        Node<TNode> Get<TNode>(NodeReference reference);

        Task<Node<TNode>> GetAsync<TNode>(NodeReference reference);

        Node<TNode> Get<TNode>(NodeReference<TNode> reference);

        void Update<TNode>(NodeReference<TNode> nodeReference,
                           Action<TNode> updateCallback,
                           Func<TNode, IEnumerable<IndexEntry>> indexEntriesCallback = null,
                           Action<IEnumerable<FieldChange>> changeCallback = null);

        void Update<TRelationshipData>(RelationshipReference<TRelationshipData> relationshipReference, Action<TRelationshipData> updateCallback)
            where TRelationshipData : class, new();

        void Delete(NodeReference reference, DeleteMode mode);

        RelationshipReference CreateRelationship<TSourceNode, TRelationship>(NodeReference<TSourceNode> sourceNodeReference, TRelationship relationship)
            where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSourceNode>;

        void DeleteRelationship(RelationshipReference reference);

        string ExecuteScalarGremlin(string query, IDictionary<string, object> parameters);

        IEnumerable<TResult> ExecuteGetAllProjectionsGremlin<TResult>(IGremlinQuery query) where TResult : new();

        IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(IGremlinQuery query);

        IEnumerable<RelationshipInstance> ExecuteGetAllRelationshipsGremlin(string query, IDictionary<string, object> parameters);

        IEnumerable<RelationshipInstance<TData>> ExecuteGetAllRelationshipsGremlin<TData>(string query, IDictionary<string, object> parameters) where TData : class, new();

        Dictionary<string, IndexMetaData> GetIndexes(IndexFor indexFor);

        bool CheckIndexExists(string indexName, IndexFor indexFor);

        void CreateIndex(string indexName, IndexConfiguration config, IndexFor indexFor);

        void ReIndex(NodeReference node, IEnumerable<IndexEntry> indexEntries);

        void DeleteIndex(string indexName, IndexFor indexFor);

        void DeleteIndexEntries(string indexName, long nodeId);

        IEnumerable<Node<TNode>> QueryIndex<TNode>(string indexName, IndexFor indexFor, string query );

        void ShutdownServer();

        event OperationCompletedEventHandler OperationCompleted;

        ICypherFluentQueryPreStart Cypher { get; }

        IGremlinClient Gremlin { get; }

        Version ServerVersion { get; }
    }
}
