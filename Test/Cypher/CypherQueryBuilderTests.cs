using NUnit.Framework;
using Neo4jClient.Cypher;
using System.Linq.Expressions;
using System;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherQueryBuilderTests
    {
        [Test]
        public void ShouldBuildSingleStartBitWithSingleNode()
        {
            var builder = new CypherQueryBuilder();
            builder.AddStartBit("n", (NodeReference)1);

            var query = builder.ToQuery();

            Assert.AreEqual("START n=node({p0})", query.QueryText); 
            Assert.AreEqual(1, query.QueryParameters["p0"]);
        }

        [Test]
        public void ShouldBuildSingleStartBitWithSingleRelationship()
        {
            var builder = new CypherQueryBuilder();
            builder.AddStartBit("r", (RelationshipReference)1);

            var query = builder.ToQuery();

            Assert.AreEqual("START r=relationship({p0})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
        }

        [Test]
        public void ShouldBuildSingleStartBitWithMultipleNodes()
        {
            var builder = new CypherQueryBuilder();
            builder.AddStartBit("n", (NodeReference)1, (NodeReference)2, (NodeReference)3);

            var query = builder.ToQuery();

            Assert.AreEqual("START n=node({p0}, {p1}, {p2})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
            Assert.AreEqual(3, query.QueryParameters["p2"]);
        }

        [Test]
        public void ShouldBuildMultipleStartPoints()
        {
            var builder = new CypherQueryBuilder();
            builder.AddStartBit("a", (NodeReference)1);
            builder.AddStartBit("b", (NodeReference)2);

            var query = builder.ToQuery();

            Assert.AreEqual("START a=node({p0}), b=node({p1})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        public void ShouldUseSetResultModeForIdentityBasedReturn()
        {
            var builder = new CypherQueryBuilder();
            builder.SetReturn("foo", false);

            var query = builder.ToQuery();

            Assert.AreEqual(CypherResultMode.Set, query.ResultMode);
        }

        [Test]
        public void ShouldUseProjectionResultModeForLambdaBasedReturn()
        {
            Expression<Func<ICypherResultItem, object>> expression =
                a => new { Foo = a.As<object>() };

            var builder = new CypherQueryBuilder();
            builder.SetReturn(expression, false);

            var query = builder.ToQuery();

            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
        }
    }
}
