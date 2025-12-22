using System;
using System.Data.SqlClient;
using Voyager.Common.Results;
using Voyager.DBConnection.Exceptions;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.MsSql
{
	public class ExceptionFactory : IExceptionPolicy
	{
		public Exception GetException(Exception ex)
		{

#pragma warning disable IDE0019 // Use pattern matching
			SqlException sqlException = ex as SqlException;
#pragma warning restore IDE0019 // Use pattern matching
			if (sqlException != null)
			{
				if (sqlException.Class >= 20)
					return new SqlServiceException(ex.Message, ex);

				if (sqlException.Number == ErrorCodes.DeadLockNumber)
					return new DeadlockException(ex.Message, ex);

				if (sqlException.Number == ErrorCodes.ConnectionNumber)
					return new SqlConnectionException(ex.Message, ex);

				if (sqlException.Number == ErrorCodes.Readony)
					return new SqlConnectionException(ex.Message, ex);

				if (sqlException.Number == ErrorCodes.SessionExpired)
					return new SessionExpiredException(ex.Message, ex);

				if (sqlException.Number == ErrorCodes.Timeout_adonetNumber)
					return new TimeOutException(ex.Message, ex);
			}

			return ex;

		}

	}
}
