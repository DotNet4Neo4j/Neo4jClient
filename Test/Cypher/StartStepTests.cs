using System;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class StartStepTests
    {
        [Test]
        public void StartShouldCreateStepToNodeQuery()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.StartV<object>("myNode",new[] {200});
            Assert.IsInstanceOf<ICypherNodeQuery<object>>(query);
            Assert.AreEqual("start myNode=node(p0)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
        }

        [Test]
        public void StartShouldCreateStepToNodesQuery()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.StartV<object>("myNodes", new[] { 200, 201 });
            Assert.IsInstanceOf<ICypherNodeQuery<object>>(query);
            Assert.AreEqual("start myNodes=node(p0,p1)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
            Assert.AreEqual(201, query.QueryParameters["p1"]);
        }

        [Test]
        public void StartShouldCreateStepToRelationshipQuery()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.StartE<object>("myRelationship", new[] { 200 });
            Assert.IsInstanceOf<ICypherRelationshipQuery<object>>(query);
            Assert.AreEqual("start myRelationship=relationship(p0)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
        }

        [Test]
        public void StartShouldCreateStepToRelationshipsQuery()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"));
            var query = client.Cypher.StartE<object>("myRelationships", new[] { 200, 201 });
            Assert.IsInstanceOf<ICypherRelationshipQuery<object>>(query);
            Assert.AreEqual("start myRelationships=relationship(p0,p1)", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
            Assert.AreEqual(201, query.QueryParameters["p1"]);
        }
    }
}