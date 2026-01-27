namespace Voyager.DBConnection.PostgreSql
{
	/// <summary>
	/// PostgreSQL-specific implementation of Database class.
	/// Provides a simplified constructor that automatically uses NpgsqlFactory.
	/// </summary>
	public class PostgreSqlDatabase : Database
	{
		/// <summary>
		/// Initializes a new instance of PostgreSqlDatabase with PostgreSQL connection string.
		/// </summary>
		/// <param name="postgreSqlConnectionString">PostgreSQL connection string</param>
		public PostgreSqlDatabase(string postgreSqlConnectionString)
			: base(postgreSqlConnectionString, new DBProvider().GetPostgreSqlProvider())
		{
		}
	}
}
