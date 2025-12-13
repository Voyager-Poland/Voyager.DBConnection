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
    /// <summary>
    /// Provides database command execution with result-based error handling, event publishing, and feature extensibility.
    /// </summary>
    public class DbCommandExecutor : IDisposable, IRegisterEvents, IFeatureHost
    {
        private readonly Database db;
        private readonly IMapErrorPolicy errorPolicy;
        private readonly EventHost eventHost = new EventHost();
        private readonly FeatureHost featureHost = new FeatureHost();
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbCommandExecutor"/> class with a custom error mapping policy.
        /// </summary>
        /// <param name="db">The database instance.</param>
        /// <param name="errorPolicy">The error mapping policy for converting exceptions to errors.</param>
        /// <exception cref="LackExceptionPolicyException">Thrown when errorPolicy is null.</exception>
        /// <exception cref="NoDbException">Thrown when db is null.</exception>
        public DbCommandExecutor(Database db, IMapErrorPolicy errorPolicy)
        {
            Guard.DBPolicyGuard(errorPolicy);
            Guard.DbGuard(db);

            this.db = db;
            this.errorPolicy = errorPolicy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbCommandExecutor"/> class with default error mapping.
        /// </summary>
        /// <param name="db">The database instance.</param>
        /// <exception cref="NoDbException">Thrown when db is null.</exception>
        public DbCommandExecutor(Database db)
        {
            Guard.DbGuard(db);
            this.db = db;
            this.errorPolicy = new DefaultMapError();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                // free managed resources
                this.db.Dispose();
                featureHost.Dispose();
            }
            // free unmanaged resources (none)
            disposed = true;
        }

        ~DbCommandExecutor()
        {
            Dispose(false);
        }

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <returns>A <see cref="Transaction"/> object representing the new transaction.</returns>
        public virtual Transaction BeginTransaction()
        {
            return db.BeginTransaction();
        }

        /// <summary>
        /// Executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the number of rows affected, or an error if the operation failed.</returns>
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

        /// <summary>
        /// Executes a non-query command and maps the result using the provided callback.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Callback that maps the command to a result value.</param>
        /// <returns>A result containing the mapped value, or an error if the operation failed.</returns>
        public virtual Result<TValue> ExecuteMap<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
                var result = ExecuteWithEvents(command, executeNonQuery)
                    .Bind(_ => afterCall.Invoke(command));
                return result;
            }
        }

        /// <summary>
        /// Executes a command and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the scalar value, or an error if the operation failed.</returns>
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

        /// <summary>
        /// Executes a command and processes the result set using a consumer.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the processed value, or an error if the operation failed.</returns>
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

        /// <summary>
        /// Asynchronously executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation, containing a result with the number of rows affected or an error.</returns>
        public virtual async Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Asynchronously executes a non-query command and maps the result using the provided callback.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Callback that maps the command to a result value.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the mapped result or an error.</returns>
        public virtual async Task<Result<TValue>> ExecuteMapAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken = default)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, Task<int>> executeNonQueryAsync = async (cmd) => await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                var result = await ExecuteWithEventsAsync(command, executeNonQueryAsync, cancellationToken).ConfigureAwait(false);
                return result.Bind(_ => afterCall.Invoke(command));
            }
        }

        /// <summary>
        /// Asynchronously executes a command and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the scalar value or an error.</returns>
        public virtual async Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Asynchronously executes a command and processes the result set using a consumer.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the processed value or an error.</returns>
        public virtual async Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null, CancellationToken cancellationToken = default)
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
                    return proc.Call(() => action.Invoke(command), command);
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
                    return proc.Call(() => result, command);
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

        /// <summary>
        /// Registers an event handler for SQL call events.
        /// </summary>
        /// <param name="logEvent">The event handler to register.</param>
        public void AddEvent(Action<SqlCallEvent> logEvent)
        {
            eventHost.AddEvent(logEvent);
        }

        /// <summary>
        /// Unregisters an event handler for SQL call events.
        /// </summary>
        /// <param name="logEvent">The event handler to unregister.</param>
        public void RemoveEvent(Action<SqlCallEvent> logEvent)
        {
            eventHost.RemoveEvent(logEvent);
        }

        /// <summary>
        /// Adds a feature to extend the functionality of the command executor.
        /// </summary>
        /// <param name="feature">The feature to add.</param>
        public void AddFeature(IFeature feature)
        {
            featureHost.AddFeature(feature);
        }
    }
}
