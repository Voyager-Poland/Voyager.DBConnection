using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection.Test
{
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
