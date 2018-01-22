using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryResultsTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ReturnColumnAlias()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IRawGraphClient>();

            client
                .ExecuteGetCypherResults<ReturnPropertyQueryResult>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<ReturnPropertyQueryResult>());

            var cypher = new CypherFluentQuery(client);
            var results = cypher
                .Start("a", (NodeReference)1)
                .Return(a => new ReturnPropertyQueryResult
                {
                    SomethingTotallyDifferent = a.As<FooNode>().Age,
                })
                .Results;

            Assert.IsAssignableFrom<IEnumerable<ReturnPropertyQueryResult>>(results);
        }

      
        [Fact]
        public void ReturnColumnAliasOfTypeEnum()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IRawGraphClient>();

            client
                .ExecuteGetCypherResults<ReturnPropertyQueryResult>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<ReturnPropertyQueryResult>());

            var cypher = new CypherFluentQuery(client);
            var results = cypher
                .Start("a", (NodeReference)1)
                .Return(a => new FooNode
                {
                    TheType = a.As<FooNode>().TheType,
                })
                .Results;

            Assert.IsAssignableFrom<IEnumerable<FooNode>>(results);
        }

        [Fact]
        public void ReturnNodeAsSet()
        {
            var client = Substitute.For<IRawGraphClient>();
            var set = new[] { new Node<FooNode>(new FooNode(), new NodeReference<FooNode>(123)) };
            client
                .ExecuteGetCypherResults<Node<FooNode>>(
                    Arg.Is<CypherQuery>(q => q.ResultMode == CypherResultMode.Set))
                .Returns(set);

            var cypher = new CypherFluentQuery(client);
            var results = cypher
                .Start("a", (NodeReference)1)
                .Return<Node<FooNode>>("a")
                .Results;

            Assert.Equal(set, results);
        }

        [Fact]
        public void ReturnRelationshipWithDataAsSet()
        {
            var client = Substitute.For<IRawGraphClient>();
            var set = new[] { new RelationshipInstance<FooNode>(new RelationshipReference<FooNode>(1), new NodeReference(0), new NodeReference(2),"Type", new FooNode()) };
            client
                .ExecuteGetCypherResults<RelationshipInstance<FooNode>>(
                    Arg.Is<CypherQuery>(q => q.ResultMode == CypherResultMode.Set))
                .Returns(set);

            var cypher = new CypherFluentQuery(client);
            var results = cypher
                .Start("a", (RelationshipReference)1)
                .Return<RelationshipInstance<FooNode>>("a")
                .Results;

            Assert.Equal(set, results);
        }

        [Fact]
        public void ReturnRelationshipAsSet()
        {
            var client = Substitute.For<IRawGraphClient>();
            var set = new[] { new RelationshipInstance(new RelationshipReference(1), new NodeReference(0), new NodeReference(2), "Type") };
            client
                .ExecuteGetCypherResults<RelationshipInstance>(
                    Arg.Is<CypherQuery>(q => q.ResultMode == CypherResultMode.Set))
                .Returns(set);

            var cypher = new CypherFluentQuery(client);
            var results = cypher
                .Start("a", (RelationshipReference)1)
                .Return<RelationshipInstance>("a")
                .Results;

            Assert.Equal(set, results);
        }

        [Fact]
        public void ExecutingQueryMultipleTimesShouldResetParameters()
        {
            var client = Substitute.For<IRawGraphClient>();

            client
                .ExecuteGetCypherResults<ReturnPropertyQueryResult>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<ReturnPropertyQueryResult>());

            var cypher = new CypherFluentQuery(client);
            var query1 = cypher
                .Start("a", (NodeReference)1)
                .Return<object>("a.Name")
                .Query;

            Assert.Equal(1, query1.QueryParameters.Count());
            Assert.Equal(1L, query1.QueryParameters["p0"]);

            var query2 = cypher
                .Start("b", (NodeReference)2)
                .Return<object>("a.Name")
                .Query;

            Assert.Equal(1, query2.QueryParameters.Count());
            Assert.Equal(2L, query2.QueryParameters["p0"]);
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
