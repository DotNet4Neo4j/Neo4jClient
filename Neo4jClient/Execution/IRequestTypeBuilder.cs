using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
