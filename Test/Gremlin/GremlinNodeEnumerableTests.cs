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
                .ExecuteGetAllNodesGremlin<object>("abc", parameters)
                .Returns(expectedResults);

            // Act
            var enumerable = new GremlinNodeEnumerable<object>(new GremlinQuery(client, "abc", parameters));
            var results = enumerable.ToArray();

            // Assert
            Assert.AreEqual(expectedResults, results);
        }
    }
}