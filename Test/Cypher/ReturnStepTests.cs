using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class ReturnStepTests
    {
        [Test]
        public void ReturnShouldAppendStep()
        {
            var node = new NodeReference<object>(200);
            var query = node.Start().Return("ColumnName1");
            Assert.AreEqual("start thisNode=node({p0}) return ColumnName1", query.QueryText);
            Assert.AreEqual(200, query.QueryParameters["p0"]);
        }
    }
} 