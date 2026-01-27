using System.Data.Common;

namespace Voyager.DBConnection.PostgreSql
{
	public class DBProvider
	{
		public DbProviderFactory GetPostgreSqlProvider()
		{
			// Directly return the factory instance
			return global::Npgsql.NpgsqlFactory.Instance;
		}
	}
}
