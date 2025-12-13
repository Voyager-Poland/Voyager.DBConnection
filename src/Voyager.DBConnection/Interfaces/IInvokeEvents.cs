using Voyager.DBConnection.Events;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Defines a mechanism for invoking event handlers for SQL call events.
	/// </summary>
	internal interface IInvokeEvents
	{
		/// <summary>
		/// Invokes registered event handlers with the specified SQL call event.
		/// </summary>
		/// <param name="callEvent">The SQL call event to invoke handlers with.</param>
		void Invoke(SqlCallEvent callEvent);
	}
}
