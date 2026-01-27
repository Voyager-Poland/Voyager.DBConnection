namespace System.Data.Common
{
	/// <summary>
	/// Extension methods for DbProviderFactory to create configured DbCommand instances.
	/// </summary>
	public static class DbCommandType
	{
		/// <summary>
		/// Creates a DbCommand configured for executing a stored procedure.
		/// </summary>
		/// <param name="dbProviderFactory">The database provider factory.</param>
		/// <param name="procName">The name of the stored procedure.</param>
		/// <returns>A DbCommand configured with CommandType.StoredProcedure.</returns>
		public static DbCommand GetStroredProcedure(this DbProviderFactory dbProviderFactory, string procName)
		{
			var cmd = dbProviderFactory.CreateCommand();
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = procName;
			return cmd;
		}

		/// <summary>
		/// Creates a DbCommand configured for executing a SQL text command.
		/// </summary>
		/// <param name="dbProviderFactory">The database provider factory.</param>
		/// <param name="sqlValue">The SQL command text.</param>
		/// <returns>A DbCommand configured with CommandType.Text.</returns>
		public static DbCommand GetSqlCommand(this DbProviderFactory dbProviderFactory, string sqlValue)
		{
			var cmd = dbProviderFactory.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sqlValue;
			return cmd;
		}

	}
}
