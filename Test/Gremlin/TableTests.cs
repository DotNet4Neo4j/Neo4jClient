using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.Gremlin;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class TableTests
    {
        [Test]
        public void TableShouldAppendStepToQuery()
        {
            var query = new NodeReference(123)
                .OutV<object>()
                .As("foo")
                .Table<TableResult>();

            Assert.IsInstanceOf<GremlinProjectionEnumerable<TableResult>>(query);
            Assert.IsInstanceOf<IEnumerable<TableResult>>(query);
            Assert.IsInstanceOf<IGremlinQuery>(query);

            var enumerable = (IGremlinQuery)query;
            Assert.AreEqual("g.v(p0).outV.as(p1).table(new Table()).cap", enumerable.QueryText);
            Assert.AreEqual(123, enumerable.QueryParameters["p0"]);
            Assert.AreEqual("foo", enumerable.QueryParameters["p1"]);
        }

        [Test]
        public void TableShouldAppendStepToQueryWithClosures()
        {
            var query = new NodeReference(123)
                .OutV<Foo>()
                .As("foo")
                .InV<Bar>()
                .As("bar")
                .Table<TableResult, Foo, Bar>(
                    foo => foo.SomeText,
                    bar => bar.SomeNumber
                );

            Assert.IsInstanceOf<GremlinProjectionEnumerable<TableResult>>(query);
            Assert.IsInstanceOf<IEnumerable<TableResult>>(query);
            Assert.IsInstanceOf<IGremlinQuery>(query);

            var enumerable = (IGremlinQuery)query;
            Assert.AreEqual("g.v(p0).outV.as(p1).inV.as(p2).table(new Table()){it[p3]}{it[p4]}.cap", enumerable.QueryText);
            Assert.AreEqual(123, enumerable.QueryParameters["p0"]);
            Assert.AreEqual("foo", enumerable.QueryParameters["p1"]);
            Assert.AreEqual("bar", enumerable.QueryParameters["p2"]);
            Assert.AreEqual("SomeText", enumerable.QueryParameters["p3"]);
            Assert.AreEqual("SomeNumber", enumerable.QueryParameters["p4"]);
        }

        [Test]
        public void TableCapShouldTransferResponseToResult()
        {
            // Arrange
            var responses = new List<List<GremlinTableCapResponse>>
                {
                    new List<GremlinTableCapResponse>
                        {
                            new GremlinTableCapResponse
                                {
                                    Columns = new List<string>
                                        {
                                            "Foo"
                                        },
                                    Data = new List<List<string>>
                                        {
                                            new List<string>
                                                {"data"}
                                        }
                                }
                        }
                };

            // Act
            var result = GremlinTableCapResponse.TransferResponseToResult<TableResult>(responses, new JsonConverter[0]).ToArray();

            // Assert
            Assert.AreEqual("data", result.First().Foo);
        }

        public class Foo
        {
            public string SomeText { get; set; }
        }

        public class Bar
        {
            public int SomeNumber { get; set; }
        }

        public class TableResult
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }
    }
}