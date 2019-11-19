
namespace Neo4jClient.Tests
{
    public class MockResponseThrows : MockResponse
    {
        public override string Content
        {
            get
            {
                throw new MockResponseThrowsException();
            }
            set
            {
                throw new MockResponseThrowsException();
            }
        }
    }
}
