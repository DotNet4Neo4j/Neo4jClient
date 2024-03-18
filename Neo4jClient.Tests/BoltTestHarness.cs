using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Neo4j.Driver;

namespace Neo4jClient.Tests
{
    public class TestRecord : IRecord{
        private IEnumerable<object> values;
        public T Get<T>(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(string key, out T value)
        {
            throw new NotImplementedException();
        }

        public T GetCaseInsensitive<T>(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetCaseInsensitive<T>(string key, out T value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public object this[int index] => throw new NotImplementedException();

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public object this[string key] => throw new NotImplementedException();

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => values;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, object> Values { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> Keys { get; }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get; }
    }

    public class BoltTestHarness : IDisposable
    {
        public BoltTestHarness()
        {
            var mockResultCursor = new Mock<IResultCursor>();
            
            var mockTransaction = new Mock<IAsyncTransaction>();
            mockTransaction
                .Setup(t => t.RunAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(Task.FromResult(mockResultCursor.Object));

            var mockSession = new Mock<IAsyncSession>(MockBehavior.Loose);
            mockSession
                .Setup(s => s.RunAsync("CALL dbms.components()", null))
                .Returns(Task.FromResult<IResultCursor>(new BoltGraphClientTests.BoltGraphClientTests.ServerInfo()));
            mockSession
                .Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>(), null))
                .Throws(new Exception("Should never use synchronous method"));
            
            mockSession
                .Setup(s => s.BeginTransactionAsync(It.IsAny<Action<TransactionConfigBuilder>>()))
                .Returns(Task.FromResult(mockTransaction.Object));
            
            mockSession
                .Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(mockTransaction.Object));

            var mockDriver = new Mock<IDriver>(MockBehavior.Strict);
            mockDriver
                .Setup(d => d.AsyncSession())
                .Returns(mockSession.Object);
            mockDriver
                .Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>()))
                .Returns(mockSession.Object);

            MockSession = mockSession;
            MockDriver = mockDriver;
        }


        public Mock<IDriver> MockDriver { get; }
        public Mock<IAsyncSession> MockSession { get; }

        public void Dispose()
        {
        }

        public void SetupCypherRequestResponse(string request, IDictionary<string, object> cypherQueryQueryParameters, IResultCursor response)
        {
            MockSession.Setup(s => s.RunAsync(request, It.IsAny<IDictionary<string, object>>(), null)).Returns(Task.FromResult(response));
            var mockTransaction = new Mock<IAsyncTransaction>();
            mockTransaction.Setup(s => s.RunAsync(request, It.IsAny<IDictionary<string, object>>())).Returns(Task.FromResult(response));
        
            MockSession
                .Setup(x => x.ExecuteReadAsync(It.IsAny<Func<IAsyncQueryRunner, Task<List<IRecord>>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
                .Returns<Func<IAsyncTransaction, Task<List<IRecord>>>, Action<TransactionConfigBuilder>>(async (func, config) => await func(mockTransaction.Object) );

            MockSession
                .Setup(x => x.ExecuteWriteAsync(It.IsAny<Func<IAsyncQueryRunner, Task<List<IRecord>>>>(), It.IsAny<Action<TransactionConfigBuilder>>()))
                .Returns<Func<IAsyncTransaction, Task<List<IRecord>>>, Action<TransactionConfigBuilder>>(async (func, config) => await func(mockTransaction.Object) );

            MockSession
                .Setup(s => s.BeginTransactionAsync(It.IsAny<Action<TransactionConfigBuilder>>()))
                .Returns(Task.FromResult(mockTransaction.Object));
            MockSession
                .Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(mockTransaction.Object));
        }
        public async Task<IRawGraphClient> CreateAndConnectBoltGraphClient()
        {
            var bgc = new BoltGraphClient(MockDriver.Object);
            await bgc.ConnectAsync();
            MockDriver.Invocations.Clear();
            return bgc;
        }
    }
}