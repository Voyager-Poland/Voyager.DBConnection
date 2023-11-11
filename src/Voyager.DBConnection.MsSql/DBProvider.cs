using System.Data.Common;

namespace Voyager.DBConnection.MsSql
{
	public class DBProvider
	{
		static DBProvider()
		{

#if NETCORE
			DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);
#endif
		}

		public DbProviderFactory GetSqlProvider()
		{
#if NETCORE
			string providerName = "Microsoft.Data.SqlClient";
#else
			string providerName = "System.Data.SqlClient";
#endif
			return DbProviderFactories.GetFactory(providerName);
		}


	}
}
