using System;

namespace Voyager.DBConnection.Interfaces
{
    /// <summary>
    /// Defines a policy for mapping exceptions to error results during database operations.
    /// </summary>
    public interface IMapErrorPolicy
    {
        /// <summary>
        /// Maps an exception to an error result.
        /// </summary>
        /// <param name="ex">The exception to map.</param>
        /// <returns>An <see cref="Voyager.Common.Results.Error"/> representing the mapped exception.</returns>
        Voyager.Common.Results.Error MapError(Exception ex);
    }
}
