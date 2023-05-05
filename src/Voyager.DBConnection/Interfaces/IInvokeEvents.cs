using Voyager.DBConnection.Events;

namespace Voyager.DBConnection.Interfaces
{
	internal interface IInvokeEvents
	{
		void Invoke(SqlCallEvent callEvent);
	}
}
