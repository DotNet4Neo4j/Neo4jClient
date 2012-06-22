using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    internal class RelationshipApiResponse<TData>
        where TData : class, new()
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }

        [JsonProperty("type")]
        public string TypeKey { get; set; }

        [JsonProperty("data")]
        public TData Data { get; set; }

        public RelationshipInstance<TData> ToRelationshipInstance(IGraphClient client)
        {
            var relationshipId = int.Parse(GetLastPathSegment(Self));
            var startNodeId = int.Parse(GetLastPathSegment(Start));
            var endNodeId = int.Parse(GetLastPathSegment(End));

            return new RelationshipInstance<TData>(
                new RelationshipReference<TData>(relationshipId, client),
                new NodeReference(startNodeId, client),
                new NodeReference(endNodeId, client),
                TypeKey,
                Data);
        }


        public RelationshipReference<TData> ToRelationshipReference(IGraphClient client)
        {
            var relationshipId = int.Parse(GetLastPathSegment(Self));
            return new RelationshipReference<TData>(relationshipId, client);
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
