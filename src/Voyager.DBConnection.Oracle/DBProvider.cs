using System.Data.Common;

namespace Voyager.DBConnection.Oracle
{
	public class DBProvider
	{
		static DBProvider()
		{
#if NETCORE
			DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess.Client", global::Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance);
#endif
		}

		public DbProviderFactory GetOracleProvider()
		{
			return DbProviderFactories.GetFactory("Oracle.ManagedDataAccess.Client");
		}
	}
}
