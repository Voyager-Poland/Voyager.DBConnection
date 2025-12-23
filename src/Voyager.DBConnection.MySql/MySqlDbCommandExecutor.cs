namespace Voyager.DBConnection.MySql
{
	public class MySqlDbCommandExecutor : Voyager.DBConnection.DbCommandExecutor
	{
		public MySqlDbCommandExecutor(string mySqlConnectionString)
			: base(new MySqlDatabase(mySqlConnectionString), new MySqlErrorMapper())
		{
		}
	}
}
