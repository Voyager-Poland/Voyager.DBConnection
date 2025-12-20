using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	public class DbCommandExecutor : IDisposable, IRegisterEvents, IFeatureHost, IDbCommandExecutor
	{
		private readonly Database db;
		private readonly IMapErrorPolicy errorPolicy;
		private readonly EventHost eventHost = new EventHost();
		private readonly FeatureHost featureHost = new FeatureHost();

		public DbCommandExecutor(Database db, IMapErrorPolicy errorPolicy)
		{
			ParameterValidator.DBPolicyGuard(errorPolicy);
			ParameterValidator.DbGuard(db);

			this.db = db;
			this.errorPolicy = errorPolicy;
		}

		public DbCommandExecutor(Database db)
		{
			ParameterValidator.DbGuard(db);
			this.db = db;
			this.errorPolicy = new DefaultMapError();
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
			=> ExecuteNonQueryCore(() => GetCommand(commandFactory), null, afterCall);

		public Result<int> ExecuteNonQuery(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryCore(() => commandFunction(db), null, afterCall);

		public Result<int> ExecuteNonQuery(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, afterCall);

		private Result<int> ExecuteNonQueryCore(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Action<DbCommand> afterExecute)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
				var executionResult = ExecuteWithEvents(command, executeNonQuery);
				if (executionResult.IsSuccess)
					afterExecute?.Invoke(command);
				return executionResult;
			}
		}



		public Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryAsyncCore(() => GetCommand(commandFactory), null, afterCall, CancellationToken.None);

		public Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteNonQueryAsyncCore(() => GetCommand(commandFactory), null, afterCall, cancellationToken);

		public Task<Result<int>> ExecuteNonQueryAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryAsyncCore(() => commandFunction(db), null, afterCall, CancellationToken.None);

		public Task<Result<int>> ExecuteNonQueryAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteNonQueryAsyncCore(() => commandFunction(db), null, afterCall, cancellationToken);

		public Task<Result<int>> ExecuteNonQueryAsync(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null)
			=> ExecuteNonQueryAsyncCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, afterCall, CancellationToken.None);

		public Task<Result<int>> ExecuteNonQueryAsync(string procedureName, Action<DbCommand> actionAddParams, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteNonQueryAsyncCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, afterCall, cancellationToken);

		private async Task<Result<int>> ExecuteNonQueryAsyncCore(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Action<DbCommand> afterExecute, CancellationToken cancellationToken)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, Task<int>> executeNonQueryAsync = async (cmd) => await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				var executionResult = await ExecuteWithEventsAsync(command, executeNonQueryAsync, cancellationToken).ConfigureAwait(false);
				if (executionResult.IsSuccess)
					afterExecute?.Invoke(command);
				return executionResult;
			}
		}


		public Result<TValue> ExecuteAndBind<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindCore(() => GetCommand(commandFactory), null, afterCall);

		public Result<TValue> ExecuteAndBind<TValue>(Func<IDatabase, DbCommand> commandFunction, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindCore(() => commandFunction(db), null, afterCall);

		public Result<TValue> ExecuteAndBind<TValue>(string procedureName, Action<DbCommand> actionAddParams, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, afterCall);

		private Result<TValue> ExecuteAndBindCore<TValue>(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Func<DbCommand, Result<TValue>> afterCall)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
				var result = ExecuteWithEvents(command, executeNonQuery)
						.Bind(_ => afterCall.Invoke(command));
				return result;
			}
		}


		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindAsyncCore(() => GetCommand(commandFactory), null, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken)
			=> ExecuteAndBindAsyncCore(() => GetCommand(commandFactory), null, afterCall, cancellationToken);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindAsyncCore(() => commandFunction(db), null, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken)
			=> ExecuteAndBindAsyncCore(() => commandFunction(db), null, afterCall, cancellationToken);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, Func<DbCommand, Result<TValue>> afterCall)
			=> ExecuteAndBindAsyncCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteAndBindAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken)
			=> ExecuteAndBindAsyncCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, afterCall, cancellationToken);

		private async Task<Result<TValue>> ExecuteAndBindAsyncCore<TValue>(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, Task<int>> executeNonQueryAsync = async (cmd) => await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				var result = await ExecuteWithEventsAsync(command, executeNonQueryAsync, cancellationToken).ConfigureAwait(false);
				return result.Bind(_ => afterCall.Invoke(command));
			}
		}


		public Result<object> ExecuteScalar(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
			=> ExecuteScalarCore(() => GetCommand(commandFactory), null, afterCall);

		public Result<object> ExecuteScalar(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null)
			=> ExecuteScalarCore(() => commandFunction(db), null, afterCall);

		public Result<object> ExecuteScalar(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null)
			=> ExecuteScalarCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, afterCall);

		private Result<object> ExecuteScalarCore(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Action<DbCommand> afterExecute)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, object> executeScalar = (cmd) => cmd.ExecuteScalar();
				var result = ExecuteWithEvents(command, executeScalar);
				if (result.IsSuccess)
					afterExecute?.Invoke(command);
				return result;
			}
		}


		public Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
			=> ExecuteScalarAsyncCore(() => GetCommand(commandFactory), null, afterCall, CancellationToken.None);

		public Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteScalarAsyncCore(() => GetCommand(commandFactory), null, afterCall, cancellationToken);

		public Task<Result<object>> ExecuteScalarAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null)
			=> ExecuteScalarAsyncCore(() => commandFunction(db), null, afterCall, CancellationToken.None);

		public Task<Result<object>> ExecuteScalarAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteScalarAsyncCore(() => commandFunction(db), null, afterCall, cancellationToken);

		public Task<Result<object>> ExecuteScalarAsync(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null)
			=> ExecuteScalarAsyncCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, afterCall, CancellationToken.None);

		public Task<Result<object>> ExecuteScalarAsync(string procedureName, Action<DbCommand> actionAddParams, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteScalarAsyncCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, afterCall, cancellationToken);

		private async Task<Result<object>> ExecuteScalarAsyncCore(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, Action<DbCommand> afterExecute, CancellationToken cancellationToken)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				Func<DbCommand, Task<object>> executeScalarAsync = async (cmd) => await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
				var result = await ExecuteWithEventsAsync(command, executeScalarAsync, cancellationToken).ConfigureAwait(false);
				if (result.IsSuccess)
					afterExecute?.Invoke(command);
				return result;
			}
		}

		public Result<TValue> ExecuteReader<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
		{
			using (DbCommand command = GetCommand(commandFactory))
			{
				var reader = GetDataReader(command);
				if (!reader.IsSuccess)
					return reader.Error;

				var result = reader
						.Map(reader => HandleReader(consumer, reader))
						.Tap(_ => afterCall?.Invoke(command))
						.Finally(() => reader.Value.Dispose());

				return result;
			}
		}

		public Result<TValue> ExecuteReader<TValue>(Func<IDatabase, DbCommand> commandFunction, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
		{
			using (DbCommand command = commandFunction(db))
			{
				var reader = GetDataReader(command);
				if (!reader.IsSuccess)
					return reader.Error;

				var result = reader
						.Map(reader => HandleReader(consumer, reader))
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
				var reader = GetDataReader(command);
				if (!reader.IsSuccess)
					return reader.Error;

				var result = reader
						.Map(reader => HandleReader(consumer, reader))
						.Tap(_ => afterCall?.Invoke(command))
						.Finally(() => reader.Value.Dispose());

				return result;
			}
		}

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
			=> ExecuteReaderAsyncCore(() => GetCommand(commandFactory), null, consumer, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteReaderAsyncCore(() => GetCommand(commandFactory), null, consumer, afterCall, cancellationToken);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
			=> ExecuteReaderAsyncCore(() => commandFunction(db), null, consumer, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteReaderAsyncCore(() => commandFunction(db), null, consumer, afterCall, cancellationToken);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
			=> ExecuteReaderAsyncCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, consumer, afterCall, CancellationToken.None);

		public Task<Result<TValue>> ExecuteReaderAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken)
			=> ExecuteReaderAsyncCore(() => db.GetStoredProcCommand(procedureName), actionAddParams, consumer, afterCall, cancellationToken);

		private async Task<Result<TValue>> ExecuteReaderAsyncCore<TValue>(Func<DbCommand> commandFactory, Action<DbCommand> beforeExecute, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken)
		{
			using (DbCommand command = commandFactory())
			{
				beforeExecute?.Invoke(command);
				var reader = await GetDataReaderAsync(command, cancellationToken).ConfigureAwait(false);
				if (!reader.IsSuccess)
					return reader.Error;

				var result = reader
						.Map(reader => HandleReader(consumer, reader))
						.Tap(_ => afterCall?.Invoke(command))
						.Finally(() => reader.Value.Dispose());

				return result;
			}
		}

		private TDomain HandleReader<TDomain>(IGetConsumer<TDomain> consumer, IDataReader dr)
		{
			TDomain result = consumer.GetResults(dr);
			while (dr.NextResult())
			{ }
			// dr.Close(); // removed — Dispose is called in Finally()
			return result;
		}

		protected virtual Result<IDataReader> GetDataReader(DbCommand command)
		{
			Func<DbCommand, IDataReader> executeReader = (cmd) => cmd.ExecuteReader();
			return ExecuteWithEvents(command, executeReader);
		}

		protected virtual async Task<Result<IDataReader>> GetDataReaderAsync(DbCommand command, CancellationToken cancellationToken)
		{
			Func<DbCommand, Task<IDataReader>> executeReaderAsync = async (cmd) => await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
			return await ExecuteWithEventsAsync(command, executeReaderAsync, cancellationToken).ConfigureAwait(false);
		}

		protected DbCommand GetCommand(IDbCommandFactory commandFactory)
		{
			return commandFactory.ConstructDbCommand(db);
		}

		private Result<TValue> ExecuteWithEvents<TValue>(DbCommand command, Func<DbCommand, TValue> action)
		{
			var proc = new ExecutionEventPublisher<TValue>(this.eventHost);

			return Result<TValue>.Try(
					() =>
					{
						db.OpenCmd(command);
						return proc.Execute(() => action.Invoke(command), command);
					},
					ex =>
					{
						var error = HandleSqlException(ex);
						proc.ErrorPublish(error);
						return error;
					}
			);
		}

		private async Task<Result<TValue>> ExecuteWithEventsAsync<TValue>(DbCommand command, Func<DbCommand, Task<TValue>> action, CancellationToken cancellationToken)
		{
			var proc = new ExecutionEventPublisher<TValue>(this.eventHost);

			return await Result<TValue>.TryAsync(
					async () =>
					{
						db.OpenCmd(command);
						var result = await action.Invoke(command).ConfigureAwait(false);
						return proc.Execute(() => result, command);
					},
					ex =>
					{
						var error = HandleSqlException(ex);
						proc.ErrorPublish(error);
						return error;
					}
			).ConfigureAwait(false);
		}

		Common.Results.Error HandleSqlException(Exception ex)
		{
			return errorPolicy.MapError(ex);
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
