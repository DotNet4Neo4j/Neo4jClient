using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Neo4jClient.Test
{
    [TestFixture]
    public class IndexEntryTests
    {
        [Test]
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

            Assert.AreEqual("index-entry", entry.Name);
            Assert.AreEqual(2, entry.KeyValues.Count());
            Assert.AreEqual("foo", entry.KeyValues.ElementAt(0).Key);
            Assert.AreEqual(123, entry.KeyValues.ElementAt(0).Value);
            Assert.AreEqual("bar", entry.KeyValues.ElementAt(1).Key);
            Assert.AreEqual("baz", entry.KeyValues.ElementAt(1).Value);
        }

        [Test]
        public void CanInitializeWithCollectionIntializer()
        {
            var entry = new IndexEntry("index-entry")
            {
                { "foo", 123 },
                { "bar", "baz" }
            };

            Assert.AreEqual("index-entry", entry.Name);
            Assert.AreEqual(2, entry.KeyValues.Count());
            Assert.AreEqual("foo", entry.KeyValues.ElementAt(0).Key);
            Assert.AreEqual(123, entry.KeyValues.ElementAt(0).Value);
            Assert.AreEqual("bar", entry.KeyValues.ElementAt(1).Key);
            Assert.AreEqual("baz", entry.KeyValues.ElementAt(1).Value);
        }

        [Test]
        public void CanCallAddAfterUsingNameConstructor()
        {
            // ReSharper disable UseObjectOrCollectionInitializer
            var entry = new IndexEntry("index-entry");
            entry.Add("qak", "qoo");
            // ReSharper restore UseObjectOrCollectionInitializer

            Assert.AreEqual(1, entry.KeyValues.Count());
            Assert.AreEqual("qak", entry.KeyValues.ElementAt(0).Key);
            Assert.AreEqual("qoo", entry.KeyValues.ElementAt(0).Value);
        }

        [Test]
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

            Assert.AreEqual(3, entry.KeyValues.Count());
            Assert.AreEqual("qak", entry.KeyValues.ElementAt(2).Key);
            Assert.AreEqual("qoo", entry.KeyValues.ElementAt(2).Value);
        }

        [Test]
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
