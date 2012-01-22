using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Cypher
{
    public class CypherApiTableResponse<TCol1>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1>>> Data { get; set; }
    }

    public class CypherApiTableResponse<TCol1, TCol2>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2>>> Data { get; set; }
    }

    public class CypherApiTableResponse<TCol1, TCol2, TCol3>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2, TCol3>>> Data { get; set; }
    }

    public class CypherApiTableResponse<TCol1, TCol2, TCol3, TCol4>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2, TCol3, TCol4>>> Data { get; set; }
    }

    public class CypherApiTableResponse<TCol1, TCol2, TCol3, TCol4, TCol5>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2, TCol3, TCol4, TCol5>>> Data { get; set; }
    }

    public class CypherApiTableResponse<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6>>> Data { get; set; }
    }

    public class CypherApiTableResponse<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7>>> Data { get; set; }
    }

    public class CypherApiTableResponse<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8>>> Data { get; set; }
    }

    public class CypherApiTableResponse<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8, TCol9>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8, TCol9>>> Data { get; set; }
    }

    public class CypherApiTableResponse<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8, TCol9, TCol10>
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("data")]
        public List<List<DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8, TCol9, TCol10>>> Data { get; set; }
    }
}
