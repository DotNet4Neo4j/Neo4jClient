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
            new GraphClient().Create<object>(null);
        }
    }
}