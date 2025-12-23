using System.Data.Common;

namespace Voyager.DBConnection.Sqlite
{
	public class DBProvider
	{
		static DBProvider()
		{
#if NETCORE
			DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", global::Microsoft.Data.Sqlite.SqliteFactory.Instance);
#endif
		}

		public DbProviderFactory GetSqliteProvider()
		{
			return DbProviderFactories.GetFactory("Microsoft.Data.Sqlite");
		}
	}
}
