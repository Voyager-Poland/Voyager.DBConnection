using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;


namespace Voyager.DBConnection
{
	internal class EventHost : IInvokeEvents, IRegisterEvents
	{
		event Action<Events.SqlCallEvent>? SqlCallEvent;

		void IInvokeEvents.Invoke(SqlCallEvent callEvent)
		{
			if (this.SqlCallEvent != null)
				this.SqlCallEvent.Invoke(callEvent);
		}

		public void AddEvent(Action<SqlCallEvent> logEvent)
		{
			SqlCallEvent += logEvent;
		}

		public void RemoveEvent(Action<SqlCallEvent> logEvent)
		{
			SqlCallEvent -= logEvent;
		}
	}
}
