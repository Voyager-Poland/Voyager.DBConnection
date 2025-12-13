using System.Data.Common;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Defines a factory for creating and configuring database commands.
	/// </summary>
	public interface ICommandFactory : IReadOutParameters
	{
		/// <summary>
		/// Constructs and configures a database command using the provided database connection.
		/// </summary>
		/// <param name="db">The database instance used to create the command.</param>
		/// <returns>A configured <see cref="DbCommand"/> ready for execution.</returns>
		DbCommand ConstructDbCommand(Database db);
	}

}
