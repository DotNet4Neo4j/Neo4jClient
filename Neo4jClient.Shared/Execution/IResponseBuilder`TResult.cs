using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Neo4jClient.Cypher;

namespace Neo4jClient.Execution
{
    internal interface IResponseBuilder<TResult> where TResult : new()
    {
        IResponseBuilder<TResult> WithExpectedStatusCodes(params HttpStatusCode[] statusCodes);
        IResponseFailBuilder<TResult> FailOnCondition(Func<HttpResponseMessage, bool> condition);
        Task<TResult> ExecuteAsync(string commandDescription);
        Task<TResult> ExecuteAsync(Func<Task<TResult>, TResult> continuationFunction);
        Task<TResult> ExecuteAsync(string commandDescription, Func<Task<TResult>, TResult> continuationFunction);
        Task<TResult> ExecuteAsync(string commandDescription,
            Func<Task<TResult>, TResult> continuationFunction, TaskFactory taskFactory);
        Task<TResult> ExecuteAsync();

        Task<TExpected> ExecuteAsync<TExpected>(Func<Task<TResult>, TExpected> continuationFunction);
        Task<TExpected> ExecuteAsync<TExpected>(string commandDescription, Func<Task<TResult>, TExpected> continuationFunction);
        Task<TExpected> ExecuteAsync<TExpected>(string commandDescription,
            Func<Task<TResult>, TExpected> continuationFunction, TaskFactory taskFactory);

        TResult Execute(string commandDescription, TaskFactory taskFactory);
        TResult Execute(string commandDescription);
        TResult Execute();
    }
}
