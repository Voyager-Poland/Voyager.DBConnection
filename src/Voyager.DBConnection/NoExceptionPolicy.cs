using System;
using Voyager.Common.Results;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	class NoExceptionPolicy : IExceptionPolicy
	{
		public Exception GetException(Exception ex)
		{
			return ex;
		}
	}

	public class DefaultMapError : IMapErrorPolicy
	{
		public Voyager.Common.Results.Error MapError(Exception ex)
		{
			return Error.FromException(ex);
		}
	}
}
