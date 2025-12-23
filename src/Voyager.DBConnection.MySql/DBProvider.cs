using System.Data.Common;

namespace Voyager.DBConnection.MySql
{
	public class DBProvider
	{
		static DBProvider()
		{
#if NETCORE
			DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", global::MySql.Data.MySqlClient.MySqlClientFactory.Instance);
#endif
		}

		public DbProviderFactory GetMySqlProvider()
		{
			return DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
		}
	}
}
