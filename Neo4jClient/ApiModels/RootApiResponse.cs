using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    class RootApiResponse
    {
        [JsonProperty("cypher")]
        public string Cypher { get; set; }

        [JsonProperty("batch")]
        public string Batch { get; set; }

        [JsonProperty("node")]
        public string Node { get; set; }

        [JsonProperty("node_index")]
        public string NodeIndex { get; set; }

        [JsonProperty("relationship_index")]
        public string RelationshipIndex { get; set; }

        [JsonProperty("reference_node")]
        public string ReferenceNode { get; set; }

        [JsonProperty("extensions_info")]
        public string ExtensionsInfo { get; set; }

        [JsonProperty("extensions")]
        public ExtensionsApiResponse Extensions { get; set; }

        public string neo4j_version { get; set; }

        [JsonIgnore]
        public Version Version
        {
            get
            {
                if (string.IsNullOrEmpty(neo4j_version))
                    return new Version();

                switch (neo4j_version)
                {
                    case "1.8.RC1": return new Version(1,8,0,8);
                }

                var numericalVersionString = Regex.Replace(
                    neo4j_version,
                    @"(?<major>\d*)[.](?<minor>\d*)[.]?M(?<build>\d*).*",
                    "${major}.${minor}.0.${build}");

                numericalVersionString = Regex.Replace(
                    numericalVersionString,
                    @"(?<major>\d*)[.](?<minor>\d*)-.*",
                    "${major}.${minor}");

                Version result;
                var parsed = Version.TryParse(numericalVersionString, out result);

                return parsed ? result : new Version(0, 0);
            }
        }
    }
}
