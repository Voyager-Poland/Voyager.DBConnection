using System.Data;
using System.Data.Common;
using Moq;
using Voyager.DBConnection.MockServcie;

namespace Voyager.DBConnection.Test
{
	class ConnectionExceptionPolicy : Interfaces.IExceptionPolicy
	{
		Connection connection;
		private bool spy;

		[SetUp]
		public void ConSetup()
		{
			var factory = new DbProviderFactoryMock();
			var dbMock = new Mock<Database>("Data Source=mockSql; Initial Catalog=TestDB; Integrated Security = true;", factory) { CallBase = true };
			dbMock.Setup(d => d.GetStoredProcCommand(It.IsAny<string>()))
				.Returns((string name) =>
				{
					var cmd = new MockDbCommand();
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = name;
					return cmd;
				});
			dbMock.Setup(d => d.GetSqlCommand(It.IsAny<string>()))
				.Returns((string sql) =>
				{
					var cmd = new MockDbCommand();
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = sql;
					return cmd;
				});
			connection = new Connection(dbMock.Object, this);
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
			string cmdTest = "mockCmd";
#pragma warning disable CS8765
			public override string CommandText { get => cmdTest; set => cmdTest = value; }
#pragma warning restore CS8765
			public override int CommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override CommandType CommandType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override bool DesignTimeVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public override UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			protected override DbConnection DbConnection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			protected override DbParameterCollection DbParameterCollection => throw new NotImplementedException();

			protected override DbTransaction DbTransaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public override void Cancel()
			{
				throw new NotImplementedException();
			}

			public override int ExecuteNonQuery()
			{
				throw new NotImplementedException();
			}

			public override object ExecuteScalar()
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


	abstract class ConnectionTest
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
			var factory = new DbProviderFactoryMock();
			var dbMock = new Mock<Database>("Data Source=mockSql; Initial Catalog=TestDB; Integrated Security = true;", factory) { CallBase = true };
			dbMock.Setup(d => d.GetStoredProcCommand(It.IsAny<string>()))
				.Returns((string name) =>
				{
					var cmd = new MockDbCommand();
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = name;
					return cmd;
				});
			dbMock.Setup(d => d.GetSqlCommand(It.IsAny<string>()))
				.Returns((string sql) =>
				{
					var cmd = new MockDbCommand();
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = sql;
					return cmd;
				});
			return new Connection(dbMock.Object);
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
