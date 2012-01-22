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

        public static IEnumerable<TResult> TransferResponseToResult<TResult, TColumn1, TColumn2, TColumn3>(CypherApiTableResponse<TColumn1, TColumn2, TColumn3> response) where TResult : new()
        {
            var type = typeof(TResult);
            var properties = type.GetProperties();

            if (response != null)
            {

                if (response.Columns == null || response.Data == null)
                    yield break;

                var columns = response.Columns;
                var dataRows = response.Data;

                var result = new TResult();
                    foreach (var prop in properties.Where(p => columns.Any(c => c.ToLowerInvariant() == p.Name.ToLowerInvariant())))
                    {
                        var columnIndex = columns.IndexOf(prop.Name.ToLowerInvariant()); //ToDo ensure cypher columns are always lower case when parsed in CypherQuery
                        if (columnIndex == -1) continue;
                        prop.SetValue(result, dataRows, null);
                    }
                    yield return result;
            }
        }
    }
}
