using System;
using Microsoft.Data.Sqlite;
using Voyager.Common.Results;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.Sqlite
{
	public class SqliteErrorMapper : IMapErrorPolicy
	{
		public Error MapError(Exception ex)
		{
			SqliteException sqliteException = ex as SqliteException;
			if (sqliteException != null)
			{
				if (sqliteException.SqliteErrorCode == ErrorCodes.SQLITE_BUSY ||
				    sqliteException.SqliteErrorCode == ErrorCodes.SQLITE_LOCKED)
					return Error.UnavailableError(sqliteException.SqliteErrorCode.ToString(), sqliteException.Message);

				if (sqliteException.SqliteErrorCode == ErrorCodes.SQLITE_CONSTRAINT)
					return Error.DatabaseError(sqliteException.SqliteErrorCode.ToString(), sqliteException.Message);

				return Error.DatabaseError(sqliteException.SqliteErrorCode.ToString(), sqliteException.Message);
			}
			return Error.FromException(ex);
		}
	}
}
