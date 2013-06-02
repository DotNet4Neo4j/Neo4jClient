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

        /// <summary>
        /// Creates a node, relationships and index entries all in a single HTTP call (which also means a single transaction).
        /// </summary>
        NodeReference<TNode> Create<TNode>(TNode node, IEnumerable<IRelationshipAllowingParticipantNode<TNode>> relationships, IEnumerable<IndexEntry> indexEntries)
            where TNode : class;

        Node<TNode> Get<TNode>(NodeReference reference);

        Task<Node<TNode>> GetAsync<TNode>(NodeReference reference);

        Node<TNode> Get<TNode>(NodeReference<TNode> reference);

        RelationshipInstance<TData> Get<TData>(RelationshipReference<TData> reference) where TData : class, new();

        RelationshipInstance<TData> Get<TData>(RelationshipReference reference) where TData : class, new();

        Task<RelationshipInstance<TData>> GetAsync<TData>(RelationshipReference reference) where TData : class, new();

        /// <summary>
        /// Retrieves the specified node, gives you an opportunity to mutate it in the callback, then persists the final object back to Neo4j. Results in two calls over the wire: one to retrieve, one to set.
        /// </summary>
        /// <typeparam name="TNode">POCO type that represents the structure of the node's data</typeparam>
        /// <param name="nodeReference">The node to retrieve and update</param>
        /// <param name="replacementData">Data to replace the node with</param>
        /// <param name="indexEntries">New index entries that should also be persisted</param>
        void Update<TNode>(NodeReference<TNode> nodeReference,
                           TNode replacementData,
                           IEnumerable<IndexEntry> indexEntries = null);

        /// <summary>
        /// Retrieves the specified node, gives you an opportunity to mutate it in the callback, then persists the final object back to Neo4j. Results in two calls over the wire: one to retrieve, one to set.
        /// </summary>
        /// <typeparam name="TNode">POCO type that represents the structure of the node's data</typeparam>
        /// <param name="nodeReference">The node to retrieve and update</param>
        /// <param name="updateCallback">A callback to mutate the values between retrieval and persistence</param>
        /// <param name="indexEntriesCallback">A callback to return new index entries that should also be persisted</param>
        /// <param name="changeCallback">A callback to respond to the resulting property changes</param>
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

        void ReIndex(RelationshipReference relationship, IEnumerable<IndexEntry> indexEntries);

        void DeleteIndex(string indexName, IndexFor indexFor);

        /// <summary>
        /// Delete all index entries for specified node
        /// </summary>
        void DeleteIndexEntries(string indexName, NodeReference relationshipReference);

        /// <summary>
        /// Delete all index entries for specified relationship
        /// </summary>
        void DeleteIndexEntries(string indexName, RelationshipReference relationshipReference);

        [Obsolete("There are encoding issues with this method. You should use the newer Cypher aproach instead. See https://bitbucket.org/Readify/neo4jclient/issue/54/spaces-in-search-text-while-searching-for for an explanation of the problem, and https://bitbucket.org/Readify/neo4jclient/wiki/cypher for documentation about doing index queries with Cypher.")]
        IEnumerable<Node<TNode>> QueryIndex<TNode>(string indexName, IndexFor indexFor, string query );

        IEnumerable<Node<TNode>> LookupIndex<TNode>(string exactIndexName, IndexFor indexFor, string indexKey, long id);
        IEnumerable<Node<TNode>> LookupIndex<TNode>(string exactIndexName, IndexFor indexFor, string indexKey, int id);

        void ShutdownServer();

        event OperationCompletedEventHandler OperationCompleted;

        ICypherFluentQuery Cypher { get; }

        IGremlinClient Gremlin { get; }

        Version ServerVersion { get; }
    }
}
