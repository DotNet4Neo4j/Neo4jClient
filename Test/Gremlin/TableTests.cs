using System.Collections.Generic;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class TableTests
    {
        [Test]
        public void TableShouldAppendStepToQuery()
        {
            var query = new NodeReference(123)
                .OutV<object>()
                .As("foo")
                .Table<TableResult>();

            Assert.IsInstanceOf<GremlinProjectionEnumerable<TableResult>>(query);
            Assert.IsInstanceOf<IEnumerable<TableResult>>(query);
            Assert.IsInstanceOf<IGremlinQuery>(query);

            var enumerable = (IGremlinQuery)query;
            Assert.AreEqual("g.v(p0).outV.as(p1).table(new Table())", enumerable.QueryText);
            Assert.AreEqual(123, enumerable.QueryParameters["p0"]);
            Assert.AreEqual("foo", enumerable.QueryParameters["p1"]);
        }

        public class TableResult
        {
        }
    }
}