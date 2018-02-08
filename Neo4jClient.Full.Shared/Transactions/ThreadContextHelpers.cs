using System.Collections.Generic;
using System.Threading;

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

    internal interface IScopedTransactions<T> where T:class
    {
        int Count { get; }
        bool HasValue { get; }
        void Push(T item);
        T Pop();
        T Peek();
    }

#if NET45
        internal class ThreadContextWrapper<T> 
            : IScopedTransactions<T> 
            where T:class
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

            public T Pop()
            {
                return _stack.Pop();
            }

            public void Push(T item)
            {
                _stack.Push(item);
            }
        }
#else
    internal class ThreadContextWrapper<T> 
        : IScopedTransactions<T> 
        where T:class
    {
        private static AsyncLocal<Stack<T>> asyncLocal;

        public ThreadContextWrapper()
        {
            asyncLocal = new AsyncLocal<Stack<T>> {Value = new Stack<T>()};
        }

        public int Count => asyncLocal.Value?.Count ?? 0;
        public bool HasValue => asyncLocal.Value != null;

        public T Peek()
        {
            return asyncLocal.Value?.Peek();
        }

        public T Pop()
        {
            return asyncLocal.Value?.Pop();
        }

        public void Push(T item)
        {
            asyncLocal.Value.Push(item);
        }
    }
#endif
}