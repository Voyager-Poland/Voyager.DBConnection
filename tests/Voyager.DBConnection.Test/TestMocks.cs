using System;
using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection.Test
{
    internal class TestDbProviderFactory : DbProviderFactory
    {
        public override DbConnection CreateConnection()
        {
            return new TestDbConnection();
        }

        public override DbCommand CreateCommand()
        {
            return new TestDbCommand();
        }

        public override DbParameter CreateParameter()
        {
            return new TestDbParameter();
        }
    }

    internal class TestDbConnection : DbConnection
    {
        private ConnectionState _state = ConnectionState.Closed;

        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => "Test";
        public override string DataSource => "Test";
        public override string ServerVersion => "Test";
        public override ConnectionState State => _state;

        public override void ChangeDatabase(string databaseName) => throw new NotImplementedException();

        public override void Close()
        {
            _state = ConnectionState.Closed;
        }

        public override void Open()
        {
            _state = ConnectionState.Open;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new TestDbTransaction(this, isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new TestDbCommand();
        }
    }

    internal class TestDbTransaction : DbTransaction
    {
        public TestDbTransaction(DbConnection connection, IsolationLevel isolationLevel)
        {
            DbConnection = connection;
            IsolationLevel = isolationLevel;
        }

        public override IsolationLevel IsolationLevel { get; }
        protected override DbConnection DbConnection { get; }

        public override void Commit() { }
        public override void Rollback() { }
    }

    internal class TestDbCommand : DbCommand
    {
        private DbParameterCollection _parameters = new TestDbParameterCollection();

        public override string CommandText { get; set; } = string.Empty;
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection => _parameters;
        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel() { }

        public override int ExecuteNonQuery() => 0;

        public override object ExecuteScalar() => 0;

        public override void Prepare() { }

        protected override DbParameter CreateDbParameter() => new TestDbParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => new TestDbDataReader();
    }

    internal class TestDbParameter : DbParameter
    {
        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; set; } = string.Empty;
        public override int Size { get; set; }
        public override string SourceColumn { get; set; } = string.Empty;
        public override bool SourceColumnNullMapping { get; set; }
        public override object Value { get; set; }

        public override void ResetDbType() { }
    }

    internal class TestDbParameterCollection : DbParameterCollection
    {
        private readonly System.Collections.Generic.Dictionary<string, DbParameter> _parameters = new();

        public override int Count => _parameters.Count;
        public override object SyncRoot => ((System.Collections.ICollection)_parameters).SyncRoot;

        public override int Add(object value)
        {
            var param = (DbParameter)value;
            _parameters[param.ParameterName] = param;
            return _parameters.Count - 1;
        }

        public override void AddRange(Array values) => throw new NotImplementedException();
        public override void Clear() => _parameters.Clear();
        public override bool Contains(object value) => throw new NotImplementedException();
        public override bool Contains(string value) => _parameters.ContainsKey(value);
        public override void CopyTo(Array array, int index) => throw new NotImplementedException();
        public override System.Collections.IEnumerator GetEnumerator() => _parameters.Values.GetEnumerator();
        public override int IndexOf(object value) => throw new NotImplementedException();
        public override int IndexOf(string parameterName) => throw new NotImplementedException();
        public override void Insert(int index, object value) => throw new NotImplementedException();
        public override void Remove(object value) => throw new NotImplementedException();
        public override void RemoveAt(int index) => throw new NotImplementedException();
        public override void RemoveAt(string parameterName) => _parameters.Remove(parameterName);

        protected override DbParameter GetParameter(int index) => throw new NotImplementedException();
        protected override DbParameter GetParameter(string parameterName) => _parameters[parameterName];
        protected override void SetParameter(int index, DbParameter value) => throw new NotImplementedException();
        protected override void SetParameter(string parameterName, DbParameter value) => _parameters[parameterName] = value;
    }

    internal class TestDbDataReader : DbDataReader
    {
        public override object this[int ordinal] => throw new NotImplementedException();
        public override object this[string name] => throw new NotImplementedException();
        public override int Depth => 0;
        public override int FieldCount => 0;
        public override bool HasRows => false;
        public override bool IsClosed => false;
        public override int RecordsAffected => 0;

        public override bool GetBoolean(int ordinal) => throw new NotImplementedException();
        public override byte GetByte(int ordinal) => throw new NotImplementedException();
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => throw new NotImplementedException();
        public override char GetChar(int ordinal) => throw new NotImplementedException();
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => throw new NotImplementedException();
        public override string GetDataTypeName(int ordinal) => throw new NotImplementedException();
        public override DateTime GetDateTime(int ordinal) => throw new NotImplementedException();
        public override decimal GetDecimal(int ordinal) => throw new NotImplementedException();
        public override double GetDouble(int ordinal) => throw new NotImplementedException();
        public override System.Collections.IEnumerator GetEnumerator() => throw new NotImplementedException();
        public override Type GetFieldType(int ordinal) => throw new NotImplementedException();
        public override float GetFloat(int ordinal) => throw new NotImplementedException();
        public override Guid GetGuid(int ordinal) => throw new NotImplementedException();
        public override short GetInt16(int ordinal) => throw new NotImplementedException();
        public override int GetInt32(int ordinal) => throw new NotImplementedException();
        public override long GetInt64(int ordinal) => throw new NotImplementedException();
        public override string GetName(int ordinal) => throw new NotImplementedException();
        public override int GetOrdinal(string name) => throw new NotImplementedException();
        public override string GetString(int ordinal) => throw new NotImplementedException();
        public override object GetValue(int ordinal) => throw new NotImplementedException();
        public override int GetValues(object[] values) => throw new NotImplementedException();
        public override bool IsDBNull(int ordinal) => throw new NotImplementedException();
        public override bool NextResult() => false;
        public override bool Read() => false;
    }
}
