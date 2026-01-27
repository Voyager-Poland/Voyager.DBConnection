using System;
using MySql.Data.MySqlClient;
using Voyager.Common.Results;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.MySql
{
	public class MySqlErrorMapper : IMapErrorPolicy
	{
		public Error MapError(Exception ex)
		{
			MySqlException mySqlException = ex as MySqlException;
			if (mySqlException != null)
			{
				if (mySqlException.Number == ErrorCodes.DuplicateEntry)
					return Error.ConflictError(mySqlException.Number.ToString(), mySqlException.Message);

				if (mySqlException.Number == ErrorCodes.DeadlockFound)
					return Error.UnavailableError(mySqlException.Number.ToString(), mySqlException.Message);

				if (mySqlException.Number == ErrorCodes.LockWaitTimeout)
					return Error.TimeoutError(mySqlException.Number.ToString(), mySqlException.Message);

				return Error.DatabaseError(mySqlException.Number.ToString(), mySqlException.Message);
			}
			return Error.FromException(ex);
		}
	}
}
