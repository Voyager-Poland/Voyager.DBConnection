using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.Test
{
	class ConnectionFeature : ConnectionTest
	{
		protected override Connection GetConn()
		{
			var conn = base.GetConn();
			conn.AddFeature(new CountAllRequest(conn));
			return conn;
		}

		[TearDown]
		public void Monitor()
		{
			Console.Out.WriteLine($"Aktualna liczba wywołań {CountAllRequest.AllRequestCount}");
		}

		[Test]
		public override void DataReader()
		{
			int before = CountAllRequest.AllRequestCount;
			base.DataReader();
			Assert.That(CountAllRequest.AllRequestCount, Is.EqualTo(before + 1));
		}

		class CountAllRequest : Voyager.DBConnection.Interfaces.IFeature
		{
			public static int AllRequestCount = 0;
			private IRegisterEvents registerEvents;

			public CountAllRequest(IRegisterEvents registerEvents)
			{
				this.registerEvents = registerEvents;
				this.registerEvents.AddEvent(MyHandler);
			}

			private void MyHandler(SqlCallEvent @event)
			{
				AllRequestCount++;
			}

			public void Dispose()
			{
				Console.Out.WriteLine("Zwolnione połączenie");
				registerEvents.RemoveEvent(MyHandler);
			}
		}
	}
}
