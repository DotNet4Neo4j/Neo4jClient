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
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-return-property
            // START n=node(1)
            // RETURN n.FirstName

            var text = CypherReturnExpressionBuilder.BuildText(n => new ReturnPropertyQueryResult
            {
                ClientName = n.As<FooNode>().FirstName
            });

            Assert.AreEqual("n.FirstName", text);
        }

        public class FooNode
        {
            public string FirstName { get; set; }
        }

        public class ReturnPropertyQueryResult
        {
            public string ClientName { get; set; }
        }
    }
}
