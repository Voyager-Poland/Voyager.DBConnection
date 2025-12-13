using System;

namespace Voyager.DBConnection.Events
{
	/// <summary>
	/// Represents a SQL command execution event that resulted in an exception.
	/// </summary>
	/// <remarks>
	/// This class extends <see cref="SqlCallEvent"/> to include exception information
	/// captured during command execution. Unlike <see cref="CommonErrorEvent"/> which uses
	/// the Result pattern, this event captures thrown CLR exceptions during database operations.
	/// </remarks>
	public class ExceptionEvent : SqlCallEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionEvent"/> class.
		/// </summary>
		/// <param name="exception">The exception that was thrown during command execution.</param>
		/// <param name="callEvent">The original SQL call event with execution details.</param>
		/// <remarks>
		/// Marks the event as an error (<see cref="SqlCallEvent.IsError"/> = true)
		/// and preserves the execution duration from the original call event.
		/// </remarks>
		public ExceptionEvent(Exception exception, SqlCallEvent callEvent) : base(callEvent.SqlText, callEvent.CallTime)
		{
			Exception = exception;
			this.Duration = callEvent.Duration;
			this.IsError = true;
		}

		/// <summary>
		/// Gets the exception that was thrown during command execution.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Returns a string representation of the exception event including execution details and exception information.
		/// </summary>
		/// <returns>A formatted string containing SQL call information and complete exception details.</returns>
		public override string ToString()
		{
			return $"{base.ToString()}{Environment.NewLine}Error:{Exception.ToString()}";
		}
	}
}
