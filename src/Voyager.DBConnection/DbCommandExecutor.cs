using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;
using Voyager.DBConnection.Exceptions;

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

        public virtual void Dispose()
        {
            this.db.Dispose();
            featureHost.Dispose();
        }

        public virtual Transaction BeginTransaction()
        {
            return db.BeginTransaction();
        }

        public virtual Result<int> ExecuteNonQuery(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
                var executionResult = ExecuteWithEvents(command, executeNonQuery);
                if (executionResult.IsSuccess)
                    afterCall?.Invoke(command);
                return executionResult;
            }
        }

        public virtual Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null) => ExecuteNonQueryAsync(commandFactory, afterCall, CancellationToken.None);

        public virtual async Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, Task<int>> executeNonQueryAsync = async (cmd) => await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                var executionResult = await ExecuteWithEventsAsync(command, executeNonQueryAsync, cancellationToken).ConfigureAwait(false);
                if (executionResult.IsSuccess)
                    afterCall?.Invoke(command);
                return executionResult;
            }
        }

        public virtual Result<TValue> ExecuteAndBind<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
                var result = ExecuteWithEvents(command, executeNonQuery)
                    .Bind(_ => afterCall.Invoke(command));
                return result;
            }
        }

        public virtual Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall) => ExecuteAndBindAsync(commandFactory, afterCall, CancellationToken.None);

        public virtual async Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, Task<int>> executeNonQueryAsync = async (cmd) => await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                var result = await ExecuteWithEventsAsync(command, executeNonQueryAsync, cancellationToken).ConfigureAwait(false);
                return result.Bind(_ => afterCall.Invoke(command));
            }
        }

        public virtual Result<object> ExecuteScalar(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, object> executeScalar = (cmd) => cmd.ExecuteScalar();
                var result = ExecuteWithEvents(command, executeScalar);
                if (result.IsSuccess)
                    afterCall?.Invoke(command);
                return result;
            }
        }

        public virtual Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null) => ExecuteScalarAsync(commandFactory, afterCall, CancellationToken.None);

        public virtual async Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, Task<object>> executeScalarAsync = async (cmd) => await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                var result = await ExecuteWithEventsAsync(command, executeScalarAsync, cancellationToken).ConfigureAwait(false);
                if (result.IsSuccess)
                    afterCall?.Invoke(command);
                return result;
            }
        }

        public virtual Result<TValue> ExecuteReader<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
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

        public virtual Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null) => ExecuteReaderAsync(commandFactory, consumer, afterCall, CancellationToken.None);

        public virtual async Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
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
