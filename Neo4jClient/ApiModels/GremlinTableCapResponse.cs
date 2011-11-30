using System.Collections.Generic;
using System.Linq;
using Neo4jClient.Converters;

namespace Neo4jClient.ApiModels
{
    internal class GremlinTableCapResponse
    {
        public List<string> Columns { get; set; }
        public List<List<string>> Data { get; set; }

        internal static IEnumerable<TResult> TransferResponseToResult<TResult>(List<List<GremlinTableCapResponse>> response) where TResult : new()
        {
            var type = typeof(TResult);
            var properties = type.GetProperties();

            var gremlinTableCapResponses = response.SingleOrDefault();
            if (gremlinTableCapResponses != null)
            {
                var gremlinTableCapResponse = gremlinTableCapResponses[0]; //ToDo Added support for multiple table results.

                if (gremlinTableCapResponse == null || gremlinTableCapResponse.Columns == null || gremlinTableCapResponse.Data == null)
                    yield break;

                var columns = gremlinTableCapResponse.Columns;
                var dataRows = gremlinTableCapResponse.Data.Select(dr => dr);

                foreach (var t in dataRows)
                {
                    var result = new TResult();
                    foreach (var prop in properties.Where(p => columns.Any(c => c.ToLowerInvariant() == p.Name.ToLowerInvariant())))
                    {
                        var columnIndex = columns.IndexOf(prop.Name);
                        if (columnIndex == -1) continue;
                        var columnData = t;
                        var columnCellData = columnData[columnIndex];
                        result.ConvertAndSetValue(columnCellData, prop);
                    }
                    yield return result;
                }
            }
        }

        
    }
}
