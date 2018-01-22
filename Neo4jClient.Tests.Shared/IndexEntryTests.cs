using System;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test
{
    
    public class IndexEntryTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void CanInitializeWithLongForm()
        {
            var entry = new IndexEntry
            {
                Name = "index-entry",
                KeyValues = new[]
                {
                    new KeyValuePair<string, object>("foo", 123),
                    new KeyValuePair<string, object>("bar", "baz")
                }
            };

            Assert.Equal("index-entry", entry.Name);
            Assert.Equal(2, entry.KeyValues.Count());
            Assert.Equal("foo", entry.KeyValues.ElementAt(0).Key);
            Assert.Equal(123, entry.KeyValues.ElementAt(0).Value);
            Assert.Equal("bar", entry.KeyValues.ElementAt(1).Key);
            Assert.Equal("baz", entry.KeyValues.ElementAt(1).Value);
        }

        [Fact]
        public void CanInitializeWithCollectionIntializer()
        {
            var entry = new IndexEntry("index-entry")
            {
                { "foo", 123 },
                { "bar", "baz" }
            };

            Assert.Equal("index-entry", entry.Name);
            Assert.Equal(2, entry.KeyValues.Count());
            Assert.Equal("foo", entry.KeyValues.ElementAt(0).Key);
            Assert.Equal(123, entry.KeyValues.ElementAt(0).Value);
            Assert.Equal("bar", entry.KeyValues.ElementAt(1).Key);
            Assert.Equal("baz", entry.KeyValues.ElementAt(1).Value);
        }

        [Fact]
        public void CanCallAddAfterUsingNameConstructor()
        {
            // ReSharper disable UseObjectOrCollectionInitializer
            var entry = new IndexEntry("index-entry");
            entry.Add("qak", "qoo");
            // ReSharper restore UseObjectOrCollectionInitializer

            Assert.Equal(1, entry.KeyValues.Count());
            Assert.Equal("qak", entry.KeyValues.ElementAt(0).Key);
            Assert.Equal("qoo", entry.KeyValues.ElementAt(0).Value);
        }

        [Fact]
        public void CanCallAddAfterUsingCollectionIntializer()
        {
            // ReSharper disable UseObjectOrCollectionInitializer
            var entry = new IndexEntry("index-entry")
            {
                { "foo", 123 },
                { "bar", "baz" }
            };
            // ReSharper restore UseObjectOrCollectionInitializer

            entry.Add("qak", "qoo");

            Assert.Equal(3, entry.KeyValues.Count());
            Assert.Equal("qak", entry.KeyValues.ElementAt(2).Key);
            Assert.Equal("qoo", entry.KeyValues.ElementAt(2).Value);
        }

        [Fact]
        public void AddAfterAssigningCustomListShouldThrowException()
        {
            var entry = new IndexEntry
            {
                KeyValues = new[]
                {
                    new KeyValuePair<string, object>("foo", 123)
                }
            };

            Assert.Throws<InvalidOperationException>(() => entry.Add("qak", "qoo"));
        }
    }
}
