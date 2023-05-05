namespace Voyager.DBConnection.MsSql
{
	public class SqlDatabase : Voyager.DBConnection.Database
	{
		public SqlDatabase(string sqlConnectionString) : base(sqlConnectionString, new DBProvider().GetSqlProvider())
		{
			this.ParameterToken = '@';
		}

	}
}
