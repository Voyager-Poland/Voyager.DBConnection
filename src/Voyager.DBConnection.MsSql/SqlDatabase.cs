namespace Voyager.DBConnection.MsSql
{
	/// <summary>
	/// SQL Server-specific implementation of Database class.
	/// Provides a simplified constructor that automatically uses SqlClientFactory.
	/// </summary>
	public class SqlDatabase : Database
	{
		/// <summary>
		/// Initializes a new instance of SqlDatabase with SQL Server connection string.
		/// </summary>
		/// <param name="sqlConnectionString">SQL Server connection string</param>
		public SqlDatabase(string sqlConnectionString)
			: base(sqlConnectionString, new DBProvider().GetSqlProvider())
		{
		}
	}
}
