using System.Collections.Generic;

namespace Neo4jClient.Transactions
{
    internal static class ThreadContextHelper
    {
        internal static IScopedTransactions<TransactionScopeProxy> CreateScopedTransactions()
        {
            return new ThreadContextWrapper<TransactionScopeProxy>();
        }

        internal static IScopedTransactions<BoltTransactionScopeProxy> CreateBoltScopedTransactions()
        {
            return new ThreadContextWrapper<BoltTransactionScopeProxy>();
        }
    }

    internal interface IScopedTransactions<T> where T : class
    {
        int Count { get; }
        bool HasValue { get; }
        void Push(T item);
        T Pop();
        T TryPop();
        T Peek();
        T TryPeek();
    }

    internal class ThreadContextWrapper<T>
        : IScopedTransactions<T>
        where T : class
    {
        private readonly Stack<T> _stack;

        public ThreadContextWrapper()
        {
            _stack = new Stack<T>();
        }

        public int Count => _stack?.Count ?? 0;
        public bool HasValue => _stack != null;


        public T Peek()
        {
            return _stack.Peek();
        }

        public T TryPeek()
        {
            return _stack.Count > 0 ? _stack.Peek() : null;
        }

        public T Pop()
        {
            return _stack.Pop();
        }

        public T TryPop()
        {
            return _stack.Count > 0 ? _stack.Pop() : null;
        }

        public void Push(T item)
        {
            _stack.Push(item);
        }
    }
}