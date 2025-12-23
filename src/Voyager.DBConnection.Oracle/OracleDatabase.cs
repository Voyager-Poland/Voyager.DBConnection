namespace Voyager.DBConnection.Oracle
{
	/// <summary>
	/// Oracle-specific implementation of Database class.
	/// Provides a simplified constructor that automatically uses OracleClientFactory.
	/// </summary>
	public class OracleDatabase : Database
	{
		/// <summary>
		/// Initializes a new instance of OracleDatabase with Oracle connection string.
		/// </summary>
		/// <param name="oracleConnectionString">Oracle connection string</param>
		public OracleDatabase(string oracleConnectionString)
			: base(oracleConnectionString, new DBProvider().GetOracleProvider())
		{
		}
	}
}
