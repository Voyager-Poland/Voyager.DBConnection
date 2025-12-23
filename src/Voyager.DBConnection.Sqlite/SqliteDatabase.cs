namespace Voyager.DBConnection.Sqlite
{
	/// <summary>
	/// SQLite-specific implementation of Database class.
	/// Provides a simplified constructor that automatically uses SqliteFactory.
	/// </summary>
	public class SqliteDatabase : Database
	{
		/// <summary>
		/// Initializes a new instance of SqliteDatabase with SQLite connection string.
		/// </summary>
		/// <param name="sqliteConnectionString">SQLite connection string</param>
		public SqliteDatabase(string sqliteConnectionString)
			: base(sqliteConnectionString, new DBProvider().GetSqliteProvider())
		{
		}
	}
}
