using System.Data.Common;

namespace Voyager.DBConnection.MockServcie
{
	internal class DbProviderFactoryMock : DbProviderFactory
	{
		public override DbConnection CreateConnection()
		{
			return new MockConnection();
		}

		public override DbCommand CreateCommand()
		{
			return new MockDbCommand();
		}

		public override DbParameter CreateParameter()
		{
			return new Voyager.DBConnection.MockServcie.MockDbParameter();
		}
	}
}
