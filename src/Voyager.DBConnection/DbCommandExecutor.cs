using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;
using Voyager.DBConnection.Internal;

namespace Voyager.DBConnection
{
	public class DbCommandExecutor : IDisposable, IRegisterEvents, IFeatureHost, IDbCommandExecutor
	{
		private readonly IDatabaseInternal db;
		private readonly IMapErrorPolicy errorPolicy;
		private readonly EventHost eventHost;
		private readonly FeatureHost featureHost;

		// Helper classes for separation of concerns
		private readonly ICommandFactoryHelper commandFactoryHelper;
		private readonly IErrorMappingHelper errorMappingHelper;
		private readonly ICommandExecutionHelper commandExecutionHelper;
		private readonly IDataReaderHelper dataReaderHelper;

		public DbCommandExecutor(Database db, IMapErrorPolicy errorPolicy)
		{
			ParameterValidator.DBPolicyGuard(errorPolicy);
			ParameterValidator.DbGuard(db);

			this.db = db;
			this.errorPolicy = errorPolicy;
			this.eventHost = new EventHost();
			this.featureHost = new FeatureHost();

			// Initialize helpers - Database implements IDatabaseInternal
			this.commandFactoryHelper = new CommandFactoryHelper(db);
			this.errorMappingHelper = new ErrorMappingHelper(errorPolicy);
			this.commandExecutionHelper = new CommandExecutionHelper(db, eventHost, errorMappingHelper);
			this.dataReaderHelper = new DataReaderHelper(db, eventHost, errorMappingHelper);
		}

		public DbCommandExecutor(Database db)
		{
			ParameterValidator.DbGuard(db);
			this.db = db;
			this.errorPolicy = new DefaultMapError();
			this.eventHost = new EventHost();
			this.featureHost = new FeatureHost();

			// Initialize helpers - Database implements IDatabaseInternal
			this.commandFactoryHelper = new CommandFactoryHelper(db);
			this.errorMappingHelper = new ErrorMappingHelper(errorPolicy);
			this.commandExecutionHelper = new CommandExecutionHelper(db, eventHost, errorMappingHelper);
			this.dataReaderHelper = new DataReaderHelper(db, eventHost, errorMappingHelper);
		}

		/// <summary>
		/// Internal constructor for unit testing.
		/// Allows injection of mock helpers and database for testing purposes.
		/// </summary>
		internal DbCommandExecutor(
			IDatabaseInternal db,
			IMapErrorPolicy errorPolicy,
			ICommandFactoryHelper commandFactoryHelper,
			IErrorMappingHelper errorMappingHelper,
			ICommandExecutionHelper commandExecutionHelper,
			IDataReaderHelper dataReaderHelper,
			EventHost eventHost,
			FeatureHost featureHost)
		{
			this.db = db;
			this.errorPolicy = errorPolicy;
			this.commandFactoryHelper = commandFactoryHelper;
			this.errorMappingHelper = errorMappingHelper;
			this.commandExecutionHelper = commandExecutionHelper;
			this.dataReaderHelper = dataReaderHelper;
			this.eventHost = eventHost;
			this.featureHost = featureHost;
		}

		public void Dispose()
		{
			this.db.Dispose();
			featureHost.Dispose();
		}

		public Transaction BeginTransaction()
		{
			return db.BeginTransaction();
		}


		public Result<int> ExecuteNonQuery(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, afterCall);

		public Result<int> ExecuteNonQuery(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, afterCall);

		public Result<int> ExecuteNonQuery(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, afterCall);

		private Result<int> ExecuteNonQueryCore(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Action<DbCommand> afterExecute)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
				var executionResult = commandExecutionHelper.ExecuteWithEvents(command, executeNonQuery);
				if (executionResult.IsSuccess)
					afterExecute?.Invoke(command);
				return executionResult;
			}
		}



		public Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, afterCall, CancellationToken.None);

		public Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteNonQueryAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, afterCall, cancellationToken);

		public Task<Result<int>> ExecuteNonQueryAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, afterCall, CancellationToken.None);

		public Task<Result<int>> ExecuteNonQueryAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteNonQueryAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, afterCall, cancellationToken);

		public Task<Result<int>> ExecuteNonQueryAsync(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryAsyncCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, afterCall, CancellationToken.None);

		public Task<Result<int>> ExecuteNonQueryAsync(string procedureName, Action<DbCommand> actionAddParams, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteNonQueryAsyncCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, afterCall, cancellationToken);

		private async Task<Result<int>> ExecuteNonQueryAsyncCore(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Action<DbCommand> afterExecute, CancellationToken cancellationToken)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, Task<int>> executeNonQueryAsync = async (cmd) => await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				var executionResult = await commandExecutionHelper.ExecuteWithEventsAsync(command, executeNonQueryAsync, cancellationToken).ConfigureAwait(false);
				if (executionResult.IsSuccess)
					afterExecute?.Invoke(command);
				return executionResult;
			}
		}


		public Result<TValue> ExecuteAndBind<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, afterCall);

		public Result<TValue> ExecuteAndBind<TValue>(Func<IDatabase, DbCommand> commandFunction, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, afterCall);

		public Result<TValue> ExecuteAndBind<TValue>(string procedureName, Action<DbCommand> actionAddParams, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, afterCall);

		private Result<TValue> ExecuteAndBindCore<TValue>(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Func<DbCommand, Result<TValue>> afterCall)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
				var result = commandExecutionHelper.ExecuteWithEvents(command, executeNonQuery)
						.Bind(_ => afterCall.Invoke(command));
				return result;
			}
		}


		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken)
			=> ExecuteAndBindAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, afterCall, cancellationToken);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken)
			=> ExecuteAndBindAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, afterCall, cancellationToken);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindAsyncCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken)
			=> ExecuteAndBindAsyncCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, afterCall, cancellationToken);

		private async Task<Result<TValue>> ExecuteAndBindAsyncCore<TValue>(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, Task<int>> executeNonQueryAsync = async (cmd) => await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				var result = await commandExecutionHelper.ExecuteWithEventsAsync(command, executeNonQueryAsync, cancellationToken).ConfigureAwait(false);
				return result.Bind(_ => afterCall.Invoke(command));
			}
		}


		public Result<object> ExecuteScalar(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
			=> ExecuteScalarCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, afterCall);

		public Result<object> ExecuteScalar(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null)
			=> ExecuteScalarCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, afterCall);

		public Result<object> ExecuteScalar(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null)
			=> ExecuteScalarCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, afterCall);

		private Result<object> ExecuteScalarCore(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Action<DbCommand> afterExecute)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, object> executeScalar = (cmd) => cmd.ExecuteScalar();
				var result = commandExecutionHelper.ExecuteWithEvents(command, executeScalar);
				if (result.IsSuccess)
					afterExecute?.Invoke(command);
				return result;
			}
		}


		public Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
			=> ExecuteScalarAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, afterCall, CancellationToken.None);

		public Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteScalarAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, afterCall, cancellationToken);

		public Task<Result<object>> ExecuteScalarAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null)
			=> ExecuteScalarAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, afterCall, CancellationToken.None);

		public Task<Result<object>> ExecuteScalarAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteScalarAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, afterCall, cancellationToken);

		public Task<Result<object>> ExecuteScalarAsync(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null)
			=> ExecuteScalarAsyncCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, afterCall, CancellationToken.None);

		public Task<Result<object>> ExecuteScalarAsync(string procedureName, Action<DbCommand> actionAddParams, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteScalarAsyncCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, afterCall, cancellationToken);

		private async Task<Result<object>> ExecuteScalarAsyncCore(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Action<DbCommand> afterExecute, CancellationToken cancellationToken)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, Task<object>> executeScalarAsync = async (cmd) => await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
				var result = await commandExecutionHelper.ExecuteWithEventsAsync(command, executeScalarAsync, cancellationToken).ConfigureAwait(false);
				if (result.IsSuccess)
					afterExecute?.Invoke(command);
				return result;
			}
		}

		public Result<TValue> ExecuteReader<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
		{
			using (DbCommand command = commandFactoryHelper.CreateCommand(commandFactory))
			{
				var reader = dataReaderHelper.GetDataReader(command);
				if (!reader.IsSuccess)
					return reader.Error;

				var result = reader
						.Map(reader => dataReaderHelper.HandleReader(consumer, reader))
						.Tap(_ => afterCall?.Invoke(command))
						.Finally(() => reader.Value.Dispose());

				return result;
			}
		}

		public Result<TValue> ExecuteReader<TValue>(Func<IDatabase, DbCommand> commandFunction, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
		{
			using (DbCommand command = commandFunction(db))
			{
				var reader = dataReaderHelper.GetDataReader(command);
				if (!reader.IsSuccess)
					return reader.Error;

				var result = reader
						.Map(reader => dataReaderHelper.HandleReader(consumer, reader))
						.Tap(_ => afterCall?.Invoke(command))
						.Finally(() => reader.Value.Dispose());

				return result;
			}
		}

		public Result<TValue> ExecuteReader<TValue>(string procedureName, Action<DbCommand> actionAddParams, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
		{
			using (DbCommand command = db.GetStoredProcCommand(procedureName))
			{
				actionAddParams?.Invoke(command);
				var reader = dataReaderHelper.GetDataReader(command);
				if (!reader.IsSuccess)
					return reader.Error;

				var result = reader
						.Map(reader => dataReaderHelper.HandleReader(consumer, reader))
						.Tap(_ => afterCall?.Invoke(command))
						.Finally(() => reader.Value.Dispose());

				return result;
			}
		}

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
			=> ExecuteReaderAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, consumer, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteReaderAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFactory), null, consumer, afterCall, cancellationToken);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
			=> ExecuteReaderAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, consumer, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteReaderAsyncCore(commandFactoryHelper.CreateCommandFactory(commandFunction), null, consumer, afterCall, cancellationToken);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
			=> ExecuteReaderAsyncCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, consumer, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteReaderAsyncCore(commandFactoryHelper.CreateCommandFactory(procedureName), actionAddParams, consumer, afterCall, cancellationToken);

		private async Task<Result<TValue>> ExecuteReaderAsyncCore<TValue>(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				var reader = await dataReaderHelper.GetDataReaderAsync(command, cancellationToken).ConfigureAwait(false);
				if (!reader.IsSuccess)
					return reader.Error;

				var result = reader
						.Map(reader => dataReaderHelper.HandleReader(consumer, reader))
						.Tap(_ => afterCall?.Invoke(command))
						.Finally(() => reader.Value.Dispose());

				return result;
			}
		}

		public void AddEvent(Action<SqlCallEvent> logEvent)
		{
			eventHost.AddEvent(logEvent);
		}

		public void RemoveEvent(Action<SqlCallEvent> logEvent)
		{
			eventHost.RemoveEvent(logEvent);
		}

		public void AddFeature(IFeature feature)
		{
			featureHost.AddFeature(feature);
		}
	}
}
