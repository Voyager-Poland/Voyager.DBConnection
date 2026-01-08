using Microsoft.Extensions.Configuration;
using Voyager.Common.Results;
using Voyager.DBConnection.Oracle;

namespace Voyager.DBConnection.Oracle.IntegrationTests.Oracle;

[Category("Integration")]
[Category("Oracle")]
public abstract class OracleTestBase
{
    protected OracleDbCommandExecutor? Executor { get; private set; }
    private IConfiguration? _configuration;

    protected IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddEnvironmentVariables()
                    .Build();
            }
            return _configuration;
        }
    }

    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        var connectionString = Configuration.GetConnectionString("Oracle");
        if (string.IsNullOrEmpty(connectionString))
        {
            Assert.Ignore("Oracle connection string not configured");
        }

            Executor = new OracleDbCommandExecutor(connectionString);
            EnsureDatabaseExists();
    }

    [SetUp]
    public virtual void SetUp()
    {
        if (Executor == null)
        {
            Assert.Ignore("Oracle database connection not initialized");
        }
    }

    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
        Executor?.Dispose();
    }

    private void EnsureDatabaseExists()
    {
            // Test connection by executing a simple query
            Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT 1 FROM DUAL"))
                .TapError(error => throw new InvalidOperationException(error.Message));
    }

    protected Result<int> ExecuteNonQuery(string commandText)
    {
        return Executor!.ExecuteNonQuery(db => db.GetSqlCommand(commandText));
    }

    protected Result<object> ExecuteScalar(string commandText)
    {
        return Executor!.ExecuteScalar(db => db.GetSqlCommand(commandText));
    }

    protected void CleanupTestData()
    {
        // Clean up test data in correct order (respect foreign keys)
        ExecuteNonQuery("DELETE FROM OrderItems");
        ExecuteNonQuery("DELETE FROM Orders");
        ExecuteNonQuery("DELETE FROM Products WHERE ProductId > 4"); // Keep initial test data
        
        // Delete test users created during tests (keep only the 3 initial users)
        ExecuteNonQuery("DELETE FROM Users WHERE Username NOT IN ('john_doe', 'jane_smith', 'bob_wilson')");
    }
}
