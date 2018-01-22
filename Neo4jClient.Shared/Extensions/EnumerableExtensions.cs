using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool ContentsEqual<T>(this IReadOnlyCollection<T> collection1, IReadOnlyCollection<T> collection2)
        {
            if (collection1 == null && collection2 == null)
                return true;

            if (collection1 == null || collection2 == null)
                return false;

            return collection1.Count == collection2.Count && collection1.All(collection2.Contains);
        }
    }
}