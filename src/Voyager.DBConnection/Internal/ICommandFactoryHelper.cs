using System;
using System.Data.Common;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.Internal
{
	/// <summary>
	/// Interface for helper class responsible for creating DbCommand instances from various sources.
	/// </summary>
	internal interface ICommandFactoryHelper
	{
		/// <summary>
		/// Creates a command from IDbCommandFactory.
		/// </summary>
		DbCommand CreateCommand(IDbCommandFactory commandFactory);

		/// <summary>
		/// Creates a command factory delegate from IDbCommandFactory.
		/// </summary>
		Func<DbCommand> CreateCommandFactory(IDbCommandFactory commandFactory);

		/// <summary>
		/// Creates a command factory delegate from a command function.
		/// </summary>
		Func<DbCommand> CreateCommandFactory(Func<IDatabase, DbCommand> commandFunction);

		/// <summary>
		/// Creates a command factory delegate from a stored procedure name.
		/// </summary>
		Func<DbCommand> CreateCommandFactory(string procedureName);
	}
}
