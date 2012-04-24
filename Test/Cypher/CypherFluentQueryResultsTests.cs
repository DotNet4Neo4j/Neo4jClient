using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryResultsTests
    {
        [Test]
        public void ReturnColumnAlias()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IGraphClient>();

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

            Assert.IsInstanceOf<IEnumerable<ReturnPropertyQueryResult>>(results);
        }

        [Test]
        public void ReturnColumnAliasOfTypeEnum()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IGraphClient>();

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

            Assert.IsInstanceOf<IEnumerable<FooNode>>(results);
        }

        [Test]
        public void ReturnNodeAsSet()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a

            var client = Substitute.For<IGraphClient>();

            client
                .ExecuteGetCypherResults<Node<FooNode>>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<Node<FooNode>>());

            var cypher = new CypherFluentQuery(client);
            var results = cypher
                .Start("a", (NodeReference)1)
                .Return<Node<FooNode>>("a")
                .ResultSet;

            Assert.IsInstanceOf<IEnumerable<Node<FooNode>>>(results);
        }

        [Test]
        public void ExecutingQueryMultipleTimesShouldResetParameters()
        {
            var client = Substitute.For<IGraphClient>();

            client
                .ExecuteGetCypherResults<ReturnPropertyQueryResult>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<ReturnPropertyQueryResult>());

            var cypher = new CypherFluentQuery(client);
            var results = cypher
                .Start("a", (NodeReference)1)
                .Return<object>("a.Name")
                .Results
                .Count();

            results += cypher
                .Start("a", (NodeReference)1)
                .Return<object>("a.Name")
                .Results
                .Count();

            Assert.AreEqual(0, cypher.Query.QueryParameters.Count());
            Assert.AreEqual(0, results);
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
