using System;
using System.IO;
using System.Linq;

namespace Neo4jClient.ApiModels
{
    internal class RelationshipApiResponse
    {
        public string Self { get; set; }
        public string Start { get; set; }
        public string End { get; set; }

        public RelationshipInstance ToRelationshipInstance(IGraphClient client)
        {
            var relationshipId = int.Parse(GetLastPathSegment(Self));
            var startNodeId = int.Parse(GetLastPathSegment(Start));
            var endNodeId = int.Parse(GetLastPathSegment(End));

            return new RelationshipInstance(
                new RelationshipReference(relationshipId),
                new NodeReference(startNodeId, client),
                new NodeReference(endNodeId, client));
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
