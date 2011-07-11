using System;
using System.Collections.Specialized;
using System.Linq;
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
                .OutV<object>(new NameValueCollection
                {
                    { "Foo", "Bar" }
                })
                .QueryText;
            Assert.AreEqual("g.v(123).outV[['Foo':'Bar']]", queryText);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQueryWithTwoFilters()
        {
            var node = new NodeReference(123);
            var queryText = node
                .OutV<object>(new NameValueCollection
                {
                    { "Foo", "Bar" },
                    { "Baz", "Qak" }
                })
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
                .InV<object>(new NameValueCollection
                {
                    { "Foo", "Bar" }
                })
                .QueryText;
            Assert.AreEqual("g.v(123).inV[['Foo':'Bar']]", queryText);
        }

        [Test]
        public void InVShouldAppendStepToGremlinQueryWithTwoFilters()
        {
            var node = new NodeReference(123);
            var queryText = node
                .InV<object>(new NameValueCollection
                {
                    { "Foo", "Bar" },
                    { "Baz", "Qak" }
                })
                .QueryText;
            Assert.AreEqual("g.v(123).inV[['Foo':'Bar'],['Baz':'Qak']]", queryText);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantExpression()
        {
            var filters = new NameValueCollection();
            BasicSteps.TranslateFilter<Foo>(
                f => f.Prop1 == "abc", // This must be a constant - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.AllKeys.Single());
            Assert.AreEqual("abc", filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalExpression()
        {
            var filters = new NameValueCollection();
            var prop1Value = new string(new[] { 'a', 'b', 'c' }); // This must be a local - do not refactor this to a constant
            BasicSteps.TranslateFilter<Foo>(
                f => f.Prop1 == prop1Value,
                filters
            );
            Assert.AreEqual("Prop1", filters.AllKeys.Single());
            Assert.AreEqual("abc", filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAnotherPropertyExpression()
        {
            var filters = new NameValueCollection();
            var bar = new Bar { Prop1 = "def" };
            BasicSteps.TranslateFilter<Foo>(
                f => f.Prop1 == bar.Prop1, // This must be a method call - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.AllKeys.Single());
            Assert.AreEqual("def", filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAFunctionExpression()
        {
            var filters = new NameValueCollection();
            BasicSteps.TranslateFilter<Foo>(
                f => f.Prop1 == string.Format("{0}.{1}", "abc", "def").ToUpperInvariant(), // This must be a method call - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.AllKeys.Single());
            Assert.AreEqual("ABC.DEF", filters["Prop1"]);
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
            var queryText = NodeReference
                .RootNode
                .OutE("E_FOO")
                .InV<Foo>(new NameValueCollection { { "Foo", "Bar" } })
                .InE("E_BAR")
                .InV<Bar>()
                .QueryText;
            Assert.AreEqual("g.v(0).outE[[label:'E_FOO']].inV[['Foo':'Bar']].inE[[label:'E_BAR']].inV", queryText);
        }

        [Test]
        public void NodeCountShouldExecuteScalar()
        {
            var client = Substitute.For<IGraphClient>();
            client
                .ExecuteScalarGremlin("g.v(123).count()")
                .Returns("456");
            var node = new NodeReference(123, client);
            var result = node.NodeCount();
            Assert.AreEqual(456, result);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseSensititiveFilters()
        {
            var filters = new NameValueCollection();
            var filterText = BasicSteps.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual(string.Empty, filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveFilter()
        {
            var filters = new NameValueCollection
            {
                { "Foo", "Bar" }
            };
            var filterText = BasicSteps.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("[['Foo':'Bar']]", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForMultipleCaseSensititiveFilters()
        {
            var filters = new NameValueCollection
            {
                { "Foo", "Bar" },
                { "Baz", "Qak" }
            };
            var filterText = BasicSteps.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("[['Foo':'Bar'],['Baz':'Qak']]", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseInsensitiveFilters()
        {
            var filters = new NameValueCollection();
            var filterText = BasicSteps.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual(string.Empty, filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveFilter()
        {
            var filters = new NameValueCollection
            {
                { "Foo", "Bar" }
            };
            var filterText = BasicSteps.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForMultipleCaseInsensititiveFilters()
        {
            var filters = new NameValueCollection
            {
                { "Foo", "Bar" },
                { "Baz", "Qak" }
            };
            var filterText = BasicSteps.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') && it.'Baz'.equalsIgnoreCase('Qak') }", filterText);
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