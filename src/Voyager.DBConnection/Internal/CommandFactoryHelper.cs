using System;
using System.Data.Common;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.Internal
{
	/// <summary>
	/// Helper class responsible for creating DbCommand instances from various sources.
	/// Separates command creation logic from DbCommandExecutor.
	/// </summary>
	internal class CommandFactoryHelper : ICommandFactoryHelper
	{
		private readonly IDatabaseInternal database;

		public CommandFactoryHelper(IDatabaseInternal database)
		{
			this.database = database;
		}

		/// <summary>
		/// Creates a command from IDbCommandFactory.
		/// </summary>
		public DbCommand CreateCommand(IDbCommandFactory commandFactory)
		{
			return commandFactory.ConstructDbCommand(database);
		}

		/// <summary>
		/// Creates a command factory delegate from IDbCommandFactory.
		/// </summary>
		public Func<DbCommand> CreateCommandFactory(IDbCommandFactory commandFactory)
		{
			return () => CreateCommand(commandFactory);
		}

		/// <summary>
		/// Creates a command factory delegate from a command function.
		/// </summary>
		public Func<DbCommand> CreateCommandFactory(Func<IDatabase, DbCommand> commandFunction)
		{
			return () => commandFunction(database);
		}

		/// <summary>
		/// Creates a command factory delegate from a stored procedure name.
		/// </summary>
		public Func<DbCommand> CreateCommandFactory(string procedureName)
		{
			return () => database.GetStoredProcCommand(procedureName);
		}
	}
}
