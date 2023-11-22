using System.Data.Common;
using Voyager.UnitTestLogger;

namespace Voyager.DBConnection.Logger.Test
{
	abstract class LoggerTest
	{
		Connection connection;
		SpyLog<LoggerTest> spyLog;

		[SetUp]
		public void Setup()
		{
			spyLog = new SpyLog<LoggerTest>();

			connection = new Connection(new Voyager.DBConnection.Test.MockDataBase());
			connection.AddLogger(spyLog);
		}

		[TearDown]
		public void TearDown()
		{
			connection.Dispose();

		}

		[Test]
		public void SpyTest()
		{
			connection.ExecuteNonQuery(new MockProc());

			Assert.That(spyLog.GetSpyContent(), Is.Not.Empty);
		}

		class MockProc : Interfaces.ICommandFactory
		{
			public DbCommand ConstructDbCommand(Database db)
			{
				return db.GetSqlCommand("BAM!");
			}

			public void ReadOutParameters(Database db, DbCommand command)
			{

			}
		}
	}
}