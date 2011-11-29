using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.ApiModels
{
    internal class GremlinTableCapResponse
    {
        public string[] Columns { get; set; }
        public string[][] Data { get; set; }

        internal static IEnumerable<TResult> TransferResponseToResult<TResult>(IList<GremlinTableCapResponse[]> response) where TResult : new()
        {
            var type = typeof(TResult);
            var properties = type.GetProperties();

            var gremlinTableCapResponses = response.SingleOrDefault();
            if (gremlinTableCapResponses != null)
            {
                var gremlinTableCapResponse = gremlinTableCapResponses[0]; //ToDo Added support for multiple table results.

                if (gremlinTableCapResponse == null || gremlinTableCapResponse.Columns == null || gremlinTableCapResponse.Data == null)
                    yield break;

                var columns = gremlinTableCapResponse.Columns.ToList();
                var dataRows = gremlinTableCapResponse.Data.Select(dr => dr).ToArray();

                foreach (var t in dataRows)
                {
                    var result = new TResult();
                    foreach (
                        var prop in
                            properties.Where(p => columns.Any(c => c.ToLowerInvariant() == p.Name.ToLowerInvariant())))
                    {
                        var columnIndex = columns.IndexOf(prop.Name.ToLowerInvariant());
                        if (columnIndex == -1) continue;
                        var columnData = t;
                        prop.SetValue(result, columnData[columnIndex], null);
                    }
                    yield return result;
                }
            }
        }
    }
}
