using System;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{


    [TestFixture]
    public class StartStepTests
    {
        [Test]
        public void StartShouldBeInNodeReference()
        {
            var node = new NodeReference(200);
            var query = node.Start();
            Assert.AreEqual("start thisNode=node({p0})", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
        }

        [Test]
        public void StartShouldCreateStepToNodeQuery()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.Start("myNode", BoundPoint.Node,new[] {200});
            Assert.IsInstanceOf<ICypherQuery>(query);
            Assert.AreEqual("start myNode=node(p0)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
        }

        [Test]
        public void StartShouldCreateStepToNodesQuery()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.Start("myNodes", BoundPoint.Node, new[] { 200, 201 });
            Assert.IsInstanceOf<ICypherQuery>(query);
            Assert.AreEqual("start myNodes=node(p0,p1)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
            Assert.AreEqual(201, query.QueryParameters["p1"]);
        }

        [Test]
        public void StartShouldCreateStepToNodesQueryUsingNodeReference()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.Start("myNodes", new[] { new NodeReference<object>(200), new NodeReference<object>(201) });
            Assert.IsInstanceOf<ICypherQuery>(query);
            Assert.AreEqual("start myNodes=node(p0,p1)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
            Assert.AreEqual(201, query.QueryParameters["p1"]);
        }

        [Test]
        public void StartShouldCreateStepToRelationshipQuery()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.Start("myRelationship", BoundPoint.Relationship, new[] { 200 });
            Assert.IsInstanceOf<ICypherQuery>(query);
            Assert.AreEqual("start myRelationship=relationship(p0)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
        }

        [Test]
        public void StartShouldCreateStepToRelationshipsQuery()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.Start("myRelationships", BoundPoint.Relationship, new[] { 200, 201 });
            Assert.IsInstanceOf<ICypherQuery>(query);
            Assert.AreEqual("start myRelationships=relationship(p0,p1)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
            Assert.AreEqual(201, query.QueryParameters["p1"]);
        }

        [Test]
        public void StartShouldCreateStepToRelationshipsQueryUsingRelationshipReference()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.Start("myRelationships", new[] { new RelationshipReference(200), new RelationshipReference(201)});
            Assert.IsInstanceOf<ICypherQuery>(query);
            Assert.AreEqual("start myRelationships=relationship(p0,p1)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
            Assert.AreEqual(201, query.QueryParameters["p1"]);
        }
    }
}