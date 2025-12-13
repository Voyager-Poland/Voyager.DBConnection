using System;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	/// <summary>
	/// Hosts and manages SQL call events for database operations.
	/// </summary>
	/// <remarks>
	/// This internal class implements the event hosting pattern, allowing subscribers
	/// to register and receive notifications about SQL command execution events.
	/// It bridges the <see cref="IInvokeEvents"/> interface (for publishing events)
	/// and <see cref="IRegisterEvents"/> interface (for subscriber management).
	/// </remarks>
	internal class EventHost : IInvokeEvents, IRegisterEvents
	{
		/// <summary>
		/// Event raised when a SQL command is executed.
		/// </summary>
		event Action<Events.SqlCallEvent> SqlCallEvent;

		/// <summary>
		/// Invokes all registered SQL call event handlers with the provided event data.
		/// </summary>
		/// <param name="callEvent">The SQL call event containing execution details (timing, command, etc.)</param>
		/// <remarks>
		/// This method safely invokes all event handlers if any are registered.
		/// If no handlers are registered, the call is silently ignored.
		/// </remarks>
		void IInvokeEvents.Invoke(SqlCallEvent callEvent)
		{
			if (this.SqlCallEvent != null)
				this.SqlCallEvent.Invoke(callEvent);
		}

		/// <summary>
		/// Registers a handler to receive SQL call events.
		/// </summary>
		/// <param name="logEvent">The event handler to register. Cannot be null.</param>
		/// <remarks>
		/// Multiple handlers can be registered. They will be invoked in the order
		/// they were registered when a SQL command is executed.
		/// </remarks>
		public void AddEvent(Action<SqlCallEvent> logEvent)
		{
			SqlCallEvent += logEvent;
		}

		/// <summary>
		/// Unregisters a previously registered SQL call event handler.
		/// </summary>
		/// <param name="logEvent">The event handler to remove. If not registered, no action is taken.</param>
		/// <remarks>
		/// Safely removes a handler from the event subscription list.
		/// Attempting to remove a handler that was not registered has no effect.
		/// </remarks>
		public void RemoveEvent(Action<SqlCallEvent> logEvent)
		{
			SqlCallEvent -= logEvent;
		}
	}
}
