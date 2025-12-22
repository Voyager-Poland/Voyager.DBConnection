using Microsoft.Extensions.Configuration;

namespace Voyager.DBConnection.MsSql.IntegrationTests;

[Category("Integration")]
[Category("SqlServer")]
[Category("MsSql")]
public abstract class SqlServerTestBase
{
    protected SqlDbCommandExecutor? Executor { get; private set; }
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
        var connectionString = Configuration.GetConnectionString("SqlServer");
        if (string.IsNullOrEmpty(connectionString))
        {
            Assert.Ignore("SQL Server connection string not configured");
        }

        Executor = new SqlDbCommandExecutor(connectionString);
        EnsureDatabaseExists();
    }

    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
        Executor?.Dispose();
    }

    [SetUp]
    public virtual void SetUp()
    {
        // Can be overridden for test-specific setup
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Can be overridden for test-specific cleanup
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

    protected void ExecuteNonQuery(string sql)
    {
        Executor!.ExecuteNonQuery(db => db.GetSqlCommand(sql))
            .TapError(error => throw new InvalidOperationException($"Failed to execute SQL: {error.Message}"));
    }

    protected T? ExecuteScalar<T>(string sql)
    {
        return Executor!.ExecuteScalar(db => db.GetSqlCommand(sql))
            .Map(value => value == null || value == DBNull.Value ? default(T) : (T)value)
            .TapError(error => throw new InvalidOperationException($"Failed to execute SQL: {error.Message}"))
            .Value;
    }
}
