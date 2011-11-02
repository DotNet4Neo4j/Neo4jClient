using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    class RootApiResponse
    {
        public string Batch { get; set; }
        public string Node { get; set; }
        public string NodeIndex { get; set; }
        public string RelationshipIndex { get; set; }
        public string ReferenceNode { get; set; }
        public string ExtensionsInfo { get; set; }
        public ExtensionsApiResponse Extensions { get; set; }

        [JsonProperty("neo4j_version")]
        public string VersionString { get; set; }

        [JsonIgnore]
        public Version Version
        {
            get
            {
                if (string.IsNullOrEmpty(VersionString))
                    return new Version();

                var numericalVersionString = Regex.Replace(
                    VersionString,
                    @"(?<major>\d*)[.](?<minor>\d*)-(?<revision>\d*).*",
                    "${major}.${minor}.${revision}");

                return new Version(numericalVersionString);
            }
        }
    }
}
