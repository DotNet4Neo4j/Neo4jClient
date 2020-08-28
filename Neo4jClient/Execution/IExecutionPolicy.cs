using System;
using System.Collections.Generic;

namespace Neo4jClient.Execution
{
    internal interface IExecutionPolicy
    {
        bool InTransaction { get; }
        TransactionExecutionPolicy TransactionExecutionPolicy { get; }
        void AfterExecution(IDictionary<string, object> executionMetadata, object executionContext);
        string SerializeRequest(object toSerialize);
        Uri BaseEndpoint(string database, bool autoCommit);
        Uri AddPath(Uri startUri, object startReference);
        string Database { get; set; }
    }

    internal enum TransactionExecutionPolicy
    {
        Allowed,
        Denied,
        Required
    }
}