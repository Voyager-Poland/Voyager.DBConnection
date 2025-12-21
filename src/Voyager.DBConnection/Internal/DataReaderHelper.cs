using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.Internal
{
	/// <summary>
	/// Helper class responsible for DataReader operations.
	/// Separates DataReader handling logic from DbCommandExecutor.
	/// </summary>
	internal class DataReaderHelper
	{
		private readonly EventHost eventHost;
		private readonly ErrorMappingHelper errorMapper;
		private readonly Database database;

		public DataReaderHelper(Database database, EventHost eventHost, ErrorMappingHelper errorMapper)
		{
			this.database = database;
			this.eventHost = eventHost;
			this.errorMapper = errorMapper;
		}

		/// <summary>
		/// Executes a command and returns a DataReader.
		/// </summary>
		public Result<IDataReader> GetDataReader(DbCommand command)
		{
			var proc = new ExecutionEventPublisher<IDataReader>(eventHost);

			return Result<IDataReader>.Try(
				() =>
				{
					database.OpenCmd(command);
					return proc.Execute(() => command.ExecuteReader(), command);
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
		/// Asynchronously executes a command and returns a DataReader.
		/// </summary>
		public async Task<Result<IDataReader>> GetDataReaderAsync(DbCommand command, CancellationToken cancellationToken)
		{
			var proc = new ExecutionEventPublisher<IDataReader>(eventHost);

			return await Result<IDataReader>.TryAsync(
				async () =>
				{
					database.OpenCmd(command);
					var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					return proc.Execute(() => reader, command);
				},
				ex =>
				{
					var error = errorMapper.MapException(ex);
					proc.ErrorPublish(error);
					return error;
				}
			).ConfigureAwait(false);
		}

		/// <summary>
		/// Processes a DataReader using the provided consumer.
		/// </summary>
		public TDomain HandleReader<TDomain>(IGetConsumer<TDomain> consumer, IDataReader reader)
		{
			TDomain result = consumer.GetResults(reader);
			while (reader.NextResult())
			{ }
			// reader.Close(); // removed — Dispose is called in Finally()
			return result;
		}
	}
}
