namespace Voyager.DBConnection.Test
{
	public class MockDataBase : Database
	{
		public MockDataBase() : base("Data Source=mockSql; Initial Catalog=FanyDB; Integrated Security = true;", new DbProviderFactoryMock())
		{
		}


	}
}
