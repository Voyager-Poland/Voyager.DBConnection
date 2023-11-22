namespace Voyager.DBConnection.Test
{
	abstract class BasicCall
	{
		Voyager.DBConnection.Connection connection;

		[SetUp]
		public void Setup()
		{
			Prepare();
			connection = GetConnection();
		}

		protected virtual void Prepare()
		{

		}

		[TearDown]
		public void TearDown()
		{
			connection.Dispose();
		}

		[Test]
		public virtual void WaitFor()
		{
			WaitForCommand waitForCommand = new WaitForCommand();
			connection.ExecuteNonQuery(waitForCommand);
		}

		protected virtual Connection GetConnection()
		{
			return new Connection(new Database("Data Source=devbus; Initial Catalog=ProxyAuth; Integrated Security = true;", MSSqlDBProvider.GetSqlProvider()));
		}
	}

}