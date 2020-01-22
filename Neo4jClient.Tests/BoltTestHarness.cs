using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Moq;
using Neo4j.Driver;
using Neo4jClient.Transactions;

namespace Neo4jClient.Tests
{
    public class BoltTestHarness : IDisposable
    {
        public BoltTestHarness()
        {
            MockSession
                .Setup(s => s.RunAsync("CALL dbms.components()"))
                .Returns(Task.FromResult<IResultCursor>(new BoltGraphClientTests.BoltGraphClientTests.ServerInfo()));
            MockSession
                .Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Throws(new Exception("Should never use synchronous method"));
            MockSession
                .Setup(s => s.WriteTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task>>()))
                .Throws(new Exception("Should never use synchronous method"));
            MockSession
                .Setup(s => s.ReadTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task>>()))
                .Throws(new Exception("Should never use synchronous method"));

            MockDriver
                .Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>()))
                .Returns(MockSession.Object);
        }

       


        public Mock<IDriver> MockDriver { get; } = new Mock<IDriver>();
        public Mock<IAsyncSession> MockSession { get; } = new Mock<IAsyncSession>();

        public void Dispose()
        {
        }
        //
        // public static readonly Func<SessionConfigBuilder> SessionConfigBuilderCreator = 
        //     Expression.Lambda<Func<SessionConfigBuilder>>(Expression.New(typeof(SessionConfigBuilder).GetConstructor(Type.EmptyTypes))).Compile();
        //
        // public static SessionConfigBuilder GetConfigBuilder()
        // {
        //     SessionConfigBuilder builder = (SessionConfigBuilder) Activator.CreateInstance(typeof(SessionConfigBuilder));
        //     return builder;
        // }

        public void SetupCypherRequestResponse(string request, IDictionary<string, object> cypherQueryQueryParameters, IResultCursor response)
        {
            MockSession.Setup(s => s.RunAsync(request, It.IsAny<IDictionary<string, object>>())).Returns(Task.FromResult(response));
            MockSession.Setup(s => s.WriteTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task<IResultCursor>>>())).Returns(Task.FromResult(response));
        }

        public async Task<IRawGraphClient> CreateAndConnectBoltGraphClient()
        {
            var bgc = new BoltGraphClient(MockDriver.Object);
            await bgc.ConnectAsync();
            MockDriver.Reset();
            return bgc;
        }
    }
}