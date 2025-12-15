using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;
using Voyager.DBConnection.Events;

namespace Voyager.DBConnection.Interfaces
{
    /// <summary>
    /// Contract for executing database commands with result-based error handling and event publishing.
    /// </summary>
    public interface IDbCommandExecutor
    {
        Result<int> ExecuteNonQuery(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null);
        Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null);
        Task<Result<int>> ExecuteNonQueryAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        Result<object> ExecuteScalar(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null);
        Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null);
        Task<Result<object>> ExecuteScalarAsync(IDbCommandFactory commandFactory, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        Result<TValue> ExecuteReader<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null);
        Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null);
        Task<Result<TValue>> ExecuteReaderAsync<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall, CancellationToken cancellationToken);

        Result<TValue> ExecuteAndBind<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall);
        Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall);
        Task<Result<TValue>> ExecuteAndBindAsync<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall, CancellationToken cancellationToken);
    }
}
