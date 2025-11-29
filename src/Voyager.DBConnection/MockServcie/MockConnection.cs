using System;
using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection.MockServcie
{
	internal class MockConnection : DbConnection
	{
		public override string ConnectionString { get; set; } = "MockConnectionString";

		public override string Database => "Mock";

		public override string DataSource => "Mock";

		public override string ServerVersion => "Mock";

		public override ConnectionState State => state;

		public override void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}
		ConnectionState state = ConnectionState.Connecting;
		public override void Close()
		{
			state = ConnectionState.Closed;
		}

		public override void Open()
		{
			state = ConnectionState.Open;
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return new MockDbTransaction(this, isolationLevel);
		}

		protected override DbCommand CreateDbCommand()
		{
			throw new NotImplementedException();
		}
	}
}
