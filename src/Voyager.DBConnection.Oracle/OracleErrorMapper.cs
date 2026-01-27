using System;
using Oracle.ManagedDataAccess.Client;
using Voyager.Common.Results;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.Oracle
{
	public class OracleErrorMapper : IMapErrorPolicy
	{
		public Error MapError(Exception ex)
		{
			OracleException oracleException = ex as OracleException;
			if (oracleException != null)
			{
				if (oracleException.Number == ErrorCodes.DeadlockDetected)
					return Error.UnavailableError(oracleException.Number.ToString(), oracleException.Message);

				if (oracleException.Number == ErrorCodes.Timeout)
					return Error.TimeoutError(oracleException.Number.ToString(), oracleException.Message);
				
				if (oracleException.Number == ErrorCodes.UniqueConstraintViolation)
					return Error.ConflictError(oracleException.Number.ToString(), oracleException.Message);
				return Error.DatabaseError(oracleException.Number.ToString(), oracleException.Message);
			}
			return Error.FromException(ex);
		}
	}
}
