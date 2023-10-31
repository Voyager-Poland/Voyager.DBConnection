namespace System.Data.Common
{
	public static class DbCommandType
	{
		public static DbCommand GetStroredProcedure(this DbProviderFactory dbProviderFactory, string procName)
		{
			var cmd = dbProviderFactory.CreateCommand();
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = procName;
			return cmd;
		}

		public static DbCommand GetSqlCommand(this DbProviderFactory dbProviderFactory, string sqlValue)
		{
			var cmd = dbProviderFactory.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sqlValue;
			return cmd;
		}

	}
}
