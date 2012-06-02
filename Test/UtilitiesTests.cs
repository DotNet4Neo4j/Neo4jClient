using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Neo4jClient.Test
{
    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        public void ShouldReturnDifferenceBetweenDictionaries()
        {
            var dic1 = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" }, { "key3", "value3" }};
            var dic2 = new Dictionary<string, string> { { "key1", "newValue1" }, { "key2", "value2" }, { "key3", "newValue3" }};
            var dic3 = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" }, { "key3", "value3" } };
            var dic4 = new Dictionary<string, string> { { "key2", "value2" }, { "key3", "newValue3" } };


            var differences12 = Utilities.GetDifferencesBetweenDictionaries(dic1, dic2).ToArray();
            var differences13 = Utilities.GetDifferencesBetweenDictionaries(dic1, dic3).ToArray();
            var differences14 = Utilities.GetDifferencesBetweenDictionaries(dic1, dic4).ToArray();

            Assert.IsTrue(differences12.Count() == 2);
            Assert.IsTrue(differences12.Any(d=>d.FieldName == "key1" && d.OldValue == "value1" && d.NewValue == "newValue1"));
            Assert.IsTrue(differences12.Any(d => d.FieldName == "key3" && d.OldValue == "value3" && d.NewValue == "newValue3"));
            Assert.IsTrue(!differences13.Any());
            Assert.IsTrue(differences14.Any(d => d.FieldName == "key1" && d.OldValue == "value1" && d.NewValue == ""));
            Assert.IsTrue(differences14.Any(d => d.FieldName == "key3" && d.OldValue == "value3" && d.NewValue == "newValue3"));
        }
    }
}
