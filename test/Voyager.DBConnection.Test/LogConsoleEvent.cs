//namespace Voyager.DBConnection.Test
//{
//	internal class LogConsoleEvent : BasicCall
//	{

//		Voyager.UnitTestLogger.SpyLog<LogConsoleEvent>? logger;

//		protected override void Prepare()
//		{
//			logger = new UnitTestLogger.SpyLog<LogConsoleEvent>();
//		}

//		protected override Connection GetConnection()
//		{
//			var connection = base.GetConnection();
//			connection.AddLogger(logger!);
//			return connection;
//		}


//		[Test]
//		public override void WaitFor()
//		{
//			base.WaitFor();

//			Assert.That(logger?.GetLinesCount(), Is.EqualTo(1));

//		}
//	}

//}