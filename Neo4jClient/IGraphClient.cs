using System;
using System.Collections.Generic;

namespace Neo4jClient
{
    public interface IGraphClient
    {
        RootNode RootNode { get; }

        NodeReference<TNode> Create<TNode>(TNode node, IEnumerable<IRelationshipAllowingParticipantNode<TNode>> relationships, IEnumerable<IndexEntry> indexEntries)
            where TNode : class;

        Node<TNode> Get<TNode>(NodeReference reference);

        Node<TNode> Get<TNode>(NodeReference<TNode> reference);

        void Update<TNode>(NodeReference<TNode> nodeReference, Action<TNode> updateCallback);

        void Update<TNode>(
            NodeReference<TNode> nodeReference,
            Action<TNode> updateCallback,
            Func<TNode, IEnumerable<IndexEntry>> indexEntriesCallback);

        void Delete(NodeReference reference, DeleteMode mode);

        void CreateRelationship<TSourceNode, TRelationship>(NodeReference<TSourceNode> sourceNodeReference, TRelationship relationship)
            where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSourceNode>;

        void DeleteRelationship(RelationshipReference reference);

        string ExecuteScalarGremlin(string query, IDictionary<string, object> parameters);

        IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query, IDictionary<string, object> parameters);

        IEnumerable<RelationshipInstance> ExecuteGetAllRelationshipsGremlin(string query, IDictionary<string, object> parameters);

        Dictionary<string, IndexMetaData> GetIndexes(IndexFor indexFor);

        bool CheckIndexExists(string indexName, IndexFor indexFor);

        void CreateIndex(string indexName, IndexConfiguration config, IndexFor indexFor);

        void ReIndex(NodeReference node, IEnumerable<IndexEntry> indexEntries);

        void DeleteIndex(string indexName, IndexFor indexFor);

        IEnumerable<Node<TNode>> QueryIndex<TNode>(string indexName, IndexFor indexFor, string query );
    }
}
