using System.Collections.Generic;
using System.Linq;
using Neo4jClient.ApiModels;

namespace Neo4jClient
{
    public class Utilities
    {
        public static IEnumerable<FieldChange> GetDifferencesBetweenDictionaries(IDictionary<string, string> originalValues, IDictionary<string, string> newValues)
        {
            var changes = new List<FieldChange>();

            changes.AddRange(originalValues
                .Keys
                .Where(k => newValues.Keys.All(n => n != k))
                .Select(k => new FieldChange
                {
                    FieldName = k,
                    OldValue = originalValues[k],
                    NewValue = ""
                }));

            changes.AddRange(newValues
                .Keys
                .Where(k => originalValues.Keys.All(n => n != k))
                .Select(k => new FieldChange
                {
                    FieldName = k,
                    OldValue = "",
                    NewValue = newValues[k],
                }));

            changes.AddRange(newValues
                .Keys
                .Where(k => originalValues.Keys.Any(n => n == k) && originalValues[k] != newValues[k])
                .Select(k =>new FieldChange
                        {
                            FieldName = k,
                            OldValue = originalValues[k],
                            NewValue = newValues[k],
                        }
                ));

            return changes;
        }
    }
}
