using System;
using NUnit.Framework;
using NSubstitute;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryUsingIndexTests
    {
        [Test]
        public void UsesIndex()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(foo:Bar { id: 123 })")
                .UsingIndex(":Bar(id)")
                .Return(foo => new { qux = foo.As<object>() } )
                .Query;

            Assert.AreEqual("MATCH (foo:Bar { id: 123 })\r\nUSING INDEX :Bar(id)\r\nRETURN foo AS qux", query.QueryText);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void UsingEmptyIndexIsInvalid(string index)
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(foo:Bar { id: 123 })")
                .UsingIndex(index)
                .Return(foo => new { qux = foo.As<object>() } )
                .Query;

        }
    }
}
