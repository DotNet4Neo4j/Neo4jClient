using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    public class DataRow<TCol1>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }
    }

    public class DataRow<TCol1, TCol2>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }
    }

    public class DataRow<TCol1, TCol2, TCol3>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }

        [JsonProperty("column3")]
        public TCol3 Column3 { get; set; }
    }

    public class DataRow<TCol1, TCol2, TCol3, TCol4>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }

        [JsonProperty("column3")]
        public TCol3 Column3 { get; set; }

        [JsonProperty("column4")]
        public TCol4 Column4 { get; set; }
    }

    public class DataRow<TCol1, TCol2, TCol3, TCol4, TCol5>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }

        [JsonProperty("column3")]
        public TCol3 Column3 { get; set; }

        [JsonProperty("column4")]
        public TCol4 Column4 { get; set; }

        [JsonProperty("column5")]
        public TCol5 Column5 { get; set; }
    }

    public class DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }

        [JsonProperty("column3")]
        public TCol3 Column3 { get; set; }

        [JsonProperty("column4")]
        public TCol4 Column4 { get; set; }

        [JsonProperty("column5")]
        public TCol5 Column5 { get; set; }

        [JsonProperty("column6")]
        public TCol6 Column6 { get; set; }
    }

    public class DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }

        [JsonProperty("column3")]
        public TCol3 Column3 { get; set; }

        [JsonProperty("column4")]
        public TCol4 Column4 { get; set; }

        [JsonProperty("column5")]
        public TCol5 Column5 { get; set; }

        [JsonProperty("column6")]
        public TCol6 Column6 { get; set; }

        [JsonProperty("column7")]
        public TCol7 Column7 { get; set; }
    }

    public class DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }

        [JsonProperty("column3")]
        public TCol3 Column3 { get; set; }

        [JsonProperty("column4")]
        public TCol4 Column4 { get; set; }

        [JsonProperty("column5")]
        public TCol5 Column5 { get; set; }

        [JsonProperty("column6")]
        public TCol6 Column6 { get; set; }

        [JsonProperty("column7")]
        public TCol7 Column7 { get; set; }

        [JsonProperty("column8")]
        public TCol8 Column8 { get; set; }
    }

    public class DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8, TCol9>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }

        [JsonProperty("column3")]
        public TCol3 Column3 { get; set; }

        [JsonProperty("column4")]
        public TCol4 Column4 { get; set; }

        [JsonProperty("column5")]
        public TCol5 Column5 { get; set; }

        [JsonProperty("column6")]
        public TCol6 Column6 { get; set; }

        [JsonProperty("column7")]
        public TCol7 Column7 { get; set; }

        [JsonProperty("column8")]
        public TCol8 Column8 { get; set; }

        [JsonProperty("column9")]
        public TCol9 Column9 { get; set; }
    }

    public class DataRow<TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8, TCol9, TCol10>
    {
        [JsonProperty("column1")]
        public TCol1 Column1 { get; set; }

        [JsonProperty("column2")]
        public TCol2 Column2 { get; set; }

        [JsonProperty("column3")]
        public TCol3 Column3 { get; set; }

        [JsonProperty("column4")]
        public TCol4 Column4 { get; set; }

        [JsonProperty("column5")]
        public TCol5 Column5 { get; set; }

        [JsonProperty("column6")]
        public TCol6 Column6 { get; set; }

        [JsonProperty("column7")]
        public TCol7 Column7 { get; set; }

        [JsonProperty("column8")]
        public TCol8 Column8 { get; set; }

        [JsonProperty("column9")]
        public TCol9 Column9 { get; set; }

        [JsonProperty("column10")]
        public TCol10 Column10 { get; set; }
    }
}