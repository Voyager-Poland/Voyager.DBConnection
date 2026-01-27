namespace Voyager.DBConnection.MySql
{
	/// <summary>
	/// MySQL-specific implementation of Database class.
	/// Provides a simplified constructor that automatically uses MySqlClientFactory.
	/// </summary>
	public class MySqlDatabase : Database
	{
		/// <summary>
		/// Initializes a new instance of MySqlDatabase with MySQL connection string.
		/// </summary>
		/// <param name="mySqlConnectionString">MySQL connection string</param>
		public MySqlDatabase(string mySqlConnectionString)
			: base(mySqlConnectionString, new DBProvider().GetMySqlProvider())
		{
		}
	}
}
