using Microsoft.Data.SqlClient;
using System.Data.Common;
using Voyager.DBConnection.IntegrationTests.Infrastructure;

namespace Voyager.DBConnection.IntegrationTests.SqlServer;

[Category("Integration")]
[Category("SqlServer")]
public abstract class SqlServerTestBase : DatabaseTestBase
{
    protected override DatabaseProvider DatabaseProvider => DatabaseProvider.SqlServer;

    protected override void RegisterDbProviderFactory()
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
    }

    [OneTimeSetUp]
    public override void OneTimeSetUp()
    {
        base.OneTimeSetUp();
        EnsureDatabaseExists();
    }

    private void EnsureDatabaseExists()
    {
        try
        {
            // Test connection by executing a simple query
            _ = Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT 1"))
                .TapError(error => throw new InvalidOperationException(error.Message));
        }
        catch
        {
            Assert.Ignore("SQL Server database is not available. Please ensure Docker container is running.");
        }
    }

    protected void CleanupTestData()
    {
        ExecuteNonQuery("DELETE FROM OrderItems");
        ExecuteNonQuery("DELETE FROM Orders");
        ExecuteNonQuery("DELETE FROM Products WHERE ProductId > 4"); // Keep initial test data
        ExecuteNonQuery("DELETE FROM Users WHERE UserId > 3"); // Keep initial test data
    }
}
