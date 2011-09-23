using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            var query = node.OutV<object>();
            Assert.AreEqual("g.v(p0).outV", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQuery()
        {
            var node = new NodeReference(123);
            var queryText = node.OutV<object>().OutV<object>().QueryText;
            Assert.AreEqual("g.v(123).outV.outV", queryText);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQueryWithSingleEqualFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .OutV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
                }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).outV{ it.'Foo'.equals('Bar') }", queryText);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQueryWithTwoEqualFilters()
        {
            var node = new NodeReference(123);
            var queryText = node
                .OutV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                    new Filter { PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  },
                }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).outV{ it.'Foo'.equals('Bar') && it.'Baz'.equals('Qak') }", queryText);
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
        public void OutShouldAppendStepToGremlinQueryWithSingleCaseSensitiveEqualFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .Out<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
                    }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).outE[[label:'REL']].inV{ it.'Foo'.equals('Bar') }", queryText);
        }

        [Test]
        public void OutShouldAppendStepToGremlinQueryWithSingleCaseInsensitiveEqualFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .Out<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
                    })
                .QueryText;
            Assert.AreEqual("g.v(123).outE[[label:'REL']].inV{ it.'Foo'.equalsIgnoreCase('Bar') }", queryText);
        }

        [Test]
        public void OutShouldAppendStepToGremlinQueryWithEqualFilterForTextOfEnum()
        {
            var node = new NodeReference(123);
            var queryText = node
                .Out<TestNodeWithNullableEnum>(
                    "REL",
                    x => x.Boo == TestEnum.Bar
                    )
                .QueryText;
            Assert.AreEqual("g.v(123).outE[[label:'REL']].inV{ it.'Boo'.equalsIgnoreCase('Bar') }", queryText);
        }

        [Test]
        public void InShouldAppendStepToGremlinQueryWithNoFilter()
        {
            var query = new NodeReference(123).In<object>("REL");
            Assert.AreEqual("g.v(p0).inE[[label:p1]].outV", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("REL", query.QueryParameters["p1"]);
        }

        [Test]
        public void InShouldAppendStepToGremlinQueryWithSingleCaseSensitiveEqualFilter()
        {
            var query = new NodeReference(123)
                .In<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal}
                    }, StringComparison.Ordinal);
            Assert.AreEqual("g.v(p0).inE[[label:p1]].outV{ it[p2].equals(p3) }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("REL", query.QueryParameters["p1"]);
            Assert.AreEqual("Foo", query.QueryParameters["p2"]);
            Assert.AreEqual("Bar", query.QueryParameters["p3"]);
        }

        [Test]
        public void InShouldAppendStepToGremlinQueryWithSingleCaseInsensitiveEqualFilter()
        {
            var query = new NodeReference(123)
                .In<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal }
                    });
            Assert.AreEqual("g.v(p0).inE[[label:p1]].outV{ it[p2].equalsIgnoreCase(p3) }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("REL", query.QueryParameters["p1"]);
            Assert.AreEqual("Foo", query.QueryParameters["p2"]);
            Assert.AreEqual("Bar", query.QueryParameters["p3"]);
        }

        [Test]
        public void InVShouldAppendStepToGremlinQuery()
        {
            var query = new NodeReference(123).InV<object>();
            Assert.AreEqual("g.v(p0).inV", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void InVShouldAppendStepToGremlinQueryWithSingleEqualFilter()
        {
            var node = new NodeReference(123);
            var queryText = node
                .InV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal }
                }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).inV{ it.'Foo'.equals('Bar') }", queryText);
        }

        [Test]
        public void InVShouldAppendStepToGremlinQueryWithTwoEqualFilters()
        {
            var node = new NodeReference(123);
            var queryText = node
                .InV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                    new Filter { PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  },
                }, StringComparison.Ordinal)
                .QueryText;
            Assert.AreEqual("g.v(123).inV{ it.'Foo'.equals('Bar') && it.'Baz'.equals('Qak') }", queryText);
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
            var query = new NodeReference(123).InE();
            Assert.AreEqual("g.v(p0).inE", query.QueryText);
        }

        [Test]
        public void InEShouldAppendStepToGremlinQueryWithLabel()
        {
            var query = new NodeReference(123).InE("FOO");
            Assert.AreEqual("g.v(p0).inE[[label:p1]]", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("FOO", query.QueryParameters["p1"]);
        }

        [Test]
        public void ShouldCombineMultiStepEqualQuery()
        {
            var query = NodeReference
                .RootNode
                .OutE("E_FOO")
                .InV<Foo>(new List<Filter> { new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal } }, StringComparison.Ordinal)
                .InE("E_BAR")
                .InV<Bar>();

            Assert.AreEqual("g.v(0).outE[[label:'E_FOO']].inV{ it.'Foo'.equals('Bar') }.inE[[label:'E_BAR']].inV", query.QueryText);
        }

        [Test]
        public void GremlinCountShouldExecuteScalar()
        {
            var client = Substitute.For<IGraphClient>();
            client
                .ExecuteScalarGremlin(
                    "g.v(p0).count()",
                    Arg.Is<IDictionary<string, object>>(
                        d => (int)d["p0"] == 123))
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

        public enum TestEnum { Bar }

        public class TestNodeWithNullableEnum
        {
            public TestEnum? Boo { get; set; }
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