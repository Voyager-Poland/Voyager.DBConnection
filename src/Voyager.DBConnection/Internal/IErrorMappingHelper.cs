using System;
using Voyager.Common.Results;

namespace Voyager.DBConnection.Internal
{
	/// <summary>
	/// Interface for helper class responsible for mapping exceptions to Error objects.
	/// </summary>
	internal interface IErrorMappingHelper
	{
		/// <summary>
		/// Maps an exception to an Error using the configured error policy.
		/// </summary>
		Error MapException(Exception ex);
	}
}
