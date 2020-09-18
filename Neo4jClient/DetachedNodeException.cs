using System;

namespace Neo4jClient
{
    public class DetachedNodeException : Exception
    {
        public DetachedNodeException()
            : base(
            "A detached node is being used as the base node of a query. That is, the " +
            "node reference was created independently. To execute queries against nodes, " +
            "the node reference must have a reference back to an instance of IGraphClient. " +
            "The simplest way to achieve this is to retrieve the node from IGraphClient " +
            "in the first place, then query against it. Alternatively, you can suppy an " +
            "IGraphClient instance to the node reference at time of construction.")
        {
        }
    }
}