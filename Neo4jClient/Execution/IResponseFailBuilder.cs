using System;
using System.Net.Http;

namespace Neo4jClient.Execution
{
    internal interface IResponseFailBuilder<TResult> where TResult : new()
    {
        IResponseBuilder<TResult> WithError(Func<HttpResponseMessage, Exception> errorBuilder);
        IResponseBuilder<TResult> WithDefault();
    }
}