using System;
using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection
{
    internal class TransactionHolder : IDisposable
    {
        private readonly DbTransaction _transaction;
        private bool _committed;
        private bool _disposed;

        public TransactionHolder(DbConnection connection, IsolationLevel isolationLevel)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _transaction = connection.BeginTransaction(isolationLevel);
        }

        public bool IsActive => !_disposed && _transaction != null;

        public DbTransaction Transaction => _transaction;

        public void Commit()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TransactionHolder));

            _transaction.Commit();
            _committed = true;
        }

        public void Rollback()
        {
            if (_disposed || _transaction == null)
                return;

            try
            {
                _transaction.Rollback();
            }
            catch { /* ignoruj błędy podczas rollback */ }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (!_committed)
                {
                    Rollback();
                }

                try
                {
                    _transaction?.Dispose();
                }
                catch { }

                _disposed = true;
            }
        }
    }
}
