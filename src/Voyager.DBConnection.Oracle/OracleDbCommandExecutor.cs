namespace Voyager.DBConnection.Oracle
{
	public class OracleDbCommandExecutor : Voyager.DBConnection.DbCommandExecutor
	{
		public OracleDbCommandExecutor(string oracleConnectionString)
			: base(new OracleDatabase(oracleConnectionString), new OracleErrorMapper())
		{
		}
	}
}
