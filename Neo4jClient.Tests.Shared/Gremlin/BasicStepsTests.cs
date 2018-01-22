using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NSubstitute;
using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class BasicStepsTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void BothVShouldAppendStepToNodeReference()
        {
            var node = new NodeReference(123);
            var query = node.BothV<object>();
            Assert.Equal("g.v(p0).bothV", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void OutVShouldAppendStepToNodeReference()
        {
            var node = new NodeReference(123);
            var query = node.OutV<object>();
            Assert.Equal("g.v(p0).outV", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void OutVShouldAppendStepToGremlinQuery()
        {
            var query = new NodeReference(123).OutV<object>().OutV<object>();
            Assert.Equal("g.v(p0).outV.outV", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void OutVShouldAppendStepToGremlinQueryWithSingleEqualFilter()
        {
            var query = new NodeReference(123)
                .OutV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
                }, StringComparison.Ordinal);
            Assert.Equal("g.v(p0).outV.filter{ it[p1].equals(p2) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("Foo", query.QueryParameters["p1"]);
            Assert.Equal("Bar", query.QueryParameters["p2"]);
        }

        [Fact]
        public void OutVShouldAppendStepToGremlinQueryWithTwoEqualFilters()
        {
            var query = new NodeReference(123)
                .OutV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                    new Filter { PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  },
                }, StringComparison.Ordinal);
            Assert.Equal("g.v(p0).outV.filter{ it[p1].equals(p2) && it[p3].equals(p4) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("Foo", query.QueryParameters["p1"]);
            Assert.Equal("Bar", query.QueryParameters["p2"]);
            Assert.Equal("Baz", query.QueryParameters["p3"]);
            Assert.Equal("Qak", query.QueryParameters["p4"]);
        }

        [Fact]
        public void OutVShouldReturnTypedGremlinEnumerable()
        {
            var node = new NodeReference(123);
            var query = node.OutV<object>();
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
        }

        [Fact]
        public void OutVShouldCombineWithInE()
        {
            var query = new NodeReference(123).InE().OutV<object>();
            Assert.Equal("g.v(p0).inE.outV", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void OutShouldAppendStepToGremlinQueryWithNoFilter()
        {
            var query = new NodeReference(123).Out<object>("REL");
            Assert.Equal("g.v(p0).out(p1)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("REL", query.QueryParameters["p1"]);
        }

        [Fact]
        public void OutShouldAppendStepToGremlinQueryWithSingleCaseSensitiveEqualFilter()
        {
            var query = new NodeReference(123)
                .Out<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
                    }, StringComparison.Ordinal);
            Assert.Equal("g.v(p0).out(p1).filter{ it[p2].equals(p3) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("REL", query.QueryParameters["p1"]);
            Assert.Equal("Foo", query.QueryParameters["p2"]);
            Assert.Equal("Bar", query.QueryParameters["p3"]);
        }

        [Fact]
        public void OutShouldAppendStepToGremlinQueryWithSingleCaseInsensitiveEqualFilter()
        {
            var query = new NodeReference(123)
                .Out<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
                    });
            Assert.Equal("g.v(p0).out(p1).filter{ it[p2].equalsIgnoreCase(p3) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("REL", query.QueryParameters["p1"]);
            Assert.Equal("Foo", query.QueryParameters["p2"]);
            Assert.Equal("Bar", query.QueryParameters["p3"]);
        }

        [Fact]
        public void OutShouldAppendStepToGremlinQueryWithEqualFilterForTextOfEnum()
        {
            var query = new NodeReference(123)
                .Out<TestNodeWithNullableEnum>(
                    "REL",
                    x => x.Boo == TestEnum.Bar
                    );
            Assert.Equal("g.v(p0).out(p1).filter{ it[p2].equalsIgnoreCase(p3) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("REL", query.QueryParameters["p1"]);
            Assert.Equal("Boo", query.QueryParameters["p2"]);
            Assert.Equal("Bar", query.QueryParameters["p3"]);
        }

        [Fact]
        public void InShouldAppendStepToGremlinQueryWithNoFilter()
        {
            var query = new NodeReference(123).In<object>("REL");
            Assert.Equal("g.v(p0).in(p1)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("REL", query.QueryParameters["p1"]);
        }

        [Fact]
        public void InShouldAppendStepToGremlinQueryWithSingleCaseSensitiveEqualFilter()
        {
            var query = new NodeReference(123)
                .In<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal}
                    }, StringComparison.Ordinal);
            Assert.Equal("g.v(p0).in(p1).filter{ it[p2].equals(p3) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("REL", query.QueryParameters["p1"]);
            Assert.Equal("Foo", query.QueryParameters["p2"]);
            Assert.Equal("Bar", query.QueryParameters["p3"]);
        }

        [Fact]
        public void InShouldAppendStepToGremlinQueryWithSingleCaseInsensitiveEqualFilter()
        {
            var query = new NodeReference(123)
                .In<object>(
                    "REL",
                    new List<Filter>
                    {
                        new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal }
                    });
            Assert.Equal("g.v(p0).in(p1).filter{ it[p2].equalsIgnoreCase(p3) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("REL", query.QueryParameters["p1"]);
            Assert.Equal("Foo", query.QueryParameters["p2"]);
            Assert.Equal("Bar", query.QueryParameters["p3"]);
        }

        [Fact]
        public void InVShouldAppendStepToGremlinQuery()
        {
            var query = new NodeReference(123).InV<object>();
            Assert.Equal("g.v(p0).inV", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void InVShouldAppendStepToGremlinQueryWithSingleEqualFilter()
        {
            var query = new NodeReference(123)
                .InV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal }
                }, StringComparison.Ordinal);
            Assert.Equal("g.v(p0).inV.filter{ it[p1].equals(p2) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("Foo", query.QueryParameters["p1"]);
            Assert.Equal("Bar", query.QueryParameters["p2"]);
        }

        [Fact]
        public void InVShouldAppendStepToGremlinQueryWithTwoEqualFilters()
        {
            var query = new NodeReference(123)
                .InV<object>(new List<Filter>
                {
                    new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                    new Filter { PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  },
                }, StringComparison.Ordinal);
            Assert.Equal("g.v(p0).inV.filter{ it[p1].equals(p2) && it[p3].equals(p4) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("Foo", query.QueryParameters["p1"]);
            Assert.Equal("Bar", query.QueryParameters["p2"]);
            Assert.Equal("Baz", query.QueryParameters["p3"]);
            Assert.Equal("Qak", query.QueryParameters["p4"]);
        }

        [Fact]
        public void InVShouldReturnTypedGremlinEnumerable()
        {
            var node = new NodeReference(123);
            var query = node.InV<object>();
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
        }

        [Fact]
        public void InVShouldCombineWithOutE()
        {
            var query = new NodeReference(123).OutE().InV<object>();
            Assert.Equal("g.v(p0).outE.inV", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void BothEShouldAppendStepToGremlinQuery()
        {
            var query = new NodeReference(123).BothE();
            Assert.Equal("g.v(p0).bothE", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void OutEShouldAppendStepToGremlinQuery()
        {
            var query = new NodeReference(123).OutE();
            Assert.Equal("g.v(p0).outE", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void OutEShouldAppendStepToGremlinQueryWithLabel()
        {
            var query = new NodeReference(123).OutE("FOO");
            Assert.Equal("g.v(p0).outE.filter{ it[p1].equals(p2) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("label", query.QueryParameters["p1"]);
            Assert.Equal("FOO", query.QueryParameters["p2"]);
        }

        [Fact]
        public void TypedOutEShouldAppendStepToGremlinQueryWithLabel()
        {
            var query = new NodeReference(123).OutE<object>("FOO");
            Assert.Equal("g.v(p0).outE.filter{ it[p1].equals(p2) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("label", query.QueryParameters["p1"]);
            Assert.Equal("FOO", query.QueryParameters["p2"]);
        }

        [Fact]
        public void TypedOutEShouldAppendStepToGremlinQueryWithoutLabel()
        {
            var query = new NodeReference(123).OutE<object>();
            Assert.Equal("g.v(p0).outE", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void InEShouldAppendStepToGremlinQuery()
        {
            var query = new NodeReference(123).InE();
            Assert.Equal("g.v(p0).inE", query.QueryText);
        }

        [Fact]
        public void InEShouldAppendStepToGremlinQueryWithLabel()
        {
            var query = new NodeReference(123).InE("FOO");
            Assert.Equal("g.v(p0).inE.filter{ it[p1].equals(p2) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("label", query.QueryParameters["p1"]);
            Assert.Equal("FOO", query.QueryParameters["p2"]);
        }

        [Fact]
        public void TypedInEShouldAppendStepToGremlinQueryWithLabel()
        {
            var query = new NodeReference(123).InE<object>("FOO");
            Assert.Equal("g.v(p0).inE.filter{ it[p1].equals(p2) }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("label", query.QueryParameters["p1"]);
            Assert.Equal("FOO", query.QueryParameters["p2"]);
        }

        [Fact]
        public void TypedInEShouldAppendStepToGremlinQueryWithoutLabel()
        {
            var query = new NodeReference(123).InE<object>();
            Assert.Equal("g.v(p0).inE", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void ShouldCombineMultiStepEqualQuery()
        {
            var query = new NodeReference(0)
                .OutE("E_FOO")
                .InV<Foo>(new List<Filter> { new Filter { PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal } }, StringComparison.Ordinal)
                .InE("E_BAR")
                .InV<Bar>();

            Assert.Equal("g.v(p0).outE.filter{ it[p1].equals(p2) }.inV.filter{ it[p3].equals(p4) }.inE.filter{ it[p5].equals(p6) }.inV", query.QueryText);
            Assert.Equal(0L, query.QueryParameters["p0"]);
            Assert.Equal("label", query.QueryParameters["p1"]);
            Assert.Equal("E_FOO", query.QueryParameters["p2"]);
            Assert.Equal("Foo", query.QueryParameters["p3"]);
            Assert.Equal("Bar", query.QueryParameters["p4"]);
            Assert.Equal("label", query.QueryParameters["p5"]);
            Assert.Equal("E_BAR", query.QueryParameters["p6"]);
        }

        [Fact]
        public void GremlinCountShouldExecuteScalar()
        {
            var client = Substitute.For<IGraphClient>();
            client
                .ExecuteScalarGremlin(
                    "g.v(p0).count()",
                    Arg.Is<IDictionary<string, object>>(
                        d => (long)d["p0"] == 123))
                .Returns("456");
            var node = new NodeReference(123L, client);
            var result = node.GremlinCount();
            Assert.Equal(456, result);
        }

        [Fact]
        public void GremlinCountShouldThrowDetachedNodeExceptionWhenBaseReferenceClientIsNull()
        {
            var node = new NodeReference(123);
            Assert.Throws<DetachedNodeException>(() => node.GremlinCount());
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