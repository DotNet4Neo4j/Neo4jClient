using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class GremlinNodeEnumerableTests
    {
        [Test]
        [ExpectedException(typeof(DetachedNodeException))]
        public void GetEnumeratorShouldThrowDetachedNodeExceptionWhenClientNotSet()
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            IEnumerable<Node<object>> enumerable = new GremlinNodeEnumerable<object>(new GremlinQuery(null, "abc", null));
            enumerable.GetEnumerator();
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void GetEnumeratorShouldExecuteQueryAgainstClient()
        {
            // Arrange
            var expectedResults = new[]
            {
                new Node<object>(new object(), new NodeReference<object>(123)),
                new Node<object>(new object(), new NodeReference<object>(456)),
                new Node<object>(new object(), new NodeReference<object>(789))
            };
            var parameters = new Dictionary<string, object>();
            var client = Substitute.For<IGraphClient>();
            client
                .ExecuteGetAllNodesGremlin<object>(Arg.Any<IGremlinQuery>())
                .Returns(expectedResults);

            // Act
            var enumerable = new GremlinNodeEnumerable<object>(new GremlinQuery(client, "abc", parameters));
            var results = enumerable.ToArray();

            // Assert
            Assert.AreEqual(expectedResults, results);
        }

        [Test]
        public void DebugQueryTextShouldReturnExpandedText()
        {
            var gremlinQuery = new GremlinQuery(
                null,
                "g[p0][p1][p2][p3][p4][p5][p6][p7][p8][p9][p10][p11]",
                new Dictionary<string, object>
                {
                    { "p0", "val00" },
                    { "p1", "val01" },
                    { "p2", "val02" },
                    { "p3", "val03" },
                    { "p4", "val04" },
                    { "p5", "val05" },
                    { "p6", "val06" },
                    { "p7", "val07" },
                    { "p8", "val08" },
                    { "p9", "val09" },
                    { "p10", "val10" },
                    { "p11", "val11" },
                });
            var enumerable = new GremlinNodeEnumerable<object>(gremlinQuery);
            Assert.AreEqual(
                "g['val00']['val01']['val02']['val03']['val04']['val05']['val06']['val07']['val08']['val09']['val10']['val11']",
                enumerable.DebugQueryText);
        }
    }
}