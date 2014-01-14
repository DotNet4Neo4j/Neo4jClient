using System;
using System.Threading;
using System.Threading.Tasks;
using Neo4jClient.Transactions;
using NUnit.Framework;

namespace Neo4jClient.Test.Transactions
{
    [TestFixture]
    public class TransactionManagementTests
    {
        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        public void BeginTransactionShouldFailWithLower20Versions()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateGraphClient(false);
                client.Connect();
                client.BeginTransaction();
            }
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void BeginTransactionShouldFailWithoutConnectingFirst()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"), null);
            client.BeginTransaction();
        }

        [Test]
        public void ShouldBeAbleToGetTransactionObjectAfterBeginTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.Connect();
                using (var transaction = client.BeginTransaction())
                {
                    Assert.AreSame(transaction, client.Transaction);
                }
            }
        }

        [Test]
        public void ShouldNotBeAbleToGetTransactionAfterTransactionScope()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {

                }

                Assert.IsNull(client.Transaction);
            }
        }

        [Test]
        [ExpectedException(typeof (ClosedTransactionException))]
        public void ShouldNotBeAbleToCommitTwice()
        {
            var transaction = new Transaction(new GraphClient(new Uri("http://foo/db/data")), null);
            transaction.Commit();
            transaction.Commit();
        }

        [Test]
        [ExpectedException(typeof (ClosedTransactionException))]
        public void ShouldNotBeAbleToRollbackTwice()
        {
            var transaction = new Transaction(new GraphClient(new Uri("http://foo/db/data")), null);
            transaction.Rollback();
            transaction.Rollback();
        }

        [Test]
        [ExpectedException(typeof (ClosedTransactionException))]
        public void ShouldNotBeAbleToCommitAfterRollback()
        {
            var transaction = new Transaction(new GraphClient(new Uri("http://foo/db/data")), null);
            transaction.Rollback();
            transaction.Commit();
        }

        [Test]
        [ExpectedException(typeof (ClosedTransactionException))]
        public void ShouldNotBeAbleToRollbackAfterCommit()
        {
            var transaction = new Transaction(new GraphClient(new Uri("http://foo/db/data")), null);
            transaction.Commit();
            transaction.Rollback();
        }

        [Test]
        public void TwoThreadsShouldNotHaveTheSameTransactionObject()
        {
            // if thread support is not well implemented then the t2's BeginTransaction will fail with NotSupportedException
            ITransaction transactionFromThread1 = null;
            ITransaction transactionFromThread2 = null;
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                var firstTransactionSet = new EventWaitHandle(false, EventResetMode.AutoReset);
                var secondTransactionSet = new EventWaitHandle(false, EventResetMode.AutoReset);
                var t1 = new Task(() =>
                {
                    try
                    {
                        using (var transaction = client.BeginTransaction())
                        {
                            transactionFromThread1 = transaction;
                            firstTransactionSet.Set();
                            secondTransactionSet.WaitOne();
                        }
                    }
                    catch (Exception e)
                    {
                        firstTransactionSet.Set();
                        throw;
                    }
                });

                var t2 = new Task(() =>
                {
                    firstTransactionSet.WaitOne();
                    try
                    {
                        using (var transaction = client.BeginTransaction())
                        {
                            transactionFromThread2 = transaction;
                            secondTransactionSet.Set();
                        }
                    }
                    catch (Exception e)
                    {
                        secondTransactionSet.Set();
                        throw;
                    }
                });

                t1.Start();
                t2.Start();
                Task.WaitAll(t1, t2);
                Assert.IsNotNull(transactionFromThread1);
                Assert.IsNotNull(transactionFromThread2);
                Assert.AreNotEqual(transactionFromThread1, transactionFromThread2);

            }
        }
    }
}
