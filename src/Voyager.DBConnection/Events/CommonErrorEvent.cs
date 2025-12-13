using System;

namespace Voyager.DBConnection.Events
{
    /// <summary>
    /// Represents a SQL command execution event that resulted in an error.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="SqlCallEvent"/> to include error information
    /// captured during command execution. It's used to log SQL command failures with
    /// associated error details from the Result pattern.
    /// </remarks>
    public class CommonErrorEvent : SqlCallEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonErrorEvent"/> class.
        /// </summary>
        /// <param name="error">The error that occurred during command execution.</param>
        /// <param name="callEvent">The original SQL call event with execution details.</param>
        /// <remarks>
        /// Marks the event as an error (<see cref="SqlCallEvent.IsError"/> = true)
        /// and preserves the execution duration from the original call event.
        /// </remarks>
        public CommonErrorEvent(Common.Results.Error error, SqlCallEvent callEvent) : base(callEvent.SqlText, callEvent.CallTime)
        {
            Error = error;
            this.Duration = callEvent.Duration;
            this.IsError = true;
        }

        /// <summary>
        /// Gets the error that occurred during command execution.
        /// </summary>
        public Common.Results.Error Error { get; }

        /// <summary>
        /// Returns a string representation of the error event including execution details and error message.
        /// </summary>
        /// <returns>A formatted string containing SQL call information and error message.</returns>
        public override string ToString()
        {
            return $"{base.ToString()}{Environment.NewLine}Error:{Error.Message}";
        }
    }
}
