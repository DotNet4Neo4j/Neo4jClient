using System;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    class CypherFluentQueryCreateUniqueTests
    {
        [Test]
        public void CreateNodeWithValuesViaCreateUnique()
        {
            // http://docs.neo4j.org/chunked/1.8.M03/query-relate.html#relate-create-nodes-with-values
            //START root=node(2)
            //CREATE UNIQUE root-[:X]-(leaf {name:'D'} )
            //RETURN leaf

            var client = Substitute.For<IRawGraphClient>();
            client.ServerVersion.Returns(new Version(1, 8));
            var query = new CypherFluentQuery(client)
                .Start("root", (NodeReference)2)
                .CreateUnique("root-[:X]-(leaf {name:'D'} )")
                .Return<object>("leaf")
                .Query;

            Assert.AreEqual("START root=node({p0})\r\nCREATE UNIQUE root-[:X]-(leaf {name:'D'} )\r\nRETURN leaf", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters["p0"]);
        }

        [Test]
        public void CreateNodeWithValuesViaCreateUniqueAfterMatch()
        {
            //START root=node(2)
            //MATCH root-[:X]-foo
            //CREATE UNIQUE foo-[:Y]-(leaf {name:'D'} )
            //RETURN leaf

            var client = Substitute.For<IRawGraphClient>();
            client.ServerVersion.Returns(new Version(1, 8));
            var query = new CypherFluentQuery(client)
                .Start("root", (NodeReference)2)
                .Match("root-[:X]-foo")
                .CreateUnique("foo-[:Y]-(leaf {name:'D'} )")
                .Return<object>("leaf")
                .Query;

            Assert.AreEqual("START root=node({p0})\r\nMATCH root-[:X]-foo\r\nCREATE UNIQUE foo-[:Y]-(leaf {name:'D'} )\r\nRETURN leaf", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters["p0"]);
        }
    }
}
