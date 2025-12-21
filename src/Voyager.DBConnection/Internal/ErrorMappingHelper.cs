using System;
using Voyager.Common.Results;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.Internal
{
	/// <summary>
	/// Helper class responsible for mapping exceptions to Error objects.
	/// Separates error mapping logic from DbCommandExecutor.
	/// </summary>
	internal class ErrorMappingHelper : IErrorMappingHelper
	{
		private readonly IMapErrorPolicy errorPolicy;

		public ErrorMappingHelper(IMapErrorPolicy errorPolicy)
		{
			this.errorPolicy = errorPolicy;
		}

		/// <summary>
		/// Maps an exception to an Error using the configured error policy.
		/// </summary>
		public Error MapException(Exception ex)
		{
			return errorPolicy.MapError(ex);
		}
	}
}
