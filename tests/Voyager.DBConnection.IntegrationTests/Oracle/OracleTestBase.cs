using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using Voyager.DBConnection.IntegrationTests.Infrastructure;

namespace Voyager.DBConnection.IntegrationTests.Oracle;

[Category("Integration")]
[Category("Oracle")]
public abstract class OracleTestBase : DatabaseTestBase
{
    protected override DatabaseProvider DatabaseProvider => DatabaseProvider.Oracle;

    protected override void RegisterDbProviderFactory()
    {
        DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess.Client", OracleClientFactory.Instance);
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
            Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT 1 FROM DUAL"))
                .TapError(error => throw new InvalidOperationException(error.Message));
        }
        catch
        {
            Assert.Ignore("Oracle database is not available. Please ensure Docker container is running.");
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
