using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;

namespace Voyager.DBConnection.Internal
{
	/// <summary>
	/// Interface for helper class responsible for executing database commands with event publishing.
	/// </summary>
	internal interface ICommandExecutionHelper
	{
		/// <summary>
		/// Executes a command with event publishing and error handling.
		/// </summary>
		Result<TValue> ExecuteWithEvents<TValue>(DbCommand command, Func<DbCommand, TValue> action);

		/// <summary>
		/// Asynchronously executes a command with event publishing and error handling.
		/// </summary>
		Task<Result<TValue>> ExecuteWithEventsAsync<TValue>(DbCommand command, Func<DbCommand, Task<TValue>> action, CancellationToken cancellationToken);
	}
}
