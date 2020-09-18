using System;

namespace Neo4jClient.Tests
{
    public class MockResponseThrowsException : Exception
    {
        public MockResponseThrowsException()
            : base("Murphy's law")
        {
            // Nothing more to do
        }
    }
}
