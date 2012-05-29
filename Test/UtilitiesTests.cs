using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test
{
    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        public void ShouldReturnDifferenceBetweenDictionaries()
        {
            var dic1 = new Dictionary<string, string> {{ "key1", "value1" }, { "key2", "value2" }, { "key3", "value3" }};
            var dic2 = new Dictionary<string, string> {{ "key1", "newValue1" }, { "key2", "value2" }, { "key3", "newValue3" }};
            var dic3 = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" }, { "key3", "value3" } };

            var differences12 = Utilities.GetDifferencesBetweenDictionaries(dic1, dic2).ToArray();
            var differences13 = Utilities.GetDifferencesBetweenDictionaries(dic1, dic3).ToArray();

            Assert.IsTrue(differences12.Count() == 2);
            Assert.IsTrue(differences12.Any(d=>d.FieldName == "key1" && d.OldValue == "value1" && d.NewValue == "newValue1"));
            Assert.IsTrue(differences12.Any(d => d.FieldName == "key3" && d.OldValue == "value3" && d.NewValue == "newValue3"));
            Assert.IsTrue(!differences13.Any());

        }
    }
}
