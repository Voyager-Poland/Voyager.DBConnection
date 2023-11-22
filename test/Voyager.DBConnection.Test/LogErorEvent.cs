//namespace Voyager.DBConnection.Test
//{
//	internal class LogErorEvent : BasicCall
//	{

//		Voyager.UnitTestLogger.SpyLog<LogConsoleEvent>? logger;

//		protected override void Prepare()
//		{
//			logger = new UnitTestLogger.SpyLog<LogConsoleEvent>();
//		}

//		protected override Connection GetConnection()
//		{
//			var connection = new Connection(new Database("Data Source=blad; Initial Catalog=ProxyAuth; Integrated Security = true;", MSSqlDBProvider.GetSqlProvider()));
//			//	connection.AddLogger(logger!);
//			return connection;
//		}


//		[Test]
//		public override void WaitFor()
//		{
//			try
//			{
//				base.WaitFor();
//			}
//			catch { }
//			Assert.That(logger?.GetLinesCount(), Is.GreaterThan(2));

//		}
//	}

//}