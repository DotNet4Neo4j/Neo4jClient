using System;
using Neo4jClient.Cypher;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryYieldTests
    {
        private static IRawGraphClient GraphClient_30
        {
            get
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(CypherCapabilities.Cypher30);
                return client;
            }
        }

        [Test]
        public void YieldsGivenText()
        {
            var client = GraphClient_30;
            var query = new CypherFluentQuery(client)
                .Yield("uuid")
                .Query;

            Assert.AreEqual("YIELD uuid", query.QueryText);
        }

        [Test]
        public void ThrowArgumentException_WhenNoStoredProcedureIsGiven()
        {
            var client = GraphClient_30;
            Assert.Throws<ArgumentException>(() =>
            {
                var query = new CypherFluentQuery(client).Yield(null).Query;
            });
        }

        [Test]

        public void ThrowsInvalidOperationException_WhenClientVersionIsLessThan_30()
        {
            var client = GraphClient_30;
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);

            Assert.Throws<InvalidOperationException>(() => { var query = new CypherFluentQuery(client).Yield("uuid").Query; });
        }
    }
}