using System;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Defines a policy for transforming or handling exceptions thrown during database operations.
	/// </summary>
	public interface IExceptionPolicy
	{
		/// <summary>
		/// Transforms or wraps the given exception according to the policy's rules.
		/// </summary>
		/// <param name="ex">The original exception to transform.</param>
		/// <returns>The transformed exception, which may be the original exception, a wrapped exception, or a completely different exception.</returns>
		Exception GetException(Exception ex);
	}
}
