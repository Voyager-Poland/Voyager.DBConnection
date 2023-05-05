using Voyager.DBConnection.Events;

namespace Voyager.DBConnection.Interfaces
{
	public interface IRegisterEvents
	{
		void AddEvent(Action<SqlCallEvent> logEvent);
		void RemoveEvent(Action<SqlCallEvent> logEvent);
	}
}
