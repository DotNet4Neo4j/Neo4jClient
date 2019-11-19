using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Neo4jClient.Execution
{
    internal interface IResponseBuilder
    {
        IResponseBuilder WithExpectedStatusCodes(params HttpStatusCode[] statusCodes);
        IResponseFailBuilder FailOnCondition(Func<HttpResponseMessage, bool> condition);
        Task<HttpResponseMessage> ExecuteAsync();
        Task<HttpResponseMessage> ExecuteAsync(string commandDescription);
        Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>, HttpResponseMessage> continuationFunction);
        Task<HttpResponseMessage> ExecuteAsync(string commandDescription, Func<Task<HttpResponseMessage>, HttpResponseMessage> continuationFunction);
        

        Task<TExpected> ExecuteAsync<TExpected>(Func<Task<HttpResponseMessage>, TExpected> continuationFunction);
        Task<TExpected> ExecuteAsync<TExpected>(string commandDescription, Func<Task<HttpResponseMessage>, TExpected> continuationFunction);

        HttpResponseMessage Execute();
        HttpResponseMessage Execute(string commandDescription);
        IResponseBuilder<TParse> ParseAs<TParse>() where TParse : new();
    }

    internal interface IResponseFailBuilder
    {
        IResponseBuilder WithError(Func<HttpResponseMessage, Exception> errorBuilder);
        IResponseBuilder WithNull();
    }
}