using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class CypherReturnExpressionBuilderTests
    {
        [Test]
        public void ReturnProperty()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var text = CypherReturnExpressionBuilder.BuildText(n => new ReturnPropertyQueryResult
            {
                SomethingTotallyDifferent = n.As<FooNode>().Age
            });

            Assert.AreEqual("n.Age AS SomethingTotallyDifferent", text);
        }

        public class FooNode
        {
            public int Age { get; set; }
        }

        public class ReturnPropertyQueryResult
        {
            public int SomethingTotallyDifferent { get; set; }
        }
    }
}
