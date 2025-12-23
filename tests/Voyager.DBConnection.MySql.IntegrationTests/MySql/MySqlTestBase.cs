using Microsoft.Extensions.Configuration;
using Voyager.Common.Results;
using Voyager.DBConnection.MySql;

namespace Voyager.DBConnection.MySql.IntegrationTests.MySql;

[Category("Integration")]
[Category("MySql")]
public abstract class MySqlTestBase
{
    protected MySqlDbCommandExecutor? Executor { get; private set; }
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
        var connectionString = Configuration.GetConnectionString("MySql");
        if (string.IsNullOrEmpty(connectionString))
        {
            Assert.Ignore("MySQL connection string not configured");
        }

        try
        {
            Executor = new MySqlDbCommandExecutor(connectionString);
            EnsureDatabaseExists();
        }
        catch (Exception ex)
        {
            Assert.Ignore($"MySQL database is not available: {ex.Message}");
        }
    }

    [SetUp]
    public virtual void SetUp()
    {
        if (Executor == null)
        {
            Assert.Ignore("MySQL database connection not initialized");
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
            throw new InvalidOperationException($"Failed to connect to MySQL database: {ex.Message}", ex);
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
        // MySQL table names are case-insensitive on Windows, case-sensitive on Linux
        ExecuteNonQuery("DELETE FROM OrderItems");
        ExecuteNonQuery("DELETE FROM Orders");
        ExecuteNonQuery("DELETE FROM Products WHERE ProductId > 4"); // Keep initial test data
        ExecuteNonQuery("DELETE FROM Users WHERE UserId > 3"); // Keep initial test data
    }
}
