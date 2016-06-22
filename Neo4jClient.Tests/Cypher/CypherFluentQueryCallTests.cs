using System;
using Neo4jClient.Cypher;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryCallTests
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
        public void CallsStoredProcedureGiven()
        {
            var client = GraphClient_30;
            var query = new CypherFluentQuery(client)
                .Call("apoc.sp()")
                .Query;

            Assert.AreEqual("CALL apoc.sp()", query.QueryText);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowArgumentException_WhenNoStoredProcedureIsGiven()
        {
            var client = GraphClient_30;
            var query = new CypherFluentQuery(client)
                .Call(null).Query;
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void ThrowsInvalidOperationException_WhenClientVersionIsLessThan_30()
        {
            var client = GraphClient_30;
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);

            var query = new CypherFluentQuery(client)
                .Call("apoc.sp")
                .Query;
        }
    }
}