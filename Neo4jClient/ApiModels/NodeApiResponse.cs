using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    internal class NodeApiResponse<TNode>
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("data")]
        public TNode Data { get; set; }

        public Node<TNode> ToNode(IGraphClient client)
        {
            var nodeId = long.Parse(GetLastPathSegment(Self));
            return new Node<TNode>(Data, new NodeReference<TNode>(nodeId, client));
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
