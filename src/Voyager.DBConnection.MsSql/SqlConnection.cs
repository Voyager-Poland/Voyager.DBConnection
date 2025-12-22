namespace Voyager.DBConnection.MsSql
{
	public class SqlConnection : Connection
	{

		public SqlConnection(string sqlConnectionString) : base(new SqlDatabase(sqlConnectionString), new ExceptionFactory())
		{
		}
	}
}
