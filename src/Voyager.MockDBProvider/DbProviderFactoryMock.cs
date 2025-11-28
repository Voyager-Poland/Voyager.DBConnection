//using System.Data;
//using System.Data.Common;

//namespace Voyager.DBConnection.Test
//{
//	internal class DbProviderFactoryMock : DbProviderFactory
//	{
//		public override DbConnection? CreateConnection()
//		{
//			return new MockConnection();
//		}

//		public override DbCommand? CreateCommand()
//		{
//			return new MockDbCommand();
//		}

//		public override DbParameter? CreateParameter()
//		{
//			return new MockDbParameter();
//		}
//	}

//	class MockDbParameter : DbParameter
//	{
//		public override DbType DbType { get; set; }
//		public override ParameterDirection Direction { get; set; }
//		public override bool IsNullable { get; set; }
//		public override string ParameterName { get; set; }
//		public override int Size { get; set; }
//		public override string SourceColumn { get; set; }
//		public override bool SourceColumnNullMapping { get; set; }
//		public override object? Value { get; set; }

//		public override void ResetDbType()
//		{
//			throw new NotImplementedException();
//		}
//	}
//	class MockConnection : DbConnection
//	{
//		public override string ConnectionString { get; set; }

//		public override string Database => "Mock";

//		public override string DataSource => "Mock";

//		public override string ServerVersion => "Mock";

//		public override ConnectionState State => state;

//		public override void ChangeDatabase(string databaseName)
//		{
//			throw new NotImplementedException();
//		}
//		ConnectionState state = ConnectionState.Connecting;
//		public override void Close()
//		{
//			state = ConnectionState.Closed;
//		}

//		public override void Open()
//		{
//			state = ConnectionState.Open;
//		}

//		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
//		{
//			return new MockDbTransaction(this, isolationLevel);
//		}

//		protected override DbCommand CreateDbCommand()
//		{
//			throw new NotImplementedException();
//		}
//	}

//	class MockDbTransaction : DbTransaction
//	{
//		public MockDbTransaction(DbConnection dbConnection, IsolationLevel isolationLevel)
//		{
//			DbConnection = dbConnection;
//			IsolationLevel = isolationLevel;
//		}
//		public override IsolationLevel IsolationLevel { get; }

//		protected override DbConnection? DbConnection { get; }

//		public override void Commit()
//		{
//			throw new NotImplementedException();
//		}

//		public override void Rollback()
//		{
//			throw new NotImplementedException();
//		}
//	}
//}
