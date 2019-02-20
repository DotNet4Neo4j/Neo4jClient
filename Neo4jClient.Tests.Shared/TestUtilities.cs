using System.Collections.Generic;

namespace Neo4jClient.Tests.Shared
{
    public static class TestUtilities
    {
        public static bool IsEqualTo<TKey, TValue>(this IDictionary<TKey, TValue> d1, IDictionary<TKey, TValue> d2)
        {
            if (d1 == null && d2 == null)
                return true;
            if (d1 == null || d2 == null)
                return false;

            if (d1.Count != d2.Count)
                return false;

            foreach (var d1Key in d1.Keys)
            {
                if (!d2.ContainsKey(d1Key))
                    return false;

                var v1 = d1[d1Key];
                var v2 = d2[d1Key];
                if (v1.GetType() == typeof(Dictionary<TKey, TValue>))
                    return IsEqualTo((IDictionary<TKey, TValue>)v1, (IDictionary<TKey, TValue>)v2);

                if (!d1[d1Key].Equals(d2[d1Key]))
                    return false;
            }
            return true;
        }
    }
}
