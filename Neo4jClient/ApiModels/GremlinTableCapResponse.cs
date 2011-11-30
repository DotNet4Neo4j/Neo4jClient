using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
                        try
                        {
                            var validType = prop.PropertyType;
                            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                            {
                                var nullableConverter = new NullableConverter(prop.PropertyType);
                                validType = nullableConverter.UnderlyingType;
                            }

                            if (columnCellData == null || string.IsNullOrEmpty(columnCellData))
                            {
                                prop.SetValue(result, null, null);
                                continue;
                            }

                            object convertedData;
                            if (validType.IsEnum)
                            {
                                convertedData = Enum.Parse(validType, columnCellData, false);
                            }
                            else if (validType == typeof(DateTimeOffset))
                            {
                                convertedData = DateTimeOffset.Parse(columnCellData);
                            }
                            else
                            {
                                convertedData = Convert.ChangeType(columnCellData, validType);
                            }
                            prop.SetValue(result, convertedData, null);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("Could not set property {0} to value {1} for type {2}\n {3}", prop.Name, columnCellData, result.GetType().FullName, ex));
                        }
                    }
                    yield return result;
                }
            }
        }
    }
}
