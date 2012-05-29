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
            var dic1 = new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value2"}, {"key3", "value3"}};

            var dic2 = new Dictionary<string, string> {{"key1", "foo"}, {"key2", "value2"}, {"key3", "bar"}};

            var differences = Utilities.GetDifferencesBetweenDictionaries(dic1, dic2);



            //Assert.AreEqual(client, ((IGremlinQuery)node).Client);
        }
    }
}
