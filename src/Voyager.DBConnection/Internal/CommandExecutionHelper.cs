using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;
using Voyager.DBConnection.Events;

namespace Voyager.DBConnection.Internal
{
	/// <summary>
	/// Helper class responsible for executing database commands with event publishing.
	/// Separates command execution logic from DbCommandExecutor.
	/// </summary>
	internal class CommandExecutionHelper : ICommandExecutionHelper
	{
		private readonly Database database;
		private readonly EventHost eventHost;
		private readonly IErrorMappingHelper errorMapper;

		public CommandExecutionHelper(Database database, EventHost eventHost, IErrorMappingHelper errorMapper)
		{
			this.database = database;
			this.eventHost = eventHost;
			this.errorMapper = errorMapper;
		}

		/// <summary>
		/// Executes a command with event publishing and error handling.
		/// </summary>
		public Result<TValue> ExecuteWithEvents<TValue>(DbCommand command, Func<DbCommand, TValue> action)
		{
			var proc = new ExecutionEventPublisher<TValue>(eventHost);

			return Result<TValue>.Try(
				() =>
				{
					database.OpenCmd(command);
					return proc.Execute(() => action.Invoke(command), command);
				},
				ex =>
				{
					var error = errorMapper.MapException(ex);
					proc.ErrorPublish(error);
					return error;
				}
			);
		}

		/// <summary>
		/// Asynchronously executes a command with event publishing and error handling.
		/// </summary>
		public async Task<Result<TValue>> ExecuteWithEventsAsync<TValue>(DbCommand command, Func<DbCommand, Task<TValue>> action, CancellationToken cancellationToken)
		{
			var proc = new ExecutionEventPublisher<TValue>(eventHost);

			return await Result<TValue>.TryAsync(
				async () =>
				{
					database.OpenCmd(command);
					var result = await action.Invoke(command).ConfigureAwait(false);
					return proc.Execute(() => result, command);
				},
				ex =>
				{
					var error = errorMapper.MapException(ex);
					proc.ErrorPublish(error);
					return error;
				}
			).ConfigureAwait(false);
		}
	}
}
