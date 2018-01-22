using System.Collections.Generic;
using System.Linq;
using Neo4jClient.Test.Fixtures;
using Xunit;
using Xunit;

namespace Neo4jClient.Test
{
    
    public class UtilitiesTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldReturnDifferenceBetweenDictionaries()
        {
            var dic1 = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" }, { "key3", "value3" }};
            var dic2 = new Dictionary<string, string> { { "key1", "newValue1" }, { "key2", "value2" }, { "key3", "newValue3" }};
            var dic3 = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" }, { "key3", "value3" } };
            var dic4 = new Dictionary<string, string> { { "key2", "value2" }, { "key3", "newValue3" } };


            var differences12 = Utilities.GetDifferencesBetweenDictionaries(dic1, dic2).ToArray();
            var differences13 = Utilities.GetDifferencesBetweenDictionaries(dic1, dic3).ToArray();
            var differences14 = Utilities.GetDifferencesBetweenDictionaries(dic1, dic4).ToArray();

            Assert.True(differences12.Count() == 2);
            Assert.True(differences12.Any(d=>d.FieldName == "key1" && d.OldValue == "value1" && d.NewValue == "newValue1"));
            Assert.True(differences12.Any(d => d.FieldName == "key3" && d.OldValue == "value3" && d.NewValue == "newValue3"));
            Assert.True(!differences13.Any());
            Assert.True(differences14.Any(d => d.FieldName == "key1" && d.OldValue == "value1" && d.NewValue == ""));
            Assert.True(differences14.Any(d => d.FieldName == "key3" && d.OldValue == "value3" && d.NewValue == "newValue3"));
        }
    }
}
