using System;
using System.Collections.Generic;
using System.Transactions;

namespace MicroORM.DataAccess
{
	internal class Transaction : ITransaction
	{
		private readonly ISession _session;
		private bool _disposed;
		private bool isTransactionCommitted = false;
		private bool isTransactionRolledBack = false;
		private TransactionScope txn;
		private readonly List<Action> _dataMutations;

		public Transaction(ISession session)
		{
			this._session = session;
			this._dataMutations = new List<Action>();

			if (System.Transactions.Transaction.Current != null)
			{
				this.txn = new TransactionScope(System.Transactions.Transaction.Current);
			}
			else
			{
				this.txn = new TransactionScope();
			}
		}

		public void Enqueue(Action dataMutation)
		{
			GuardOnDisposed();
			this._dataMutations.Add(dataMutation);
		}

		public void Rollback()
		{
			this.isTransactionRolledBack = true;
		}

		public void Commit()
		{
			GuardOnDisposed();

			if (this.isTransactionCommitted == true || this.isTransactionRolledBack == true) return;

			this.isTransactionCommitted = true;

			this._dataMutations.ForEach(mutation => mutation.Invoke());

			this.txn.Complete();
		}

		public void Dispose()
		{
			this.Disposing(true);
			GC.SuppressFinalize(this);
		}

		private void Disposing(bool disposing)
		{
			if (disposing == true)
			{
				this.Commit();

				if (this._dataMutations != null)
				{
					this._dataMutations.Clear();
				}

				if (this.txn != null)
				{
					this.txn.Dispose();
				}
				this.txn = null;
			}

			this._disposed = true;
		}

		private void GuardOnDisposed()
		{
			if (this._disposed)
				throw new ObjectDisposedException("Can not access a disposed Transaction instance for the Session.");
		}

		~Transaction()
		{
			this.Disposing(true);
		}
	}
}