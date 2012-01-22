using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Cypher
{
    public class DataRow<TCol1, TCol2, TCol3>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }

        [JsonProperty("column3")]
        public TCol3 Column3 { get; set; }
    }

    internal class CypherApiTableResponse<TCol1, TCol2, TCol3>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2, TCol3>>> Data { get; set; }
    }
}
