using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQueryResultsTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task ReturnColumnAlias()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IRawGraphClient>();

            client
                .ExecuteGetCypherResultsAsync<ReturnPropertyQueryResult>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<ReturnPropertyQueryResult>());

            var cypher = new CypherFluentQuery(client);
            var results = await cypher
                .Match("a")
                .Return(a => new ReturnPropertyQueryResult
                {
                    SomethingTotallyDifferent = a.As<FooNode>().Age,
                })
                .ResultsAsync;

            Assert.IsAssignableFrom<IEnumerable<ReturnPropertyQueryResult>>(results);
        }

      
        [Fact]
        public async Task ReturnColumnAliasOfTypeEnum()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IRawGraphClient>();

            client
                .ExecuteGetCypherResultsAsync<ReturnPropertyQueryResult>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<ReturnPropertyQueryResult>());

            var cypher = new CypherFluentQuery(client);
            var results = await cypher
                .Match("a")
                .Return(a => new FooNode
                {
                    TheType = a.As<FooNode>().TheType,
                })
                .ResultsAsync;

            Assert.IsAssignableFrom<IEnumerable<FooNode>>(results);
        }

        [Fact]
        public async Task ReturnNodeAsSet()
        {
            var client = Substitute.For<IRawGraphClient>();
            var set = new[] { new Node<FooNode>(new FooNode(), new NodeReference<FooNode>(123)) };
            client
                .ExecuteGetCypherResultsAsync<Node<FooNode>>(
                    Arg.Is<CypherQuery>(q => q.ResultMode == CypherResultMode.Set))
                .Returns(set);

            var cypher = new CypherFluentQuery(client);
            var results = await cypher
                .Match("a")
                .Return<Node<FooNode>>("a")
                .ResultsAsync;

            Assert.Equal(set, results);
        }

        [Fact]
        public async Task ReturnRelationshipWithDataAsSet()
        {
            var client = Substitute.For<IRawGraphClient>();
            var set = new[] { new RelationshipInstance<FooNode>(new RelationshipReference<FooNode>(1), new NodeReference(0), new NodeReference(2),"Type", new FooNode()) };
            client
                .ExecuteGetCypherResultsAsync<RelationshipInstance<FooNode>>(
                    Arg.Is<CypherQuery>(q => q.ResultMode == CypherResultMode.Set))
                .Returns(set);

            var cypher = new CypherFluentQuery(client);
            var results = await cypher
                .Match("a")
                .Return<RelationshipInstance<FooNode>>("a")
                .ResultsAsync;

            Assert.Equal(set, results);
        }

        [Fact]
        public async Task ReturnRelationshipAsSet()
        {
            var client = Substitute.For<IRawGraphClient>();
            var set = new[] { new RelationshipInstance(new RelationshipReference(1), new NodeReference(0), new NodeReference(2), "Type") };
            client
                .ExecuteGetCypherResultsAsync<RelationshipInstance>(
                    Arg.Is<CypherQuery>(q => q.ResultMode == CypherResultMode.Set))
                .Returns(set);

            var cypher = new CypherFluentQuery(client);
            var results = await cypher
                .Match("a")
                .Return<RelationshipInstance>("a")
                .ResultsAsync;

            Assert.Equal(set, results);
        }

        [Fact]
        public void ExecutingQueryMultipleTimesShouldResetParameters()
        {
            var client = Substitute.For<IRawGraphClient>();

            client
                .ExecuteGetCypherResultsAsync<ReturnPropertyQueryResult>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<ReturnPropertyQueryResult>());

            var cypher = new CypherFluentQuery(client);
            var query1 = cypher
                .Match("a")
                .Return<object>("a.Name")
                .Query;

            Assert.Equal(0, query1.QueryParameters.Count());


            var query2 = cypher
                .Match("b")
                .Return<object>("b.Name")
                .Query;

            Assert.Equal(0, query2.QueryParameters.Count());
        }

        public enum MyType {Type1, Type2}

        public class FooNode
        {
            public int Age { get; set; }
            public MyType TheType { get; set; }
        }

        public class ReturnPropertyQueryResult
        {
            public int SomethingTotallyDifferent { get; set; }
        }
    }
}
