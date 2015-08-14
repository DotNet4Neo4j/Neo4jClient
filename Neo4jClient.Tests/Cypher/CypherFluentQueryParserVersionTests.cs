using System;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    public class CypherFluentQueryParserVersionTests
    {
        [Test]
        public void SetsVersionToFreeTextGiven()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion("2.1.experimental")
                .Start(new
                {
                    n = All.Nodes,
                })
                .Return<object>("n")
                .Query;

            Assert.AreEqual("CYPHER 2.1.experimental\r\nSTART n=node(*)\r\nRETURN n", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);   
        }

        [Test]
        public void SetsVersion_WhenUsingVersionOverload()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion(new Version(1, 9))
                .Start(new
                {
                    n = All.Nodes,
                })
                .Return<object>("n")
                .Query;

            Assert.AreEqual("CYPHER 1.9\r\nSTART n=node(*)\r\nRETURN n", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void SetsVersion_WhenUsingIntOverload()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion(1, 9)
                .Start(new
                {
                    n = All.Nodes,
                })
                .Return<object>("n")
                .Query;

            Assert.AreEqual("CYPHER 1.9\r\nSTART n=node(*)\r\nRETURN n", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void UsesLegacy_WhenVersionRequestedIsLessThan1_9()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion(1, 8)
                .Start(new
                {
                    n = All.Nodes,
                })
                .Return<object>("n")
                .Query;

            Assert.AreEqual("CYPHER LEGACY\r\nSTART n=node(*)\r\nRETURN n", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);            
        }
    }
}
