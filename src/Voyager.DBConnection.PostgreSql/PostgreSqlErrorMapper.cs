using System;
using Npgsql;
using Voyager.Common.Results;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.PostgreSql
{
	public class PostgreSqlErrorMapper : IMapErrorPolicy
	{
		public Error MapError(Exception ex)
		{
			PostgresException postgresException = ex as PostgresException;
			if (postgresException != null)
			{
				if (postgresException.SqlState == ErrorCodes.DeadlockDetected)
					return Error.UnavailableError(postgresException.SqlState, postgresException.Message);

				if (postgresException.SqlState == ErrorCodes.QueryCanceled)
					return Error.TimeoutError(postgresException.SqlState, postgresException.Message);

				return Error.DatabaseError(postgresException.SqlState, postgresException.Message);
			}
			return Error.FromException(ex);
		}
	}
}
