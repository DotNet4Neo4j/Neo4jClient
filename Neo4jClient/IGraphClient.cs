using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Neo4jClient
{
    public interface IGraphClient
    {
        RootNode RootNode { get; }
        NodeReference<TNode> Create<TNode>(TNode node, params IRelationshipAllowingParticipantNode<TNode>[] relationships) where TNode : class;

        void CreateRelationship<TSourceNode, TRelationship>(
            NodeReference<TSourceNode> sourceNodeReference,
            TRelationship relationship)
            where TRelationship :
                Relationship,
                IRelationshipAllowingSourceNode<TSourceNode>;

        Node<TNode> Get<TNode>(NodeReference reference);
        Node<TNode> Get<TNode>(NodeReference<TNode> reference);
        void Update<TNode>(NodeReference<TNode> nodeReference, Action<TNode> updateCallback);
        void Delete(NodeReference reference, DeleteMode mode);
        string ExecuteScalarGremlin(string query);
        string ExecuteScalarGremlin(string query, NameValueCollection queryParameters);
        IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query);
        IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query, NameValueCollection queryParameters);
    }
}