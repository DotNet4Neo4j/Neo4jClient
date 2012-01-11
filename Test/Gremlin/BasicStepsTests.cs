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
            var query = new NodeReference(123).OutV<object>().OutV<object>();
            Assert.AreEqual("g.v(p0).outV.outV", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQueryWithSingleEqualFilter()
        {
            var query = new NodeReference(123)
                .OutV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
                }, StringComparison.Ordinal);
            Assert.AreEqual("g.v(p0).outV.filter{ it[p1].equals(p2) }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("Foo", query.QueryParameters["p1"]);
            Assert.AreEqual("Bar", query.QueryParameters["p2"]);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQueryWithTwoEqualFilters()
        {
            var query = new NodeReference(123)
                .OutV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                    new Filter { PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  },
                }, StringComparison.Ordinal);
            Assert.AreEqual("g.v(p0).outV.filter{ it[p1].equals(p2) && it[p3].equals(p4) }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("Foo", query.QueryParameters["p1"]);
            Assert.AreEqual("Bar", query.QueryParameters["p2"]);
            Assert.AreEqual("Baz", query.QueryParameters["p3"]);
            Assert.AreEqual("Qak", query.QueryParameters["p4"]);
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
            var query = new NodeReference(123).InE().OutV<object>();
            Assert.AreEqual("g.v(p0).inE.outV", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void OutShouldAppendStepToGremlinQueryWithNoFilter()
        {
            var query = new NodeReference(123).Out<object>("REL");
            Assert.AreEqual("g.v(p0).out(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("REL", query.QueryParameters["p1"]);
        }

        [Test]
        public void OutShouldAppendStepToGremlinQueryWithSingleCaseSensitiveEqualFilter()
        {
            var query = new NodeReference(123)
                .Out<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
                    }, StringComparison.Ordinal);
            Assert.AreEqual("g.v(p0).out(p1).filter{ it[p2].equals(p3) }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("REL", query.QueryParameters["p1"]);
            Assert.AreEqual("Foo", query.QueryParameters["p2"]);
            Assert.AreEqual("Bar", query.QueryParameters["p3"]);
        }

        [Test]
        public void OutShouldAppendStepToGremlinQueryWithSingleCaseInsensitiveEqualFilter()
        {
            var query = new NodeReference(123)
                .Out<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
                    });
            Assert.AreEqual("g.v(p0).out(p1).filter{ it[p2].equalsIgnoreCase(p3) }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("REL", query.QueryParameters["p1"]);
            Assert.AreEqual("Foo", query.QueryParameters["p2"]);
            Assert.AreEqual("Bar", query.QueryParameters["p3"]);
        }

        [Test]
        public void OutShouldAppendStepToGremlinQueryWithEqualFilterForTextOfEnum()
        {
            var query = new NodeReference(123)
                .Out<TestNodeWithNullableEnum>(
                    "REL",
                    x => x.Boo == TestEnum.Bar
                    );
            Assert.AreEqual("g.v(p0).out(p1).filter{ it[p2].equalsIgnoreCase(p3) }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("REL", query.QueryParameters["p1"]);
            Assert.AreEqual("Boo", query.QueryParameters["p2"]);
            Assert.AreEqual("Bar", query.QueryParameters["p3"]);
        }

        [Test]
        public void InShouldAppendStepToGremlinQueryWithNoFilter()
        {
            var query = new NodeReference(123).In<object>("REL");
            Assert.AreEqual("g.v(p0).in(p1)", query.QueryText);
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
            Assert.AreEqual("g.v(p0).in(p1).outV.filter{ it[p2].equals(p3) }", query.QueryText);
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
            Assert.AreEqual("g.v(p0).in(p1).outV.filter{ it[p2].equalsIgnoreCase(p3) }", query.QueryText);
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
            var query = new NodeReference(123)
                .InV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal }
                }, StringComparison.Ordinal);
            Assert.AreEqual("g.v(p0).inV.filter{ it[p1].equals(p2) }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("Foo", query.QueryParameters["p1"]);
            Assert.AreEqual("Bar", query.QueryParameters["p2"]);
        }

        [Test]
        public void InVShouldAppendStepToGremlinQueryWithTwoEqualFilters()
        {
            var query = new NodeReference(123)
                .InV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                    new Filter { PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  },
                }, StringComparison.Ordinal);
            Assert.AreEqual("g.v(p0).inV.filter{ it[p1].equals(p2) && it[p3].equals(p4) }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("Foo", query.QueryParameters["p1"]);
            Assert.AreEqual("Bar", query.QueryParameters["p2"]);
            Assert.AreEqual("Baz", query.QueryParameters["p3"]);
            Assert.AreEqual("Qak", query.QueryParameters["p4"]);
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
            var query = new NodeReference(123).OutE().InV<object>();
            Assert.AreEqual("g.v(p0).outE.inV", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void OutEShouldAppendStepToGremlinQuery()
        {
            var query = new NodeReference(123).OutE();
            Assert.AreEqual("g.v(p0).outE", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void OutEShouldAppendStepToGremlinQueryWithLabel()
        {
            var query = new NodeReference(123).OutE("FOO");
            Assert.AreEqual("g.v(p0).out(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("FOO", query.QueryParameters["p1"]);
        }

        [Test]
        public void TypedOutEShouldAppendStepToGremlinQueryWithLabel()
        {
            var query = new NodeReference(123).OutE<object>("FOO");
            Assert.AreEqual("g.v(p0).out(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("FOO", query.QueryParameters["p1"]);
        }

        [Test]
        public void TypedOutEShouldAppendStepToGremlinQueryWithoutLabel()
        {
            var query = new NodeReference(123).OutE<object>();
            Assert.AreEqual("g.v(p0).outE", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
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
            Assert.AreEqual("g.v(p0).in(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("FOO", query.QueryParameters["p1"]);
        }

        [Test]
        public void TypedInEShouldAppendStepToGremlinQueryWithLabel()
        {
            var query = new NodeReference(123).InE<object>("FOO");
            Assert.AreEqual("g.v(p0).in(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("FOO", query.QueryParameters["p1"]);
        }

        [Test]
        public void TypedInEShouldAppendStepToGremlinQueryWithoutLabel()
        {
            var query = new NodeReference(123).InE<object>();
            Assert.AreEqual("g.v(p0).inE", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
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

            Assert.AreEqual("g.v(p0).out(p1).inV.filter{ it[p2].equals(p3) }.in(p4).inV", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters["p0"]);
            Assert.AreEqual("E_FOO", query.QueryParameters["p1"]);
            Assert.AreEqual("Foo", query.QueryParameters["p2"]);
            Assert.AreEqual("Bar", query.QueryParameters["p3"]);
            Assert.AreEqual("E_BAR", query.QueryParameters["p4"]);
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