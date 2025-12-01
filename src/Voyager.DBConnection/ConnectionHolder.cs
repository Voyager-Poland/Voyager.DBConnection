using System;
using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection
{
    /// <summary>
    /// Manages database connection lifecycle with lazy initialization and automatic cleanup.
    /// </summary>
    internal class ConnectionHolder : IDisposable
    {
        private readonly DbProviderFactory _factory;
        private readonly Func<string> _connectionStringProvider;
        private DbConnection _connection;
        private bool _disposed;

        /// <summary>
        /// Creates a new ConnectionHolder.
        /// </summary>
        /// <param name="factory">The DbProviderFactory to create connections.</param>
        /// <param name="connectionStringProvider">Function that provides the connection string (allows for lazy evaluation).</param>
        public ConnectionHolder(DbProviderFactory factory, Func<string> connectionStringProvider)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _connectionStringProvider = connectionStringProvider ?? throw new ArgumentNullException(nameof(connectionStringProvider));
        }

        /// <summary>
        /// Gets an open database connection. Creates and opens one if needed.
        /// </summary>
        /// <returns>An open DbConnection.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the holder has been disposed.</exception>
        public DbConnection GetConnection()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ConnectionHolder));

            EnsureConnection();
            return _connection;
        }

        /// <summary>
        /// Gets whether the connection is ready for use.
        /// </summary>
        public bool IsConnectionReady => _connection != null && _connection.State != ConnectionState.Broken;

        private void EnsureConnection()
        {
            if (!IsConnectionReady)
            {
                _connection?.Dispose();
                _connection = _factory.CreateConnection();
                _connection.ConnectionString = _connectionStringProvider();
            }

            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        /// <summary>
        /// Releases all resources used by the ConnectionHolder.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Dispose();
                _connection = null;
                _disposed = true;
            }
        }
    }
}
