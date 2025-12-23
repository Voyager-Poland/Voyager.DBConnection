namespace Voyager.DBConnection.Sqlite
{
	public class SqliteDbCommandExecutor : Voyager.DBConnection.DbCommandExecutor
	{
		public SqliteDbCommandExecutor(string sqliteConnectionString)
			: base(new SqliteDatabase(sqliteConnectionString), new SqliteErrorMapper())
		{
		}
	}
}
