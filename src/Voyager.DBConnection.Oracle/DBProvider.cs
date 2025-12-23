using System.Data.Common;

namespace Voyager.DBConnection.Oracle
{
	public class DBProvider
	{
		public DbProviderFactory GetOracleProvider()
		{
			// Directly return the factory instance
			return global::Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance;
		}
	}
}
