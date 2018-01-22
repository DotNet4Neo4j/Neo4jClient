using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class GremlinPagedEnumeratorTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldNotLoadAnythingUntilEnumerated()
        {
            var loadedQueries = new List<IGremlinQuery>();
            Func<IGremlinQuery, IEnumerable<object>> loadCallback =
                q => { loadedQueries.Add(q); return new object[0]; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } },
                null);
            
            new GremlinPagedEnumerator<object>(loadCallback, baseQuery);

            Assert.Equal(0, loadedQueries.Count());
        }

        [Fact]
        public void ShouldLoadFirstPageOfResultsWithFirstEnumeration()
        {
            var loadedQueries = new List<IGremlinQuery>();
            Func<IGremlinQuery, IEnumerable<object>> loadCallback =
                q => { loadedQueries.Add(q); return new object[0]; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } },
                null);

            var enumerator = new GremlinPagedEnumerator<object>(loadCallback, baseQuery);
            enumerator.MoveNext();

            Assert.Equal(1, loadedQueries.Count());
            Assert.Equal("g.v(p0).outV.drop(p1).take(p2)._()", loadedQueries[0].QueryText);
            Assert.Equal(0, loadedQueries[0].QueryParameters["p1"]);
            Assert.Equal(100, loadedQueries[0].QueryParameters["p2"]);
        }

        [Fact]
        public void ShouldEnumerateOverFirstPageOfResults()
        {
            var results = Enumerable.Range(0, 100).ToArray();

            var loadedQueries = new List<IGremlinQuery>();
            Func<IGremlinQuery, IEnumerable<int>> loadCallback =
                q => { loadedQueries.Add(q); return results; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } },
                null);

            var enumerator = new GremlinPagedEnumerator<int>(loadCallback, baseQuery);
            for (var i = 0; i < 100; i++)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(results[i], enumerator.Current);
            }
        }

        [Fact]
        public void MoveNextShouldReturnFalseOnFirstCallIfThereAreNoResults()
        {
            var results = new int[0];

            var loadedQueries = new List<IGremlinQuery>();
            Func<IGremlinQuery, IEnumerable<int>> loadCallback =
                q => { loadedQueries.Add(q); return results; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } },
                null);

            var enumerator = new GremlinPagedEnumerator<int>(loadCallback, baseQuery);
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void MoveNextShouldReturnFalseAfterLastRecordOnFirstPageIfThereAreNoFurtherPages()
        {
            var pages = new Queue<IEnumerable<int>>(new[]
            {
                Enumerable.Range(0, 100),
                Enumerable.Empty<int>()
            });

            var loadedQueries = new List<IGremlinQuery>();
            Func<IGremlinQuery, IEnumerable<int>> loadCallback =
                q => { loadedQueries.Add(q); return pages.Dequeue(); };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } },
                null);

            var enumerator = new GremlinPagedEnumerator<int>(loadCallback, baseQuery);

            for (var i = 0; i < 100; i++)
                enumerator.MoveNext();

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void MoveNextShouldReturnFalseAfterLastRecordOnPartialPage()
        {
            var pages = new Queue<IEnumerable<int>>(new[]
            {
                Enumerable.Range(0, 50)
            });

            var loadedQueries = new List<IGremlinQuery>();
            Func<IGremlinQuery, IEnumerable<int>> loadCallback =
                q => { loadedQueries.Add(q); return pages.Dequeue(); };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } },
                null);

            var enumerator = new GremlinPagedEnumerator<int>(loadCallback, baseQuery);

            for (var i = 0; i < 50; i++)
                enumerator.MoveNext();

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void ShouldLoadSecondPageWhenCallingMoveNextAfterLastRecordOfFirstPage()
        {
            var results = Enumerable.Range(0, 100).ToArray();

            var loadedQueries = new List<IGremlinQuery>();
            Func<IGremlinQuery, IEnumerable<int>> loadCallback =
                q => { loadedQueries.Add(q); return results; };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } },
                null);

            var enumerator = new GremlinPagedEnumerator<int>(loadCallback, baseQuery);
            for (var i = 0; i < 100; i++)
            {
                enumerator.MoveNext();
            }

            enumerator.MoveNext();

            Assert.Equal(2, loadedQueries.Count());
            Assert.Equal("g.v(p0).outV.drop(p1).take(p2)._()", loadedQueries[0].QueryText);
            Assert.Equal(0, loadedQueries[0].QueryParameters["p1"]);
            Assert.Equal(100, loadedQueries[0].QueryParameters["p2"]);
            Assert.Equal("g.v(p0).outV.drop(p1).take(p2)._()", loadedQueries[1].QueryText);
            Assert.Equal(100, loadedQueries[1].QueryParameters["p1"]);
            Assert.Equal(100, loadedQueries[1].QueryParameters["p2"]);
        }

        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(10)]
        public void ShouldEnumerateOverMultiplePagesOfResults(int pageCount)
        {
            var pages = new Queue<IEnumerable<int>>();
            for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                pages.Enqueue(Enumerable.Range(pageIndex * 100, 100));
            }

            var loadedQueries = new List<IGremlinQuery>();
            Func<IGremlinQuery, IEnumerable<int>> loadCallback =
                q => { loadedQueries.Add(q); return pages.Dequeue(); };

            var baseQuery = new GremlinQuery(
                null,
                "g.v(p0).outV",
                new Dictionary<string, object> { { "p0", 0 } },
                null);

            var enumerator = new GremlinPagedEnumerator<int>(loadCallback, baseQuery);
            for (var i = 0; i < pageCount * 100; i++)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(i, enumerator.Current);
            }
        }
    }
}
