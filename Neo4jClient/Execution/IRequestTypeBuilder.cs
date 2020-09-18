using System;
using System.Collections.Specialized;

namespace Neo4jClient.Execution
{
    internal interface IRequestTypeBuilder
    {
        IResponseBuilder Delete(Uri endpoint);
        IResponseBuilder Get(Uri endpoint);
        IRequestWithPendingContentBuilder Post(Uri endpoint);
        IRequestWithPendingContentBuilder Put(Uri endpoint);
    }
}
