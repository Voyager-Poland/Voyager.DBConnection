using System.Data.Common;

namespace Voyager.DBConnection.PostgreSql
{
	public class DBProvider
	{
		static DBProvider()
		{
#if NETCORE
			DbProviderFactories.RegisterFactory("Npgsql", global::Npgsql.NpgsqlFactory.Instance);
#endif
		}

		public DbProviderFactory GetPostgreSqlProvider()
		{
			return DbProviderFactories.GetFactory("Npgsql");
		}
	}
}
