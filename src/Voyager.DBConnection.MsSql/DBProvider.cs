using System.Data.Common;

namespace Voyager.DBConnection.MsSql
{
	public class DBProvider
	{
		public DbProviderFactory GetSqlProvider()
		{
#if NETFRAMEWORK
			return DbProviderFactories.GetFactory("System.Data.SqlClient");
#else
			return global::Microsoft.Data.SqlClient.SqlClientFactory.Instance;
#endif
		}
	}
}
