using System.Data;
using Moq;
using Voyager.DBConnection.MockServcie;

namespace Voyager.DBConnection.Test
{

	class DataBaseTest
	{
		Database database;

		[SetUp]
		public void PrepareDB()
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
			database = dbMock.Object;
		}

		[TearDown]
		public void CleanupDB()
		{
			database?.Dispose();
		}

		[Test]
		public void BeginTran()
		{
			var trans = database.BeginTransaction().GetTransaction();
			Assert.That(trans.IsolationLevel, Is.EqualTo(System.Data.IsolationLevel.ReadCommitted));
			Assert.That(trans.Connection, Is.Not.Null);
			Assert.That(trans.Connection.State, Is.EqualTo((System.Data.ConnectionState.Open)));
		}

		[Test]
		public void PrepareStoredProc()
		{
			var cmd = database.GetStoredProcCommand("cmdTxt");
			Assert.That(cmd.CommandText, Is.EqualTo("cmdTxt"));
			Assert.That(cmd.CommandType, Is.EqualTo(System.Data.CommandType.StoredProcedure));
		}

		[Test]
		public void PrepareSqlTxt()
		{
			var cmd = database.GetSqlCommand("sqlTxt");
			Assert.That(cmd.CommandText, Is.EqualTo("sqlTxt"));
			Assert.That(cmd.CommandType, Is.EqualTo(System.Data.CommandType.Text));
		}

		[Test]
		public void AddInParameter()
		{
			var cmd = database.GetStoredProcCommand("cmdTxt");

			cmd.WithInputParameter("ala", System.Data.DbType.Int32, 4);
			var param = cmd.Parameters["ala"];
			Assert.That(param.DbType, Is.EqualTo(System.Data.DbType.Int32));
			Assert.That(param.Value, Is.EqualTo(4));
			Assert.That(param.Direction, Is.EqualTo(ParameterDirection.Input));
		}

		[Test]
		public void AddOutParameter()
		{
			var cmd = database.GetStoredProcCommand("cmdTxt");

			cmd.WithOutputParameter("ala", System.Data.DbType.AnsiString, 200);
			var param = cmd.Parameters["ala"];
			Assert.That(param.DbType, Is.EqualTo(System.Data.DbType.AnsiString));
			Assert.That(param.Size, Is.EqualTo(200));
			Assert.That(param.Direction, Is.EqualTo(ParameterDirection.Output));
		}

		[Test]
		public void AddOutOutParameter()
		{
			var cmd = database.GetStoredProcCommand("cmdTxt");

			cmd.WithInputOutputParameter("ala", System.Data.DbType.AnsiString, "Ziemia");
			var param = cmd.Parameters["ala"];
			Assert.That(param.DbType, Is.EqualTo(System.Data.DbType.AnsiString));
			Assert.That(param.Value, Is.EqualTo("Ziemia"));
			Assert.That(param.Direction, Is.EqualTo(ParameterDirection.InputOutput));

		}

		[Test]
		public void GetParamValue()
		{
			var cmd = database.GetStoredProcCommand("cmdTxt");

			cmd.WithOutputParameter("ala", System.Data.DbType.AnsiString, 40);

			var param = cmd.Parameters["ala"];
			param.Value = "Ma kota";
			Assert.That(cmd.GetParameterValue("ala"), Is.EqualTo("Ma kota"));
		}



		[Test]
		public void OpenConn()
		{
			var cmd = database.GetStoredProcCommand("cmdTxt");
			database.OpenCmd(cmd);
			Assert.That(cmd.Connection, Is.Not.Null);
			Assert.That(cmd.Connection.State, Is.EqualTo(System.Data.ConnectionState.Open));
			Assert.That(cmd.Transaction, Is.Null);
		}

		[Test]
		public void OpenConnTransactiom()
		{
			database.BeginTransaction();
			var cmd = database.GetStoredProcCommand("cmdTxt");
			database.OpenCmd(cmd);
			Assert.That(cmd.Connection, Is.Not.Null);
			Assert.That(cmd.Connection.State, Is.EqualTo(System.Data.ConnectionState.Open));
			Assert.That(cmd.Transaction, Is.Not.Null);
		}
	}
}
