using System;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    class CypherFluentQueryRelateTests
    {
        [Test]
        public void CreateNodeWithValuesViaRelate()
        {
            // http://docs.neo4j.org/chunked/1.8.M03/query-relate.html#relate-create-nodes-with-values
            //START root=node(2)
            //RELATE root-[:X]-(leaf {name:'D'} )
            //RETURN leaf

            var client = Substitute.For<IRawGraphClient>();
            client.ServerVersion.Returns(new Version(1, 8, 0, 1));
            var query = new CypherFluentQuery(client)
                .Start("root", (NodeReference)2)
                .Relate("root-[:X]-(leaf {name:'D'} )")
                .Return<object>("leaf")
                .Query;

            Assert.AreEqual("START root=node({p0})\r\nRELATE root-[:X]-(leaf {name:'D'} )\r\nRETURN leaf", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters["p0"]);
        }

        [Test]
        public void CreateNodeWithValuesViaRelateAfterMatch()
        {
            //START root=node(2)
            //MATCH root-[:X]-foo
            //RELATE foo-[:Y]-(leaf {name:'D'} )
            //RETURN leaf

            var client = Substitute.For<IRawGraphClient>();
            client.ServerVersion.Returns(new Version(1, 8, 0, 1));
            var query = new CypherFluentQuery(client)
                .Start("root", (NodeReference)2)
                .Match("root-[:X]-foo")
                .Relate("foo-[:Y]-(leaf {name:'D'} )")
                .Return<object>("leaf")
                .Query;

            Assert.AreEqual("START root=node({p0})\r\nMATCH root-[:X]-foo\r\nRELATE foo-[:Y]-(leaf {name:'D'} )\r\nRETURN leaf", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters["p0"]);
        }

        [Test]
        [TestCase("1.8.0.7")]
        [TestCase("1.8", Description = "The first 1.8, non-milestone release")]
        [TestCase("1.9")]
        public void RelateThrowsNotSupportedExceptionInVersionsAfterItWasRenamedToCreateUnique(string version)
        {
            try
            {
                var client = Substitute.For<IRawGraphClient>();
                client.ServerVersion.Returns(new Version(version));
                new CypherFluentQuery(client)
                    .Start("root", (NodeReference) 2)
                    .Match("root-[:X]-foo")
                    .Relate("foo-[:Y]-(leaf {name:'D'} )");

                Assert.Fail("The expected exception was never thrown");
            }
            catch (NotSupportedException ex)
            {
                Assert.AreEqual(
                    "You're trying to use the RELATE keyword against a Neo4j instance ≥ 1.8M07. In Neo4j 1.8M07, it was renamed from RELATE to CREATE UNIQUE. You need to update your code to use our new CreateUnique method. (We didn't want to just plumb the Relate method to CREATE UNIQUE, because that would introduce a deviation between the .NET wrapper and the Cypher language.)\r\n\r\nSee https://github.com/systay/community/commit/c7dbbb929abfef600266a20f065d760e7a1fff2e for detail.",
                    ex.Message);
            }
        }
    }
}
