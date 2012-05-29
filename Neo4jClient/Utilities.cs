using System;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient.ApiModels;

namespace Neo4jClient
{
    public class Utilities
    {
        public static IEnumerable<FieldChange> GetDifferencesBetweenDictionaries(IDictionary<string, string> originalValues, IDictionary<string, string> newValues)
        {
            return originalValues
                .Keys
                .Select(k => new FieldChange
                    {
                        FieldName = k,
                        OldValue = originalValues[k],
                        NewValue = newValues[k]
                    })
                .Where(c => !c.OldValue.Equals(c.NewValue, StringComparison.Ordinal));
        }
    }
}
