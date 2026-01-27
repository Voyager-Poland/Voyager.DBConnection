using System.Data.Common;
using Voyager.Common.Results;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Defines a mechanism for reading output parameters from executed database commands.
	/// </summary>
	public interface IReadOutParameters
	{
		/// <summary>
		/// Reads output parameters from an executed command and processes them.
		/// </summary>
		/// <param name="db">The database instance providing context for parameter processing.</param>
		/// <param name="command">The executed command containing output parameters to read.</param>
		void ReadOutParameters(Database db, DbCommand command);
	}
}
