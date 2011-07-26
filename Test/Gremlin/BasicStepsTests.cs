using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class BasicStepsTests
    {
        [Test]
        public void OutVShouldAppendStepToNodeReference()
        {
            var node = new NodeReference(123);
            var queryText = node.OutV<object>().QueryText;
            Assert.AreEqual("g.v(123).outV", queryText);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQuery()
        {
            var node = new NodeReference(123);
            var queryText = node.OutV<object>().OutV<object>().QueryText;
            Assert.AreEqual("g.v(123).outV.outV", queryText);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQueryWithSingleFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .OutV<object>(new Dictionary<string, object>
                {
                    { "Foo", "Bar" }
                }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).outV[['Foo':'Bar']]", queryText);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQueryWithTwoFilters()
        {
            var node = new NodeReference(123);
            var queryText = node
                .OutV<object>(new Dictionary<string, object>
                {
                    { "Foo", "Bar" },
                    { "Baz", "Qak" }
                }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).outV[['Foo':'Bar'],['Baz':'Qak']]", queryText);
        }

        [Test]
        public void OutVShouldReturnTypedGremlinEnumerable()
        {
            var node = new NodeReference(123);
            var query = node.OutV<object>();
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
        }

        [Test]
        public void OutVShouldCombineWithInE()
        {
            var node = new NodeReference(123);
            var queryText = node.InE().OutV<object>().QueryText;
            Assert.AreEqual("g.v(123).inE.outV", queryText);
        }

        [Test]
        public void OutShouldAppendStepToGremlinQueryWithNoFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .Out<object>("REL")
                .QueryText;
            Assert.AreEqual("g.v(123).outE[[label:'REL']].inV", queryText);
        }

        [Test]
        public void OutShouldAppendStepToGremlinQueryWithSingleCaseSensitiveFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .Out<object>(
                    "REL",
                    new Dictionary<string, object>
                    {
                        { "Foo", "Bar" }
                    }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).outE[[label:'REL']].inV[['Foo':'Bar']]", queryText);
        }

        [Test]
        public void OutShouldAppendStepToGremlinQueryWithSingleCaseInsensitiveFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .Out<object>(
                    "REL",
                    new Dictionary<string, object>
                    {
                        { "Foo", "Bar" }
                    })
                .QueryText;
            Assert.AreEqual("g.v(123).outE[[label:'REL']].inV{ it.'Foo'.equalsIgnoreCase('Bar') }", queryText);
        }

        [Test]
        public void InShouldAppendStepToGremlinQueryWithNoFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .In<object>("REL")
                .QueryText;
            Assert.AreEqual("g.v(123).inE[[label:'REL']].outV", queryText);
        }

        [Test]
        public void InShouldAppendStepToGremlinQueryWithSingleCaseSensitiveFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .In<object>(
                    "REL",
                    new Dictionary<string, object>
                    {
                        { "Foo", "Bar" }
                    }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).inE[[label:'REL']].outV[['Foo':'Bar']]", queryText);
        }

        [Test]
        public void InShouldAppendStepToGremlinQueryWithSingleCaseInsensitiveFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .In<object>(
                    "REL",
                    new Dictionary<string, object>
                    {
                        { "Foo", "Bar" }
                    })
                .QueryText;
            Assert.AreEqual("g.v(123).inE[[label:'REL']].outV{ it.'Foo'.equalsIgnoreCase('Bar') }", queryText);
        }

        [Test]
        public void InVShouldAppendStepToGremlinQuery()
        {
            var node = new NodeReference(123);
            var queryText = node.InV<object>().QueryText;
            Assert.AreEqual("g.v(123).inV", queryText);
        }

        [Test]
        public void InVShouldAppendStepToGremlinQueryWithSingleFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .InV<object>(new Dictionary<string, object>
                {
                    { "Foo", "Bar" }
                }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).inV[['Foo':'Bar']]", queryText);
        }

        [Test]
        public void InVShouldAppendStepToGremlinQueryWithTwoFilters()
        {
            var node = new NodeReference(123);
            var queryText = node
                .InV<object>(new Dictionary<string, object>
                {
                    { "Foo", "Bar" },
                    { "Baz", "Qak" }
                }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).inV[['Foo':'Bar'],['Baz':'Qak']]", queryText);
        }

        [Test]
        public void InVShouldReturnTypedGremlinEnumerable()
        {
            var node = new NodeReference(123);
            var query = node.InV<object>();
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
        }

        [Test]
        public void InVShouldCombineWithOutE()
        {
            var node = new NodeReference(123);
            var queryText = node.OutE().InV<object>().QueryText;
            Assert.AreEqual("g.v(123).outE.inV", queryText);
        }

        [Test]
        public void OutEShouldAppendStepToGremlinQuery()
        {
            var node = new NodeReference(123);
            var queryText = node.OutE().QueryText;
            Assert.AreEqual("g.v(123).outE", queryText);
        }

        [Test]
        public void OutEShouldAppendStepToGremlinQueryWithLabel()
        {
            var node = new NodeReference(123);
            var queryText = node.OutE("FOO").QueryText;
            Assert.AreEqual("g.v(123).outE[[label:'FOO']]", queryText);
        }

        [Test]
        public void InEShouldAppendStepToGremlinQuery()
        {
            var node = new NodeReference(123);
            var queryText = node.InE().QueryText;
            Assert.AreEqual("g.v(123).inE", queryText);
        }

        [Test]
        public void InEShouldAppendStepToGremlinQueryWithLabel()
        {
            var node = new NodeReference(123);
            var queryText = node.InE("FOO").QueryText;
            Assert.AreEqual("g.v(123).inE[[label:'FOO']]", queryText);
        }

        [Test]
        public void ShouldCombineMultiStepQuery()
        {
            var query = NodeReference
                .RootNode
                .OutE("E_FOO")
                .InV<Foo>(new Dictionary<string, object> { { "Foo", "Bar" } }, StringComparison.Ordinal)
                .InE("E_BAR")
                .InV<Bar>();

            Assert.AreEqual("g.v(0).outE[[label:'E_FOO']].inV[['Foo':'Bar']].inE[[label:'E_BAR']].inV", query.QueryText);
        }

        [Test]
        public void GremlinCountShouldExecuteScalar()
        {
            var client = Substitute.For<IGraphClient>();
            client
                .ExecuteScalarGremlin("g.v(123).count()")
                .Returns("456");
            var node = new NodeReference(123, client);
            var result = node.GremlinCount();
            Assert.AreEqual(456, result);
        }

        [Test]
        [ExpectedException(typeof(DetachedNodeException))]
        public void GremlinCountShouldThrowDetachedNodeExceptionWhenBaseReferenceClientIsNull()
        {
            var node = new NodeReference(123);
            node.GremlinCount();
        }

        public class Foo
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        public class Bar
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
        }
    }
}