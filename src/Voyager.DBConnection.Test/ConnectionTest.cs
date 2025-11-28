using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection.Test
{
	class ConnectionExceptionPolicy : Interfaces.IExceptionPolicy
	{
		Connection connection;
		private bool spy;

		[SetUp]
		public void ConSetup()
		{
			connection = new Connection(new MockDataBase(), this);
		}

		[TearDown]
		public void ConTearDown()
		{
			spy = false;
			connection.Dispose();
		}

		[Test]
		public void ErrorPolicy()
		{
			try
			{
				this.connection.ExecuteScalar(new ErrorProc());
			}
			catch { }
			Assert.That(spy, Is.True);
		}

		public Exception GetException(Exception ex)
		{
			spy = true;
			return ex;
		}

		class ErrorProc : Interfaces.ICommandFactory
		{
			public DbCommand ConstructDbCommand(Database db)
			{
				return new ErrorCmd();
			}

			public void ReadOutParameters(Database db, DbCommand command)
			{

			}
		}

		class ErrorCmd : DbCommand
		{
			public override string CommandText { get ; set ; } = string.Empty;	
			public override int CommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override CommandType CommandType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override bool DesignTimeVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			protected override DbConnection? DbConnection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			protected override DbParameterCollection DbParameterCollection => throw new NotImplementedException();

			protected override DbTransaction? DbTransaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public override void Cancel()
			{
				throw new NotImplementedException();
			}

			public override int ExecuteNonQuery()
			{
				throw new NotImplementedException();
			}

			public override object? ExecuteScalar()
			{
				throw new NotImplementedException();
			}

			public override void Prepare()
			{
				throw new NotImplementedException();
			}

			protected override DbParameter CreateDbParameter()
			{
				throw new NotImplementedException();
			}

			protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
			{
				throw new NotImplementedException();
			}
		}
	}


	class ConnectionTest
	{
		Connection connection;

		[SetUp]
		public void ConSetUp()
		{
			connection = GetConn();
		}

		[TearDown]
		public void ConDispose()
		{
			connection.Dispose();
		}


		protected virtual Connection GetConn()
		{
			return new Connection(new MockDataBase());
		}

		[Test]
		public void BeginTran()
		{
			var trans = connection.BeginTransaction().GetTransaction();
			Assert.That(trans.IsolationLevel, Is.EqualTo(System.Data.IsolationLevel.ReadCommitted));
			Assert.That(trans.Connection, Is.Not.Null);
			Assert.That(trans.Connection.State, Is.EqualTo(System.Data.ConnectionState.Open));
		}

		[Test]
		public void ExecutionNonQuery()
		{
			Assert.That(connection.ExecuteNonQuery(new CmdFactory()), Is.EqualTo(0));
		}


		[Test]
		public async Task ExecutionNonQueryAsync()
		{
			Assert.That(await connection.ExecuteNonQueryAsync(new CmdFactory()), Is.EqualTo(0));
		}

		[Test]
		public virtual void DataReader()
		{
			Assert.That(connection.GetReader(new CmdFactory(), new CmdReader()), Is.EqualTo(1));
		}

		[Test]
		public virtual void ExecuteScalar()
		{
			Assert.That(connection.ExecuteScalar(new CmdFactory()), Is.Not.Null);
		}
		[Test]
		public virtual async Task ExecuteScalarAsync()
		{
			Assert.That(await connection.ExecuteScalarAsync(new CmdFactory(), CancellationToken.None), Is.Not.Null);
		}
	}

	class CmdFactory : Interfaces.ICommandFactory
	{
		public DbCommand ConstructDbCommand(Database db)
		{
			return db.GetStoredProcCommand("run");
		}

		public void ReadOutParameters(Database db, DbCommand command)
		{

		}
	}

	class CmdReader : Interfaces.IGetConsumer<int>
	{
		public int GetResults(IDataReader dataReader)
		{
			return 1;
		}
	}
}
