namespace Voyager.DBConnection.MsSql
{
    public class SqlDbCommandExecutor : Voyager.DBConnection.DbCommandExecutor
	{
		public SqlDbCommandExecutor(string sqlConnectionString) : base(new SqlDatabase(sqlConnectionString), new SqlErrorMapper())
		{
		}
	}
}
