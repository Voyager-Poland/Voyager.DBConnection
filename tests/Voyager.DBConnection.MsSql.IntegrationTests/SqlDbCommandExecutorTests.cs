using System.Data;
using Microsoft.Extensions.Configuration;
using Voyager.DBConnection.Interfaces;
using Voyager.DBConnection.MsSql;

namespace Voyager.DBConnection.MsSql.IntegrationTests;

[TestFixture]
[Category("Integration")]
[Category("SqlServer")]
[Category("MsSql")]
public class SqlDbCommandExecutorTests
{
    private SqlDbCommandExecutor? _executor;
    private string? _connectionString;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        _connectionString = config.GetConnectionString("SqlServer");

        if (string.IsNullOrEmpty(_connectionString))
        {
            Assert.Ignore("SQL Server connection string not configured");
        }
    }

    [SetUp]
    public void SetUp()
    {
        _executor = new SqlDbCommandExecutor(_connectionString!);
        CleanupTestData();
    }

    [TearDown]
    public void TearDown()
    {
        _executor?.Dispose();
    }

    [Test]
    public void Constructor_WithValidConnectionString_ShouldCreateInstance()
    {
        // Arrange & Act
        var executor = new SqlDbCommandExecutor(_connectionString!);

        // Assert
        Assert.That(executor, Is.Not.Null);
        Assert.That(executor, Is.InstanceOf<DbCommandExecutor>());

        executor.Dispose();
    }

    [Test]
    public void ExecuteScalar_SimpleQuery_ShouldReturnResult()
    {
        // Act
        var result = _executor!.ExecuteScalar(
            db => db.GetSqlCommand("SELECT 1")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1));
    }

    [Test]
    public void ExecuteNonQuery_WithSqlDbCommandExecutor_ShouldUseSqlErrorMapper()
    {
        // Arrange - create first user
        _executor!.ExecuteNonQuery(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, "mapper_test_user")
                .WithInputParameter("Email", DbType.String, 100, "mapper@test.com")
                .WithInputParameter("Age", DbType.Int32, 30)
                .WithOutputParameter("UserId", DbType.Int32, 0)
        );

        // Act - try duplicate (should use SqlErrorMapper)
        var result = _executor!.ExecuteNonQuery(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, "mapper_test_user")
                .WithInputParameter("Email", DbType.String, 100, "mapper2@test.com")
                .WithInputParameter("Age", DbType.Int32, 30)
                .WithOutputParameter("UserId", DbType.Int32, 0)
        );

        // Assert - SqlErrorMapper should map this to DatabaseError
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(Voyager.Common.Results.ErrorType.Database));

        // Error code should be SQL Server specific (2627 or 2601)
        var errorCode = int.Parse(result.Error.Code);
        Assert.That(errorCode, Is.AnyOf(2627, 2601));
    }

    [Test]
    public void ExecuteReader_WithSqlDbCommandExecutor_ShouldWork()
    {
        // Arrange
        var consumer = new TestUserConsumer();

        // Act
        var result = _executor!.ExecuteReader(
            "GetActiveUsers",
            cmd => cmd.WithInputParameter("MinAge", DbType.Int32, 0),
            consumer
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Length, Is.GreaterThanOrEqualTo(3)); // We have 3 test users
    }

    [Test]
    public void SqlDbCommandExecutor_Integration_RoundTrip()
    {
        // This test verifies the full integration: SqlDbCommandExecutor -> SqlDatabase -> SqlErrorMapper

        // Arrange
        const string username = "roundtrip_user";
        const string email = "roundtrip@test.com";
        const int age = 35;

        // Act - Insert
        var insertResult = _executor!.ExecuteAndBind<int>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, username)
                .WithInputParameter("Email", DbType.String, 100, email)
                .WithInputParameter("Age", DbType.Int32, age)
                .WithOutputParameter("UserId", DbType.Int32, 0),
            cmd => cmd.GetParameterValue<int>("UserId")
        );

        // Assert Insert
        Assert.That(insertResult.IsSuccess, Is.True);
        var userId = insertResult.Value;
        Assert.That(userId, Is.GreaterThan(0));

        // Act - Read
        var readResult = _executor!.ExecuteScalar(
            db => db.GetSqlCommand($"SELECT Username FROM Users WHERE UserId = {userId}")
        );

        // Assert Read
        Assert.That(readResult.IsSuccess, Is.True);
        Assert.That(readResult.Value, Is.EqualTo(username));
    }

    private void CleanupTestData()
    {
        ExecuteNonQuery("DELETE FROM OrderItems");
        ExecuteNonQuery("DELETE FROM Orders");
        ExecuteNonQuery("DELETE FROM Products WHERE ProductId > 4");
        ExecuteNonQuery("DELETE FROM Users WHERE UserId > 3");
    }

    private void ExecuteNonQuery(string sql)
    {
        _executor!.ExecuteNonQuery(db => db.GetSqlCommand(sql))
            .TapError(error => throw new InvalidOperationException($"Failed: {error.Message}"));
    }

    private class TestUserConsumer : IResultsConsumer<TestUser[]>
    {
        public TestUser[] GetResults(System.Data.IDataReader reader)
        {
            var users = new List<TestUser>();
            while (reader.Read())
            {
                users.Add(new TestUser
                {
                    UserId = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    Age = reader.GetInt32(3)
                });
            }
            return users.ToArray();
        }
    }

    private class TestUser
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
