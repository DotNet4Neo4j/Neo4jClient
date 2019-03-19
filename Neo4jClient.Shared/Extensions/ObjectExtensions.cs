using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.Extensions
{
    public static class ObjectExtensions
    {

        public static bool In<T>(this T obj, IEnumerable<T> enumerable)
        {
            return enumerable.Contains(obj);
        }

        public static bool NotIn<T>(this T obj, IEnumerable<T> enumerable)
        {
            return !obj.In(enumerable);
        }
    }
}