using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class StartBitFormatterTests
    {
        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-node-by-id")]
        public void SingleNodeReference()
        {
            var cypher = ToCypher(new
            {
                n1 = (NodeReference) 1
            });

            Assert.AreEqual("n1=node({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(1, cypher.QueryParameters["p0"]);
        }

        [Test]
        public void SingleNode()
        {
            var cypher = ToCypher(new
            {
                n1 = new Node<object>(new object(), 123, null)
            });

            Assert.AreEqual("n1=node({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(123, cypher.QueryParameters["p0"]);
        }

        [Test]
        public void EnumerableOfNodes()
        {
            var cypher = ToCypher(new
            {
                n1 = new[] {123, 456}.Select(id => new Node<object>(new object(), id, null))
            });

            Assert.AreEqual("n1=node({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(new[] {123, 456}, cypher.QueryParameters["p0"]);
        }

        [Test]
        public void RootNodeReference()
        {
            var cypher = ToCypher(new
            {
                n1 = new RootNode(123)
            });

            Assert.AreEqual("n1=node({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(123, cypher.QueryParameters["p0"]);
        }

        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-node-by-id")]
        public void MultipleNodeReferences()
        {
            var cypher = ToCypher(new
            {
                n1 = (NodeReference)1,
                n2 = (NodeReference)2
            });

            Assert.AreEqual("n1=node({p0}), n2=node({p1})", cypher.QueryText);
            Assert.AreEqual(2, cypher.QueryParameters.Count);
            Assert.AreEqual(1, cypher.QueryParameters["p0"]);
            Assert.AreEqual(2, cypher.QueryParameters["p1"]);
        }

        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-multiple-nodes-by-id")]
        public void ArrayOfNodeReferences()
        {
            var cypher = ToCypher(new
            {
                n1 = new NodeReference[] { 1, 2 }
            });

            Assert.AreEqual("n1=node({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(new[] {1, 2}, cypher.QueryParameters["p0"]);
        }

        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-multiple-nodes-by-id")]
        public void EnumerableOfNodeReferences()
        {
            var cypher = ToCypher(new
            {
                n1 = new[] { 1, 2 }.Select(id => (NodeReference)id)
            });

            Assert.AreEqual("n1=node({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(new[] { 1, 2 }, cypher.QueryParameters["p0"]);
        }

        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-multiple-nodes-by-id")]
        public void EnumerableOfTypedNodeReferences()
        {
            var cypher = ToCypher(new
            {
                n1 = new[] { 1, 2 }.Select(id => (NodeReference<object>)id)
            });

            Assert.AreEqual("n1=node({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(new[] { 1, 2 }, cypher.QueryParameters["p0"]);
        }

        [Test]
        public void SingleRelationshipReference()
        {
            var cypher = ToCypher(new
            {
                r1 = (RelationshipReference)1
            });

            Assert.AreEqual("r1=relationship({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(1, cypher.QueryParameters["p0"]);
        }

        [Test]
        public void ArrayOfRelationshipReferences()
        {
            var cypher = ToCypher(new
            {
                r1 = new RelationshipReference[] { 1, 2 }
            });

            Assert.AreEqual("r1=relationship({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(new[] {1, 2}, cypher.QueryParameters["p0"]);
        }

        [Test]
        public void EnumerableOfRelationshipReferences()
        {
            var cypher = ToCypher(new
            {
                r1 = new[] { 1, 2 }.Select(id => (RelationshipReference)id)
            });

            Assert.AreEqual("r1=relationship({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual(new[] { 1, 2 }, cypher.QueryParameters["p0"]);
        }

        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-all-nodes")]
        public void AllNodes()
        {
            var cypher = ToCypher(new
            {
                n = All.Nodes
            });

            Assert.AreEqual("n=node(*)", cypher.QueryText);
            Assert.AreEqual(0, cypher.QueryParameters.Count);
        }

        [Test]
        public void CustomString()
        {
            var cypher = ToCypher(new
            {
                n1 = "foo"
            });

            Assert.AreEqual("n1=foo", cypher.QueryText);
            Assert.AreEqual(0, cypher.QueryParameters.Count);
        }

        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-node-by-index-lookup")]
        public void NodeByIndexLookup()
        {
            var cypher = ToCypher(new
            {
                n = Node.ByIndexLookup("someIndex", "name", "A")
            });

            Assert.AreEqual("n=node:someIndex(name = {p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual("A", cypher.QueryParameters["p0"]);
        }

        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-node-by-index-query")]
        public void NodeByIndexQuery()
        {
            var cypher = ToCypher(new
            {
                n = Node.ByIndexQuery("someIndex", "name:A")
            });

            Assert.AreEqual("n=node:someIndex({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual("name:A", cypher.QueryParameters["p0"]);
        }

        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-relationship-by-index-lookup")]
        public void RelationshipByIndexLookup()
        {
            var cypher = ToCypher(new
            {
                r = Relationship.ByIndexLookup("someIndex", "name", "A")
            });

            Assert.AreEqual("r=relationship:someIndex(name = {p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual("A", cypher.QueryParameters["p0"]);
        }

        [Test]
        public void RelationshipByIndexQuery()
        {
            var cypher = ToCypher(new
            {
                r = Relationship.ByIndexQuery("someIndex", "name:A")
            });

            Assert.AreEqual("r=relationship:someIndex({p0})", cypher.QueryText);
            Assert.AreEqual(1, cypher.QueryParameters.Count);
            Assert.AreEqual("name:A", cypher.QueryParameters["p0"]);
        }

        [Test]
        public void Mixed()
        {
            var nodeRef = (NodeReference)2;
            var relRef = (RelationshipReference)3;
            var relRef2 = (RelationshipReference)4;

            var cypher = ToCypher(new
            {
                n1 = "custom",
                n2 = nodeRef,
                n3 = Node.ByIndexLookup("indexName", "property", "value"),
                n4 = Node.ByIndexQuery("indexName", "query"),
                r1 = relRef,
                moreRels = new[] { relRef, relRef2 },
                r2 = Relationship.ByIndexLookup("indexName", "property", "value"),
                r3 = Relationship.ByIndexQuery("indexName", "query"),
                all = All.Nodes
            });

            const string expected =
                "n1=custom, " +
                "n2=node({p0}), " +
                "n3=node:indexName(property = {p1}), " +
                "n4=node:indexName({p2}), " +
                "r1=relationship({p3}), " +
                "moreRels=relationship({p4}), " +
                "r2=relationship:indexName(property = {p5}), " +
                "r3=relationship:indexName({p6}), " +
                "all=node(*)";

            Assert.AreEqual(expected, cypher.QueryText);
            Assert.AreEqual(7, cypher.QueryParameters.Count);
            Assert.AreEqual(2, cypher.QueryParameters["p0"]);
            Assert.AreEqual("value", cypher.QueryParameters["p1"]);
            Assert.AreEqual("query", cypher.QueryParameters["p2"]);
            Assert.AreEqual(3, cypher.QueryParameters["p3"]);
            Assert.AreEqual(new[] {3, 4}, cypher.QueryParameters["p4"]);
            Assert.AreEqual("value", cypher.QueryParameters["p5"]);
            Assert.AreEqual("query", cypher.QueryParameters["p6"]);
        }

        [Test]
        public void ThrowArgumentExceptionForEmptyObject()
        {
            var emptyObject = new {};
            var ex = Assert.Throws<ArgumentException>(
                () => StartBitFormatter.FormatAsCypherText(emptyObject, null)
            );
            Assert.AreEqual("startBits", ex.ParamName);
        }

        [Test]
        public void DontThrowArgumentExceptionForEmptyDictionary()
        {
            var emptyDictionary = new Dictionary<string, object>();
            Assert.DoesNotThrow(
                () => StartBitFormatter.FormatAsCypherText(emptyDictionary, null)
            );
        }

        [Test]
        public void ThrowNotSupportedExceptionForUnknownType()
        {
            var badObject = new { n1 = new StartBitFormatterTests() };
            Assert.Throws<NotSupportedException>(
                () => StartBitFormatter.FormatAsCypherText(badObject, null)
            );
        }

        [Test]
        public void NotSupportedExceptionForUnknownTypeIncludesIdentityName()
        {
            var badObject = new { n1 = new StartBitFormatterTests() };
            var exception = Assert.Throws<NotSupportedException>(
                () => StartBitFormatter.FormatAsCypherText(badObject, null)
            );
            StringAssert.Contains("n1", exception.Message);
        }

        [Test]
        public void NotSupportedExceptionForUnknownTypeIncludesTypeName()
        {
            var badObject = new { n1 = new StartBitFormatterTests() };
            var exception = Assert.Throws<NotSupportedException>(
                () => StartBitFormatter.FormatAsCypherText(badObject, null)
            );
            StringAssert.Contains(typeof(StartBitFormatterTests).FullName, exception.Message);
        }

        [Test]
        public void ThrowArgumentExceptionForNullValue()
        {
            var startBits = new { foo = (object)null };
            Assert.Throws<ArgumentException>(
                () => StartBitFormatter.FormatAsCypherText(startBits, null)
            );
        }

        [Test]
        public void ArgumentExceptionForNullValueIncludesPropertyName()
        {
            var startBits = new { foo = (object)null };
            var ex = Assert.Throws<ArgumentException>(
                () => StartBitFormatter.FormatAsCypherText(startBits, null)
            );
            StringAssert.Contains("foo", ex.Message);
        }

        static CypherQuery ToCypher(object startBits)
        {
            var parameters = new Dictionary<string, object>();
            Func<object, string> createParameter = value =>
            {
                var name = "p" + parameters.Count;
                parameters.Add(name, value);
                return string.Format("{{{0}}}", name);
            };

            var cypherText = StartBitFormatter.FormatAsCypherText(startBits, createParameter);

            var query = new CypherQuery(cypherText, parameters, CypherResultMode.Projection);
            return query;
        }
    }
}
