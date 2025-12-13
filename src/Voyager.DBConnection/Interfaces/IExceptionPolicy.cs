using System;

namespace Voyager.DBConnection.Interfaces
{
	public interface IExceptionPolicy
	{
		Exception GetException(Exception ex);
	}

	public interface IMapErrorPolicy
	{
		Voyager.Common.Results.Error MapError(Exception ex);
	}
}
