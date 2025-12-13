using System;
using System.Data.Common;

namespace Voyager.DBConnection.Events
{
	/// <summary>
	/// Represents a SQL command execution event with timing and execution information.
	/// </summary>
	/// <remarks>
	/// This class is used to track SQL command execution details including the SQL text,
	/// execution time, duration, and error status. It's typically created when a SQL command
	/// is about to be executed and finalized after execution completes.
	/// </remarks>
	public class SqlCallEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SqlCallEvent"/> class.
		/// </summary>
		/// <param name="sqlText">The SQL command text being executed.</param>
		/// <param name="dateTime">The date and time when the command execution started.</param>
		public SqlCallEvent(String sqlText, DateTime dateTime)
		{
			this.SqlText = sqlText;
			this.CallTime = dateTime;
		}

		/// <summary>
		/// Finalizes the execution event by calculating the total execution duration.
		/// </summary>
		/// <remarks>
		/// This method should be called after the SQL command completes execution.
		/// It calculates the <see cref="Duration"/> as the time elapsed between
		/// <see cref="CallTime"/> and the current system time.
		/// </remarks>
		public void Finish()
		{
			this.Duration = DateTime.Now - this.CallTime;
		}

		/// <summary>
		/// Creates a new <see cref="SqlCallEvent"/> instance from a <see cref="DbCommand"/>.
		/// </summary>
		/// <param name="command">The database command being executed.</param>
		/// <returns>A new <see cref="SqlCallEvent"/> initialized with the command text and current time.</returns>
		/// <remarks>
		/// This factory method captures the command text and current timestamp at the moment
		/// the SQL command is about to be executed.
		/// </remarks>
		internal static SqlCallEvent Create(DbCommand command)
		{
			return new SqlCallEvent(command.CommandText, DateTime.Now);
		}

		/// <summary>
		/// Gets the SQL command text being executed.
		/// </summary>
		public string SqlText { get; }

		/// <summary>
		/// Gets the date and time when the SQL command execution started.
		/// </summary>
		public DateTime CallTime { get; }

		/// <summary>
		/// Gets the total duration of the SQL command execution.
		/// </summary>
		/// <remarks>
		/// This value is calculated when <see cref="Finish"/> is called and represents
		/// the elapsed time between <see cref="CallTime"/> and the completion time.
		/// </remarks>
		public TimeSpan Duration { get; protected set; }

		/// <summary>
		/// Returns a string representation of the SQL call event.
		/// </summary>
		/// <returns>A formatted string containing call time, SQL text, and duration.</returns>
		public override string ToString()
		{
			return $"{CallTime}; {SqlText}; Duration: {Duration}";
		}

		/// <summary>
		/// Gets a value indicating whether the SQL command execution resulted in an error.
		/// </summary>
		public bool IsError { get; protected set; }
	}
}
