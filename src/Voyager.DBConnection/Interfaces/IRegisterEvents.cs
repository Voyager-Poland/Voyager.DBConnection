using System;
using Voyager.DBConnection.Events;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Defines a mechanism for registering and unregistering event handlers for SQL call events.
	/// </summary>
	public interface IRegisterEvents
	{
		/// <summary>
		/// Registers an event handler to be invoked when SQL operations occur.
		/// </summary>
		/// <param name="logEvent">The event handler to register.</param>
		void AddEvent(Action<SqlCallEvent> logEvent);

		/// <summary>
		/// Unregisters a previously registered event handler.
		/// </summary>
		/// <param name="logEvent">The event handler to unregister.</param>
		void RemoveEvent(Action<SqlCallEvent> logEvent);
	}
}
