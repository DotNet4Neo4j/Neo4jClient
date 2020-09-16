using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Cypher
{
    public class QueryStats
    {
        [JsonProperty("contains_updates")] public bool ContainsUpdates { get; set; }
        [JsonProperty("nodes_created")] public int NodesCreated { get; set; }
        [JsonProperty("nodes_deleted")] public int NodesDeleted { get; set; }
        [JsonProperty("properties_set")] public int PropertiesSet { get; set; }
        [JsonProperty("relationships_created")] public int RelationshipsCreated { get; set; }
        [JsonProperty("relationship_deleted")] public int RelationshipsDeleted { get; set; }
        [JsonProperty("labels_added")] public int LabelsAdded { get; set; }
        [JsonProperty("labels_removed")] public int LabelsRemoved { get; set; }
        [JsonProperty("indexes_added")] public int IndexesAdded { get; set; }
        [JsonProperty("indexes_removed")] public int IndexesRemoved { get; set; }
        [JsonProperty("constraints_added")] public int ConstraintsAdded { get; set; }
        [JsonProperty("constraints_removed")] public int ConstraintsRemoved { get; set; }
        [JsonProperty("contains_system_updates")] public bool ContainsSystemUpdates { get; set; }
        [JsonProperty("system_updates")] public int SystemUpdates { get; set; }

        public QueryStats(){}
        public QueryStats(Neo4j.Driver.ICounters counters)
        {
            ContainsUpdates = counters.ContainsUpdates;
            NodesCreated = counters.NodesCreated;
            NodesDeleted = counters.NodesDeleted;
            PropertiesSet = counters.PropertiesSet;
            RelationshipsCreated = counters.RelationshipsCreated;
            RelationshipsDeleted = counters.RelationshipsDeleted;
            LabelsAdded = counters.LabelsAdded;
            LabelsRemoved = counters.LabelsRemoved;
            IndexesAdded = counters.IndexesAdded;
            IndexesRemoved = counters.IndexesRemoved;
            ConstraintsAdded = counters.ConstraintsAdded;
            ConstraintsRemoved = counters.ConstraintsRemoved;
            ContainsSystemUpdates = counters.ContainsSystemUpdates;
            SystemUpdates = counters.SystemUpdates;
        }
    }
}