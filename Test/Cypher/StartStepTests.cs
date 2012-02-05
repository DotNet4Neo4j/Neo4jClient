using System;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class StartStepTests
    {
        [Test]
        public void ShouldBuildFromNodeReference()
        {
            var node = new NodeReference(200);
            var query = node.Start();
            Assert.AreEqual("start thisNode=node({p0})", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
        }

        [Test]
        public void ShouldAllowMultipleNodeReferencesInSingleStart()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.Start("myNodes", (NodeReference)200, (NodeReference)201);

            Assert.IsInstanceOf<ICypherQuery>(query);
            Assert.AreEqual("start myNodes=node(p0,p1)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
            Assert.AreEqual(201, query.QueryParameters["p1"]);
        }

        [Test]
        public void ShouldAllowMultipleRelationshipReferencesInSingleStart()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.Start("myRelationships", (RelationshipReference)200, (RelationshipReference)201);

            Assert.IsInstanceOf<ICypherQuery>(query);
            Assert.AreEqual("start myRelationships=relationship(p0,p1)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
            Assert.AreEqual(201, query.QueryParameters["p1"]);
        }
    }
}
