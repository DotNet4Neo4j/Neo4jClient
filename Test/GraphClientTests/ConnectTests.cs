using System;
using Neo4jClient;
using NUnit.Framework;

namespace Test.GraphClientTests
{
    [TestFixture]
    public class ConnectTests
    {
        [Test]
        public void ShouldThrowConnectionExceptionFor500Response()
        {
            var graphClient = new GraphClient(new Uri("http://foo"));
            graphClient.Connect();
        }
    }
}