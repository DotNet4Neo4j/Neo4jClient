
namespace Neo4jClient.Test
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
