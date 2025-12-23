namespace Voyager.DBConnection.PostgreSql
{
	public class PostgreSqlDbCommandExecutor : Voyager.DBConnection.DbCommandExecutor
	{
		public PostgreSqlDbCommandExecutor(string postgreSqlConnectionString)
			: base(new PostgreSqlDatabase(postgreSqlConnectionString), new PostgreSqlErrorMapper())
		{
		}
	}
}
