using System;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class FactoryTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfConfigurationIsNotDefined()
        {
            new GraphClientFactory(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionIfRootApiIsNotDefined()
        {
            using (var testHarness = new RestTestHarness())
            {
                var config = NeoServerConfiguration.GetConfiguration(new Uri(testHarness.BaseUri));

                config.ApiConfig = null;

                new GraphClientFactory(config);
            }
        }
    }
}
