using System;
using System.Collections.Specialized;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Represents the same interface as <c>ITransaction</c>, however this interface is used
    /// internally to manage the properties that requires interaction with the Neo4j HTTP interface
    /// </summary>
    internal interface INeo4jTransaction : ITransaction
    {
        /// <summary>
        /// The Neo4j base endpoint for this transaction
        /// </summary>
        Uri Endpoint { get; set; }
    }
}
