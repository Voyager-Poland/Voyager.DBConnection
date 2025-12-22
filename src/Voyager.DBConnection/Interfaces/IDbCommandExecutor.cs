using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;
using Voyager.DBConnection.Events;

namespace Voyager.DBConnection.Interfaces
{
    /// <summary>
    /// Defines a contract for executing database commands with Result-based error handling.
    /// </summary>
    /// <remarks>
    /// This interface provides a testable, mockable abstraction for database command execution
    /// using the Result monad pattern instead of throwing exceptions. All methods return
    /// <see cref="Result{T}"/> which encapsulates either a successful value or an error.
    /// Includes both synchronous and asynchronous operations with CancellationToken support.
    /// </remarks>
    public interface IDbCommandExecutor
    {
        /// <summary>
        /// Executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the number of rows affected, or an error if the operation failed.</returns>
        Result<int> ExecuteNonQuery(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Executes a non-query command using a function that creates the command.
        /// </summary>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the number of rows affected, or an error if the operation failed.</returns>
        Result<int> ExecuteNonQuery(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Executes a stored procedure as a non-query command.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Optional callback to add parameters to the command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the number of rows affected, or an error if the operation failed.</returns>
        Result<int> ExecuteNonQuery(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a non-query command and returns the number of affected rows.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A task representing the asynchronous operation with the result containing rows affected or an error.</returns>
        Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a non-query command with cancellation support.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing rows affected or an error.</returns>
        Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a non-query command using a function that creates the command.
        /// </summary>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A task representing the asynchronous operation with the result containing rows affected or an error.</returns>
        Task<Result<int>> ExecuteNonQueryAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a non-query command using a function with cancellation support.
        /// </summary>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing rows affected or an error.</returns>
        Task<Result<int>> ExecuteNonQueryAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a stored procedure as a non-query command.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Optional callback to add parameters to the command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A task representing the asynchronous operation with the result containing rows affected or an error.</returns>
        Task<Result<int>> ExecuteNonQueryAsync(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a stored procedure as a non-query command with cancellation support.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Optional callback to add parameters to the command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing rows affected or an error.</returns>
        Task<Result<int>> ExecuteNonQueryAsync(string procedureName, Action<DbCommand> actionAddParams, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the scalar value, or an error if the operation failed.</returns>
        Result<object> ExecuteScalar(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Executes a command using a function and returns the first column of the first row.
        /// </summary>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the scalar value, or an error if the operation failed.</returns>
        Result<object> ExecuteScalar(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Executes a stored procedure and returns the first column of the first row.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Optional callback to add parameters to the command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the scalar value, or an error if the operation failed.</returns>
        Result<object> ExecuteScalar(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a command and returns the first column of the first row.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the scalar value or an error.</returns>
        Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a command and returns the first column of the first row with cancellation support.
        /// </summary>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the scalar value or an error.</returns>
        Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a command using a function and returns the first column of the first row.
        /// </summary>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the scalar value or an error.</returns>
        Task<Result<object>> ExecuteScalarAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a command using a function with cancellation support.
        /// </summary>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the scalar value or an error.</returns>
        Task<Result<object>> ExecuteScalarAsync(Func<IDatabase, DbCommand> commandFunction, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a stored procedure and returns the first column of the first row.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Optional callback to add parameters to the command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the scalar value or an error.</returns>
        Task<Result<object>> ExecuteScalarAsync(string procedureName, Action<DbCommand> actionAddParams = null, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a stored procedure with cancellation support.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Optional callback to add parameters to the command.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the scalar value or an error.</returns>
        Task<Result<object>> ExecuteScalarAsync(string procedureName, Action<DbCommand> actionAddParams, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command and processes the result set using a consumer.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the processed value, or an error if the operation failed.</returns>
        Result<TValue> ExecuteReader<TValue>(IDbCommandFactory commandFactory, IResultsConsumer<TValue> consumer, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Executes a command using a function and processes the result set using a consumer.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the processed value, or an error if the operation failed.</returns>
        Result<TValue> ExecuteReader<TValue>(Func<IDatabase, DbCommand> commandFunction, IResultsConsumer<TValue> consumer, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Executes a stored procedure and processes the result set using a consumer.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Optional callback to add parameters to the command.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A result containing the processed value, or an error if the operation failed.</returns>
        Result<TValue> ExecuteReader<TValue>(string procedureName, Action<DbCommand> actionAddParams, IResultsConsumer<TValue> consumer, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a command and processes the result set using a consumer.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the processed value or an error.</returns>
        Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IResultsConsumer<TValue> consumer, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a command and processes the result set with cancellation support.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the processed value or an error.</returns>
        Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IResultsConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a command using a function and processes the result set.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the processed value or an error.</returns>
        Task<Result<TValue>> ExecuteReaderAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, IResultsConsumer<TValue> consumer, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a command using a function and processes the result set with cancellation support.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the processed value or an error.</returns>
        Task<Result<TValue>> ExecuteReaderAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, IResultsConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a stored procedure and processes the result set.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Optional callback to add parameters to the command.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the processed value or an error.</returns>
        Task<Result<TValue>> ExecuteReaderAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, IResultsConsumer<TValue> consumer, Action<DbCommand> afterCall = null);

        /// <summary>
        /// Asynchronously executes a stored procedure and processes the result set with cancellation support.
        /// </summary>
        /// <typeparam name="TValue">The type of the result produced by the consumer.</typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Optional callback to add parameters to the command.</param>
        /// <param name="consumer">The consumer that processes the data reader.</param>
        /// <param name="afterCall">Optional callback invoked after successful execution with the command.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the processed value or an error.</returns>
        Task<Result<TValue>> ExecuteReaderAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, IResultsConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a non-query command and binds the result using the provided callback.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Callback that maps the command to a result value using Bind.</param>
        /// <returns>A result containing the mapped value, or an error if the operation failed.</returns>
        Result<TValue> ExecuteAndBind<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall);

        /// <summary>
        /// Executes a non-query command using a function and binds the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="afterCall">Callback that maps the command to a result value using Bind.</param>
        /// <returns>A result containing the mapped value, or an error if the operation failed.</returns>
        Result<TValue> ExecuteAndBind<TValue>(Func<IDatabase, DbCommand> commandFunction, Func<DbCommand, Result<TValue>> afterCall);

        /// <summary>
        /// Executes a stored procedure and binds the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Callback to add parameters to the command.</param>
        /// <param name="afterCall">Callback that maps the command to a result value using Bind.</param>
        /// <returns>A result containing the mapped value, or an error if the operation failed.</returns>
        Result<TValue> ExecuteAndBind<TValue>(string procedureName, Action<DbCommand> actionAddParams, Func<DbCommand, Result<TValue>> afterCall);

        /// <summary>
        /// Asynchronously executes a non-query command and binds the result using the provided callback.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Callback that maps the command to a result value using Bind.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the mapped value or an error.</returns>
        Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall);

        /// <summary>
        /// Asynchronously executes a non-query command and binds the result with cancellation support.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="commandFactory">The factory that creates the database command.</param>
        /// <param name="afterCall">Callback that maps the command to a result value using Bind.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the mapped value or an error.</returns>
        Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a non-query command using a function and binds the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="afterCall">Callback that maps the command to a result value using Bind.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the mapped value or an error.</returns>
        Task<Result<TValue>> ExecuteAndBindAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, Func<DbCommand, Result<TValue>> afterCall);

        /// <summary>
        /// Asynchronously executes a non-query command using a function and binds the result with cancellation support.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="commandFunction">Function that creates the database command using the database instance.</param>
        /// <param name="afterCall">Callback that maps the command to a result value using Bind.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the mapped value or an error.</returns>
        Task<Result<TValue>> ExecuteAndBindAsync<TValue>(Func<IDatabase, DbCommand> commandFunction, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a stored procedure and binds the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Callback to add parameters to the command.</param>
        /// <param name="afterCall">Callback that maps the command to a result value using Bind.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the mapped value or an error.</returns>
        Task<Result<TValue>> ExecuteAndBindAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, Func<DbCommand, Result<TValue>> afterCall);

        /// <summary>
        /// Asynchronously executes a stored procedure and binds the result with cancellation support.
        /// </summary>
        /// <typeparam name="TValue">The type of the mapped result.</typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="actionAddParams">Callback to add parameters to the command.</param>
        /// <param name="afterCall">Callback that maps the command to a result value using Bind.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result containing the mapped value or an error.</returns>
        Task<Result<TValue>> ExecuteAndBindAsync<TValue>(string procedureName, Action<DbCommand> actionAddParams, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken);
    }
}
