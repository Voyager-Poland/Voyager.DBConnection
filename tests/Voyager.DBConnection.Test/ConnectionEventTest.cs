using Voyager.DBConnection.Events;

namespace Voyager.DBConnection.Test
{
	class ConnectionEventTest : ConnectionTest
	{
		private bool logged;

		[TearDown]
		public void ClenLogged()
		{
			logged = false;
		}
		protected override Connection GetConn()
		{
			var conn = base.GetConn();
			conn.AddEvent(this.EventCall);
			return conn;
		}

		private void EventCall(SqlCallEvent @event)
		{
			Console.Out.WriteLine(@event);
			logged = true;
		}

		[Test]
		public override void ExecuteScalar()
		{
			base.ExecuteScalar();
			Assert.That(logged, Is.True);
		}
	}
}
