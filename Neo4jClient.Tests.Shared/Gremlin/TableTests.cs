using System.Collections.Generic;
using System.Linq;
using Xunit;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Gremlin
{
    
    public class TableTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void TableShouldAppendStepToQuery()
        {
            var query = new NodeReference(123)
                .OutV<object>()
                .As("foo")
                .Table<TableResult>();

            Assert.IsAssignableFrom<GremlinProjectionEnumerable<TableResult>>(query);
            Assert.IsAssignableFrom<IEnumerable<TableResult>>(query);
            Assert.IsAssignableFrom<IGremlinQuery>(query);

            var enumerable = (IGremlinQuery)query;
            Assert.Equal("g.v(p0).outV.as(p1).table(new Table()).cap", enumerable.QueryText);
            Assert.Equal(123L, enumerable.QueryParameters["p0"]);
            Assert.Equal("foo", enumerable.QueryParameters["p1"]);
        }

        [Fact]
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

            Assert.IsAssignableFrom<GremlinProjectionEnumerable<TableResult>>(query);
            Assert.IsAssignableFrom<IEnumerable<TableResult>>(query);
            Assert.IsAssignableFrom<IGremlinQuery>(query);

            var enumerable = (IGremlinQuery)query;
            Assert.Equal("g.v(p0).outV.as(p1).inV.as(p2).table(new Table()){it[p3]}{it[p4]}.cap", enumerable.QueryText);
            Assert.Equal(123L, enumerable.QueryParameters["p0"]);
            Assert.Equal("foo", enumerable.QueryParameters["p1"]);
            Assert.Equal("bar", enumerable.QueryParameters["p2"]);
            Assert.Equal("SomeText", enumerable.QueryParameters["p3"]);
            Assert.Equal("SomeNumber", enumerable.QueryParameters["p4"]);
        }

        [Fact]
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
            Assert.Equal("data", result.First().Foo);
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