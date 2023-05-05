using System.Data.Common;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	public class Transaction : IDisposable
	{
		readonly DbTransaction dbTransaction;
		internal readonly ITransactionOwner owner;
		internal Transaction(DbTransaction dbTransaction, ITransactionOwner owner)
		{
			this.dbTransaction = dbTransaction;
			this.owner = owner;
		}

		public void Commit()
		{
			dbTransaction.Commit();
			RelaseTransaction();
		}

		public void Rollback()
		{
			dbTransaction.Rollback();
			RelaseTransaction();
		}

		public void Dispose()
		{
			RelaseTransaction();
		}

		void RelaseTransaction()
		{
			try
			{
				dbTransaction?.Dispose();
			}
			catch { }

			try
			{
				owner?.ResetTransaction();
			}
			catch { }
		}

		internal DbTransaction? GetTransaction()
		{
			return dbTransaction;
		}
	}
}
