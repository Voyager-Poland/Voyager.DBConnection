using System;
using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection.MockServcie
{
	class MockDbTransaction : DbTransaction
	{
		public MockDbTransaction(DbConnection dbConnection, IsolationLevel isolationLevel)
		{
			DbConnection = dbConnection;
			IsolationLevel = isolationLevel;
		}
		public override IsolationLevel IsolationLevel { get; }

		protected override DbConnection DbConnection { get; }

		public override void Commit()
		{
			throw new NotImplementedException();
		}

		public override void Rollback()
		{
			throw new NotImplementedException();
		}
	}
}
