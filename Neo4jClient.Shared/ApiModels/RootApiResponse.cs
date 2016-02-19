using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    class RootApiResponse
    {
        [JsonProperty("transaction")]
        public string Transaction { get; set; }

        [JsonProperty("cypher")]
        public string Cypher { get; set; }

        [JsonProperty("batch")]
        public string Batch { get; set; }

        [JsonProperty("node")]
        public string Node { get; set; }

        [JsonProperty("relationship")]
        public string Relationship { get; set; }
        
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

        /// <summary>
        /// Returns a structured representation of the Neo4j server version, but only with partial data.
        /// The version type (milestone, preview, release candidate, stable) is not taken in to account,
        /// so both 1.9.M01, 1.9.RC1 and 1.9.1 will all return 1.9.0.1.
        /// </summary>
        [JsonIgnore]
        public Version Version
        {
            get
            {
                if (string.IsNullOrEmpty(neo4j_version))
                    return new Version();

                var numericalVersionString = Regex.Replace(
                    neo4j_version,
                    @"(?<major>\d*)[.](?<minor>\d*)[.]?(M(?<build>\d*)|RC(?<build>\d*)?).*",
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
