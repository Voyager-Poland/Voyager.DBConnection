using System.Data.Common;

namespace Voyager.DBConnection.Interfaces
{
    /// <summary>
    /// Factory interface for creating database commands.
    /// </summary>
    public interface IDbCommandFactory
    {
        /// <summary>
        /// Constructs a DbCommand with appropriate configuration.
        /// </summary>
        /// <param name="db">The database instance used to create the command.</param>
        /// <returns>A configured DbCommand ready for execution.</returns>
        DbCommand ConstructDbCommand(IDatabase db);
    }
}
