using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Voyager.DBConnection.MockServcie
{
	class MockDbCommand : DbCommand
	{
		public override string CommandText { get; set; } = "MockCommandText";
		public override int CommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public override CommandType CommandType { get; set; }
		public override bool DesignTimeVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public override UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		protected override DbConnection DbConnection { get; set; } = new MockConnection();
		DbParameterCollection dbParameterCollection = new MockDbParameterCollection();
		protected override DbParameterCollection DbParameterCollection => dbParameterCollection;

		protected override DbTransaction DbTransaction { get; set; }

		public override void Cancel()
		{
			throw new NotImplementedException();
		}

		public override int ExecuteNonQuery()
		{
			return 0;
		}

		public override object ExecuteScalar()
		{
			return 0;
		}

		public override void Prepare()
		{
			throw new NotImplementedException();
		}

		protected override DbParameter CreateDbParameter()
		{
			return new MockDbParameter();
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return new MockDbDataReader();
		}
	}

	class MockDbDataReader : DbDataReader
	{
		public override object this[int ordinal] => throw new NotImplementedException();

		public override object this[string name] => throw new NotImplementedException();

		public override int Depth => throw new NotImplementedException();

		public override int FieldCount => throw new NotImplementedException();

		public override bool HasRows => throw new NotImplementedException();

		public override bool IsClosed => throw new NotImplementedException();

		public override int RecordsAffected => throw new NotImplementedException();

		public override bool GetBoolean(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override byte GetByte(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override char GetChar(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override string GetDataTypeName(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override DateTime GetDateTime(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override decimal GetDecimal(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override double GetDouble(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public override Type GetFieldType(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override float GetFloat(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override Guid GetGuid(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override short GetInt16(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override int GetInt32(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override long GetInt64(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override string GetName(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override int GetOrdinal(string name)
		{
			throw new NotImplementedException();
		}

		public override string GetString(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override object GetValue(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public override bool IsDBNull(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override bool NextResult()
		{
			return false;
		}

		public override bool Read()
		{
			throw new NotImplementedException();
		}
	}
}