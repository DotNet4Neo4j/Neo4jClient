using System.Linq;
using NSubstitute;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class AggregateTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void Length()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.Length() })
                .Query;

            Assert.Equal("RETURN length(n) AS Foo", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void Type()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.Type() })
                .Query;

            Assert.Equal("RETURN type(n) AS Foo", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void Id()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.Id() })
                .Query;

            Assert.Equal("RETURN id(n) AS Foo", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void Count()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.Count() })
                .Query;

            Assert.Equal("RETURN count(n) AS Foo", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

 


        [Fact]
        public void CountDistinct()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.CountDistinct() })
                .Query;

            Assert.Equal("RETURN count(distinct n) AS Foo", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void CountAll()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(() => new
                {
                    Foo = All.Count()
                })
                .Query;

            Assert.Equal("RETURN count(*) AS Foo", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void CountUsingICypderFluentQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            ICypherFluentQuery query = new CypherFluentQuery(client);

            var resultQuery =
                query.Return(() => new { Foo = All.Count() })
            .Query;

            Assert.Equal("RETURN count(*) AS Foo", resultQuery.QueryText);
            Assert.Equal(0, resultQuery.QueryParameters.Count());
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, resultQuery.ResultFormat);
        }

        [Fact]
        public void CountAllWithOtherIdentitiesWithNode()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(bar => new
                {
                    Foo = All.Count(),
                    Baz = bar.CollectAs<Node<object>>()
                })
                .Query;

            Assert.Equal("RETURN count(*) AS Foo, collect(bar) AS Baz", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
            Assert.Equal(CypherResultFormat.Rest, query.ResultFormat);
        }

        [Fact]
        public void CountAllWithOtherIdentities()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(bar => new
                {
                    Foo = All.Count(),
                    Baz = bar.CollectAs<object>()
                })
                .Query;

            Assert.Equal("RETURN count(*) AS Foo, collect(bar) AS Baz", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }
    }
}
