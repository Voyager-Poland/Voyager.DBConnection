using System.Data.Common;

namespace Voyager.DBConnection.MySql
{
	public class DBProvider
	{
		public DbProviderFactory GetMySqlProvider()
		{
			// Directly return the factory instance
			return global::MySql.Data.MySqlClient.MySqlClientFactory.Instance;
		}
	}
}
