using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Neo4jClient
{
    public interface IGraphClient
    {
        NodeReference<TNode> Create<TNode>(TNode node, params IRelationshipAllowingParticipantNode<TNode>[] relationships) where TNode : class;
        Node<TNode> Get<TNode>(NodeReference reference);
        Node<TNode> Get<TNode>(NodeReference<TNode> reference);
        void Update<TNode>(NodeReference<TNode> nodeReference, Action<TNode> updateCallback);
        void Delete(NodeReference reference, DeleteMode mode);
        string ExecuteScalarGremlin(string query, NameValueCollection queryParameters);
        IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(string query, NameValueCollection queryParameters);
    }
}