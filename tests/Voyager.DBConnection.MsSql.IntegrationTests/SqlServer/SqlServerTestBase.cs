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
    }

    [TearDown]
    public virtual void TearDown()
    {
        CleanupTestData();
    }

    private void EnsureDatabaseExists()
    {
        // Test connection by executing a simple query
        var result = Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT 1"));
        
        if (!result.IsSuccess)
        {
            Assert.Fail($"SQL Server database is not available. Please ensure Docker container is running.\n\nError: {result.Error.Message}\n\nConnection String: {Configuration.GetConnectionString("SqlServer")}");
        }
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

    protected void ExecuteNonQuery(string sql)
    {
        _ = Executor!.ExecuteNonQuery(db => db.GetSqlCommand(sql))
            .TapError(error => throw new InvalidOperationException($"Failed to execute SQL: {error.Message}"));
    }

    protected T? ExecuteScalar<T>(string sql)
    {
        var result = Executor!.ExecuteScalar(db => db.GetSqlCommand(sql))
            .Map(value => value == null || value == DBNull.Value ? default(T) : (T)value)
            .TapError(error => throw new InvalidOperationException($"Failed to execute SQL: {error.Message}"));
        if (!result.IsSuccess)
            throw new InvalidOperationException("Unexpected failure");
        return result.Value;
    }
}
