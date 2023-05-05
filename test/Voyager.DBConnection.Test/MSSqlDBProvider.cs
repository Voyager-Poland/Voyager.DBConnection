using System.Data.Common;
using System.Data.SqlClient;

namespace Voyager.DBConnection.Test
{
	internal class MSSqlDBProvider
	{
		static MSSqlDBProvider()
		{
			DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
		}

		public static DbProviderFactory GetSqlProvider()
		{
			return DbProviderFactories.GetFactory("Microsoft.Data.SqlClient");
		}
	}
}
