using System;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	class NoExceptiopnPolicy : IExceptionPolicy
	{
		public Exception GetException(Exception ex)
		{
			return ex;
		}
	}
}
