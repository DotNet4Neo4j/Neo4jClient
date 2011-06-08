using System;
using Neo4jClient;
using NUnit.Framework;

namespace Test.GraphClientTests
{
    [TestFixture]
    public class CreateNodeTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionForNullNode()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.Create<object>(null);
        }
    }
}