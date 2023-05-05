using System.Data.Common;
using System.Data.SqlClient;

namespace Voyager.DBConnection.MsSql
{
	public class DBProvider
	{
		static DBProvider()
		{
			DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
		}

		public DbProviderFactory GetSqlProvider()
		{
			return DbProviderFactories.GetFactory("Microsoft.Data.SqlClient");
		}
	}
}
