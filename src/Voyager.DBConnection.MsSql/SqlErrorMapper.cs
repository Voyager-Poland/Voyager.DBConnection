using System;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using Voyager.Common.Results;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.MsSql
{
	public class SqlErrorMapper : IMapErrorPolicy
	{
		public Error MapError(Exception ex)
		{

			SqlException sqlException = ex as SqlException;
			if (sqlException != null)
			{
				if (sqlException.Number == ErrorCodes.DeadLockNumber)
					return Error.UnavailableError(sqlException.Number.ToString(), sqlException.Message);

				if (sqlException.Number == ErrorCodes.Timeout_adonetNumber)
					return Error.TimeoutError(sqlException.Number.ToString(), sqlException.Message);

				if (sqlException.Number == ErrorCodes.SqlUniqueConstraintViolation)
					return Error.ConflictError(sqlException.Number.ToString(), sqlException.Message);
				return Error.DatabaseError(sqlException.Number.ToString(), sqlException.Message);

			}
			return Error.FromException(ex);
		}

	}
}
