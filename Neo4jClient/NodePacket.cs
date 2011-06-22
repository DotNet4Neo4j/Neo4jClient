using System;
using System.IO;
using System.Linq;

namespace Neo4jClient
{
    internal class NodePacket<TNode>
    {
        public string Self { get; set; }
        public TNode Data { get; set; }

        public Node<TNode> ToNode()
        {
            var nodeId = int.Parse(GetLastPathSegment(Self));
            return new Node<TNode>(Data, new NodeReference<TNode>(nodeId));
        }

        static string GetLastPathSegment(string uri)
        {
            var path = new Uri(uri).AbsolutePath;
            return path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();
        }
    }
}