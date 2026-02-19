using System.Data;
using Voyager.Common.Results;

namespace Voyager.DBConnection.MsSql.IntegrationTests;

[TestFixture]
public class DbCommandExecutorTests : SqlServerTestBase
{
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        CleanupTestData();
    }

    [Test]
    public void ExecuteNonQuery_WithStoredProcedure_ShouldInsertUser()
    {
        // Arrange
        const string username = "test_user";
        const string email = "test@example.com";
        const int age = 28;

        // Act
        var result = Executor!.ExecuteNonQuery(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, username)
                .WithInputParameter("Email", DbType.String, 100, email)
                .WithInputParameter("Age", DbType.Int32, age)
                .WithOutputParameter("UserId", DbType.Int32, 0),
            cmd =>
            {
                var userId = cmd.GetParameterValue<int>("UserId");
                Assert.That(userId, Is.GreaterThan(0));
            }
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(-1)); // SQL Server returns -1 for stored procs
    }

    [Test]
    public void ExecuteAndBind_WithOutputParameter_ShouldReturnUserId()
    {
        // Arrange
        const string username = "bind_test_user";
        const string email = "bind@example.com";
        const int age = 30;

        // Act
        var result = Executor!.ExecuteAndBind<int>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, username)
                .WithInputParameter("Email", DbType.String, 100, email)
                .WithInputParameter("Age", DbType.Int32, age)
                .WithOutputParameter("UserId", DbType.Int32, 0),
            cmd => cmd.GetParameterValue<int>("UserId") // Implicit conversion to Result<int>
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.GreaterThan(0));
    }

    [Test]
    public void ExecuteScalar_GetUserCount_ShouldReturnCount()
    {
        // Act
        var result = Executor!.ExecuteScalar(
            "GetUserCount",
            cmd => cmd.WithInputParameter("Active", DbType.Boolean, true)
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);

        var count = Convert.ToInt32(result.Value);
        Assert.That(count, Is.GreaterThanOrEqualTo(3)); // At least 3 test users
    }

    [Test]
    public void ExecuteNonQuery_DuplicateUsername_ShouldReturnConflictError()
    {
        // Arrange - first insert
        _ = Executor!.ExecuteNonQuery(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, "duplicate_user")
                .WithInputParameter("Email", DbType.String, 100, "dup1@example.com")
                .WithInputParameter("Age", DbType.Int32, 25)
                .WithOutputParameter("UserId", DbType.Int32, 0)
        );

        // Act - try to insert duplicate
        var result = Executor!.ExecuteNonQuery(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, "duplicate_user")
                .WithInputParameter("Email", DbType.String, 100, "dup2@example.com")
                .WithInputParameter("Age", DbType.Int32, 30)
                .WithOutputParameter("UserId", DbType.Int32, 0)
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Conflict));
        Assert.That(result.Error.Code, Is.EqualTo("2627"));
    }

    [Test]
    public void ExecuteAndBind_WithTapAndTapError_ShouldExecuteCorrectCallback()
    {
        // Arrange
        bool tapCalled = false;
        bool tapErrorCalled = false;

        // Act
        _ = Executor!.ExecuteAndBind<int>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, "tap_test_user")
                .WithInputParameter("Email", DbType.String, 100, "tap@example.com")
                .WithInputParameter("Age", DbType.Int32, 27)
                .WithOutputParameter("UserId", DbType.Int32, 0),
            cmd => cmd.GetParameterValue<int>("UserId")
        )
        .Tap(userId =>
        {
            tapCalled = true;
            Assert.That(userId, Is.GreaterThan(0));
        })
        .TapError(error =>
        {
            tapErrorCalled = true;
        });

        // Assert
        Assert.That(tapCalled, Is.True);
        Assert.That(tapErrorCalled, Is.False);
    }

    [Test]
    public void ExecuteAndBind_WithEnsure_ShouldValidateResult()
    {
        // Act
        var result = Executor!.ExecuteAndBind<int>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, "ensure_test")
                .WithInputParameter("Email", DbType.String, 100, "ensure@example.com")
                .WithInputParameter("Age", DbType.Int32, 25)
                .WithOutputParameter("UserId", DbType.Int32, 0),
            cmd => cmd.GetParameterValue<int>("UserId")
        )
        .Ensure(userId => userId > 0,
            Error.ValidationError("User.InvalidId", "User ID must be greater than 0"));

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ExecuteAndBind_WithMap_ShouldTransformResult()
    {
        // Act
        var result = Executor!.ExecuteAndBind<int>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, "map_test")
                .WithInputParameter("Email", DbType.String, 100, "map@example.com")
                .WithInputParameter("Age", DbType.Int32, 25)
                .WithOutputParameter("UserId", DbType.Int32, 0),
            cmd => cmd.GetParameterValue<int>("UserId")
        )
        .Map(userId => $"User-{userId}"); // Transform int to string

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Does.StartWith("User-"));
    }

    [Test]
    public async Task ExecuteNonQueryAsync_WithStoredProcedure_ShouldInsertUser()
    {
        // Arrange
        const string username = "async_test_user";
        const string email = "async@example.com";
        const int age = 28;

        // Act
        var result = await Executor!.ExecuteNonQueryAsync(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, username)
                .WithInputParameter("Email", DbType.String, 100, email)
                .WithInputParameter("Age", DbType.Int32, age)
                .WithOutputParameter("UserId", DbType.Int32, 0),
            cmd =>
            {
                var userId = cmd.GetParameterValue<int>("UserId");
                Assert.That(userId, Is.GreaterThan(0));
            },
            CancellationToken.None
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ExecuteNonQuery_WithForeignKeyViolation_ShouldReturnBusinessError()
    {
        // Arrange - try to create order for non-existent user
        var invalidUserId = 99999;

        // Act
        var result = Executor!.ExecuteNonQuery(
            "CreateOrder",
            cmd => cmd
                .WithInputParameter("UserId", DbType.Int32, invalidUserId)
                .WithInputParameter("TotalAmount", DbType.Decimal, 100.50m)
                .WithOutputParameter("OrderId", DbType.Int32, 0)
                .WithOutputParameter("OrderNumber", DbType.String, 50)
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Database));
        Assert.That(result.Error.Code, Is.EqualTo("547"));
    }
}
