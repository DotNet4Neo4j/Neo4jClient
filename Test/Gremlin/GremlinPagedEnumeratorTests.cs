using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class GremlinPagedEnumeratorTests
    {
        [Test]
        public void ShouldNotLoadAnythingUntilEnumerated()
        {
            var loadCount = 0;
            Func<string, IDictionary<string, object>, IEnumerable<object>> loadCallback =
                (queryText, queryParams) => { loadCount++; return new object[0]; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } });
            
            new GremlinPagedEnumerator<object>(loadCallback, baseQuery);

            Assert.AreEqual(0, loadCount);
        }

        [Test]
        public void ShouldLoadFirstPageOfResultsWithFirstEnumeration()
        {
            var loadedQueries = new List<string>();
            Func<string, IDictionary<string, object>, IEnumerable<object>> loadCallback =
                (queryText, queryParams) => { loadedQueries.Add(queryText); return new object[0]; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } });

            var enumerator = new GremlinPagedEnumerator<object>(loadCallback, baseQuery);
            enumerator.MoveNext();

            CollectionAssert.AreEqual(
                new[] { "g.v(p0).outV.drop(p1).take(p2)" },
                loadedQueries
            );
        }

        [Test]
        public void ShouldEnumerateOverFirstPageOfResults()
        {
            var results = Enumerable.Range(0, 100).ToArray();

            var loadedQueries = new List<string>();
            Func<string, IDictionary<string, object>, IEnumerable<int>> loadCallback =
                (queryText, queryParams) => { loadedQueries.Add(queryText); return results; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } });

            var enumerator = new GremlinPagedEnumerator<int>(loadCallback, baseQuery);
            for (var i = 0; i < 100; i++)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(results[i], enumerator.Current);
            }
        }

        [Test]
        public void MoveNextShouldReturnFalseOnFirstCallIfThereAreNoResults()
        {
            var results = new int[0];

            var loadedQueries = new List<string>();
            Func<string, IDictionary<string, object>, IEnumerable<int>> loadCallback =
                (queryText, queryParams) => { loadedQueries.Add(queryText); return results; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } });

            var enumerator = new GremlinPagedEnumerator<int>(loadCallback, baseQuery);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void ShouldLoadSecondPageWhenCallingMoveNextAfterLastRecordOfFirstPage()
        {
            var results = Enumerable.Range(0, 100).ToArray();

            var loadedQueries = new List<string>();
            Func<string, IDictionary<string, object>, IEnumerable<int>> loadCallback =
                (queryText, queryParams) => { loadedQueries.Add(queryText); return results; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } });

            var enumerator = new GremlinPagedEnumerator<int>(loadCallback, baseQuery);
            for (var i = 0; i < 100; i++)
            {
                enumerator.MoveNext();
            }

            enumerator.MoveNext();

            CollectionAssert.AreEqual(
                new[] { "g.v(p0).outV.drop(p1).take(p2)", "g.v(p0).outV.drop(p1).take(p2)" },
                loadedQueries
            );
        }
    }
}
