using System.Data.Common;

namespace Voyager.DBConnection
{
    /// <summary>
    /// Defines methods for creating database commands for stored procedures and SQL statements.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Creates a database command configured to execute a stored procedure.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <returns>A <see cref="DbCommand"/> configured for stored procedure execution.</returns>
        DbCommand GetStoredProcCommand(string procedureName);

        /// <summary>
        /// Creates a database command configured to execute a SQL statement.
        /// </summary>
        /// <param name="procedureName">The SQL statement or query to execute.</param>
        /// <returns>A <see cref="DbCommand"/> configured for SQL statement execution.</returns>
        DbCommand GetSqlCommand(string procedureName);
    }

}