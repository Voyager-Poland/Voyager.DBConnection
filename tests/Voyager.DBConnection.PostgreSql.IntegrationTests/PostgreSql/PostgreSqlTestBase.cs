using Microsoft.Extensions.Configuration;
using Voyager.Common.Results;
using Voyager.DBConnection.PostgreSql;

namespace Voyager.DBConnection.PostgreSql.IntegrationTests.PostgreSql;

[Category("Integration")]
[Category("PostgreSql")]
public abstract class PostgreSqlTestBase
{
    protected PostgreSqlDbCommandExecutor? Executor { get; private set; }
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
        var connectionString = Configuration.GetConnectionString("PostgreSql");
        if (string.IsNullOrEmpty(connectionString))
        {
            Assert.Ignore("PostgreSql connection string not configured");
        }

        try
        {
            Executor = new PostgreSqlDbCommandExecutor(connectionString);
            EnsureDatabaseExists();
        }
        catch (Exception ex)
        {
            Assert.Ignore($"PostgreSQL database is not available: {ex.Message}");
        }
    }

    [SetUp]
    public virtual void SetUp()
    {
        if (Executor == null)
        {
            Assert.Ignore("PostgreSQL database connection not initialized");
        }
    }

    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
        Executor?.Dispose();
    }

    private void EnsureDatabaseExists()
    {
        try
        {
            // Test connection by executing a simple query
            Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT 1"))
                .TapError(error => throw new InvalidOperationException(error.Message));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to connect to PostgreSQL database: {ex.Message}", ex);
        }
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
        // PostgreSQL uses lowercase table names by default
        ExecuteNonQuery("DELETE FROM orderitems");
        ExecuteNonQuery("DELETE FROM orders");
        ExecuteNonQuery("DELETE FROM products WHERE productid > 4"); // Keep initial test data
        ExecuteNonQuery("DELETE FROM users WHERE userid > 3"); // Keep initial test data
    }
}
