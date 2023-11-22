namespace Voyager.DBConnection.Test
{
	class MockDataBase : Database
	{
		public MockDataBase() : base("Data Source=mockSql; Initial Catalog=FanyDB; Integrated Security = true;", new DbProviderFactoryMock())
		{
		}
	}
}
