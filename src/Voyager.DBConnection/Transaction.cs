using System;

namespace Voyager.DBConnection
{
	public class Transaction : IDisposable
	{
		private readonly TransactionHolder _holder;
		private readonly Action _onDispose;
		private bool _disposed;

		internal Transaction(TransactionHolder holder, Action onDispose)
		{
			_holder = holder ?? throw new ArgumentNullException(nameof(holder));
			_onDispose = onDispose;
		}

		public void Commit()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(Transaction));

			_holder.Commit();
		}

		public void Rollback()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(Transaction));

			_holder.Rollback();
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_holder.Dispose();
				_onDispose?.Invoke();
				_disposed = true;
			}
		}

		internal System.Data.Common.DbTransaction GetTransaction() => _holder.Transaction;
	}
}
