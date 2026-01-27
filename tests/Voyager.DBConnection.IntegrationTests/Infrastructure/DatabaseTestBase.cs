using System.Data.Common;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for database integration tests
/// </summary>
[TestFixture]
public abstract class DatabaseTestBase
{
    protected Database? Database { get; private set; }
    protected DbCommandExecutor? Executor { get; private set; }
    protected abstract DatabaseProvider DatabaseProvider { get; }

    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        // Register DbProviderFactory
        RegisterDbProviderFactory();

        // Create database instance
        var connectionString = TestConfiguration.GetConnectionString(DatabaseProvider);
        var providerName = TestConfiguration.GetProviderName(DatabaseProvider);
        var factory = DbProviderFactories.GetFactory(providerName);

        Database = new Database(connectionString, factory);

        // Create executor with error policy
        var errorPolicy = CreateErrorPolicy();
        Executor = new DbCommandExecutor(Database, errorPolicy);
    }

    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
        Executor?.Dispose();
        Database?.Dispose();
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

    protected abstract void RegisterDbProviderFactory();

    protected virtual IMapErrorPolicy CreateErrorPolicy()
    {
        return DatabaseProvider switch
        {
            DatabaseProvider.SqlServer => new SqlServerErrorPolicy(),
            DatabaseProvider.PostgreSQL => new PostgreSqlErrorPolicy(),
            DatabaseProvider.MySQL => new MySqlErrorPolicy(),
            DatabaseProvider.Oracle => new OracleErrorPolicy(),
            DatabaseProvider.SQLite => new DefaultMapError(),
            _ => new DefaultMapError()
        };
    }

    protected void ExecuteNonQuery(string sql)
    {
        Executor!.ExecuteNonQuery(db => db.GetSqlCommand(sql)).TapError(error =>
            throw new InvalidOperationException($"Failed to execute SQL: {error.Message}"));
    }

    protected T? ExecuteScalar<T>(string sql)
    {
        return Executor!.ExecuteScalar(db => db.GetSqlCommand(sql))
            .Map(value => value == null || value == DBNull.Value ? default(T) : (T)value)
            .TapError(error => throw new InvalidOperationException($"Failed to execute SQL: {error.Message}"))
            .Value;
    }
}
