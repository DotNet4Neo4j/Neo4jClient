﻿using System;
using System.Collections.Generic;
using Neo4jClient.Cypher;

namespace Neo4jClient
{
    public class Node<TNode> : IHasNodeReference, IAttachedReference
    {
        readonly TNode data;
        readonly NodeReference<TNode> reference;

        public Node(TNode data, NodeReference<TNode> reference)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            this.data = data;
            this.reference = reference;
        }

        public Node(TNode data, long id, IGraphClient client)
        {
            this.data = data;
            reference = new NodeReference<TNode>(id, client);
        }

        public NodeReference<TNode> Reference
        {
            get { return reference; }
        }

        NodeReference IHasNodeReference.Reference
        {
            get { return reference; }
        }

        public TNode Data
        {
            get { return data; }
        }

        public ICypherFluentQuery StartCypher(string identity)
        {
            var client = ((IAttachedReference) this).Client;
            var query = new CypherFluentQuery(client)
                .Start(identity, Reference);
            return query;
        }

        IGraphClient IAttachedReference.Client
        {
            get { return ((IAttachedReference)reference).Client; }
        }

        public static bool operator ==(Node<TNode> lhs, Node<TNode> rhs)
        {
            if (ReferenceEquals(lhs, null) && ReferenceEquals(rhs, null))
                return true;

            return !ReferenceEquals(lhs, null) && lhs.Equals(rhs);
        }

        public static bool operator !=(Node<TNode> lhs, Node<TNode> rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Node<TNode>;
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Reference == Reference;
        }

        public override int GetHashCode()
        {
            return Reference.Id.GetHashCode();
        }
    }
}
