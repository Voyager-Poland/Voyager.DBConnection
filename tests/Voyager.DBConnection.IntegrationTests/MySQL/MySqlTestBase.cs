using MySql.Data.MySqlClient;
using System.Data.Common;
using Voyager.DBConnection.IntegrationTests.Infrastructure;

namespace Voyager.DBConnection.IntegrationTests.MySQL;

[Category("Integration")]
[Category("MySQL")]
public abstract class MySqlTestBase : DatabaseTestBase
{
    protected override DatabaseProvider DatabaseProvider => DatabaseProvider.MySQL;

    protected override void RegisterDbProviderFactory()
    {
        DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySqlClientFactory.Instance);
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
            Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT 1"))
                .TapError(error => throw new InvalidOperationException(error.Message));
        }
        catch
        {
            Assert.Ignore("MySQL database is not available. Please ensure Docker container is running.");
        }
    }

    protected void CleanupTestData()
    {
        // Delete in correct order due to foreign keys
        try
        {
            ExecuteNonQuery("DELETE FROM OrderItems");
            ExecuteNonQuery("DELETE FROM Orders");
            ExecuteNonQuery("DELETE FROM Products WHERE ProductId > 4"); // Keep initial test data
            ExecuteNonQuery("DELETE FROM Users WHERE UserId > 3"); // Keep initial test data

            // Also delete any test users that may have been created
            ExecuteNonQuery("DELETE FROM Users WHERE Username LIKE '%test%' OR Username LIKE '%bind%' OR Username LIKE '%tap%' OR Username LIKE '%map%' OR Username LIKE '%async%' OR Username LIKE '%delete%' OR Username LIKE '%duplicate%'");
        }
        catch
        {
            // Ignore cleanup errors - database might be empty
        }
    }
}
