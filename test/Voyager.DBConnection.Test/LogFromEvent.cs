using Voyager.DBConnection.Events;

namespace Voyager.DBConnection.Test
{
	internal class LogFromEvent : BasicCall
	{
		SqlCallEvent? SqlEvent;

		[Test]
		public override void WaitFor()
		{
			base.WaitFor();
			Assert.That(SqlEvent, Is.Not.Null);
		}

		protected override Connection GetConnection()
		{
			var connection = base.GetConnection();
			connection.AddEvent(Connection_SqlCallEvent);
			return connection;
		}

		private void Connection_SqlCallEvent(Events.SqlCallEvent obj)
		{
			SqlEvent = obj;
		}
	}

}