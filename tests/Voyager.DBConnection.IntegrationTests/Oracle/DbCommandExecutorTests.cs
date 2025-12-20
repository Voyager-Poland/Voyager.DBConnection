using System.Data;
using Voyager.Common.Results;
using Oracle.ManagedDataAccess.Client;

namespace Voyager.DBConnection.IntegrationTests.Oracle;

[TestFixture]
public class DbCommandExecutorTests : OracleTestBase
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
        const string username = "test_user_ora";
        const string email = "test@example.com";
        const int age = 28;

        // Act
        var result = Executor!.ExecuteNonQuery(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, username)
                .WithInputParameter("p_Email", DbType.String, 100, email)
                .WithInputParameter("p_Age", DbType.Int32, age)
                .WithOutputParameter("p_UserId", DbType.Int32, 0)
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        // Oracle returns -1 for successful procedure execution
    }

    [Test]
    public void ExecuteAndBind_WithOutputParameter_ShouldReturnUserId()
    {
        // Arrange
        const string username = "bind_test_ora";
        const string email = "bind@example.com";
        const int age = 30;

        // Act
        var result = Executor!.ExecuteAndBind<decimal>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, username)
                .WithInputParameter("p_Email", DbType.String, 100, email)
                .WithInputParameter("p_Age", DbType.Int32, age)
                .WithOutputParameter("p_UserId", DbType.Decimal, 0),
            cmd => cmd.GetParameterValue<decimal>("p_UserId")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.GreaterThan(0));
    }

    [Test]
    public void ExecuteScalar_GetUserCount_ShouldReturnCount()
    {
        // Arrange - Create some test users
        Executor!.ExecuteNonQuery("CreateUser", cmd => cmd
            .WithInputParameter("p_Username", DbType.String, 50, "user1_ora")
            .WithInputParameter("p_Email", DbType.String, 100, "user1@example.com")
            .WithInputParameter("p_Age", DbType.Int32, 20)
            .WithOutputParameter("p_UserId", DbType.Decimal, 0));

        Executor!.ExecuteNonQuery("CreateUser", cmd => cmd
            .WithInputParameter("p_Username", DbType.String, 50, "user2_ora")
            .WithInputParameter("p_Email", DbType.String, 100, "user2@example.com")
            .WithInputParameter("p_Age", DbType.Int32, 30)
            .WithOutputParameter("p_UserId", DbType.Decimal, 0));

        // Act - Oracle GetUserCount has OUT parameter
        var result = Executor!.ExecuteAndBind<decimal>(
            "GetUserCount",
            cmd => cmd
                .WithInputParameter("p_Active", DbType.Decimal, DBNull.Value)
                .WithOutputParameter("p_Count", DbType.Decimal, 0),
            cmd => cmd.GetParameterValue<decimal>("p_Count")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.GreaterThanOrEqualTo(5)); // 3 initial + 2 created
    }

    [Test]
    public void ExecuteNonQuery_DuplicateUsername_ShouldReturnConflictError()
    {
        // Arrange - First insert
        Executor!.ExecuteNonQuery("CreateUser", cmd => cmd
            .WithInputParameter("p_Username", DbType.String, 50, "duplicate_user_ora")
            .WithInputParameter("p_Email", DbType.String, 100, "dup1@example.com")
            .WithInputParameter("p_Age", DbType.Int32, 25)
            .WithOutputParameter("p_UserId", DbType.Decimal, 0));

        // Act - Try to insert duplicate
        var result = Executor!.ExecuteNonQuery("CreateUser", cmd => cmd
            .WithInputParameter("p_Username", DbType.String, 50, "duplicate_user_ora")
            .WithInputParameter("p_Email", DbType.String, 100, "dup2@example.com")
            .WithInputParameter("p_Age", DbType.Int32, 30)
            .WithOutputParameter("p_UserId", DbType.Decimal, 0));

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Conflict));
    }

    [Test]
    public void ExecuteAndBind_WithTapAndTapError_ShouldExecuteCorrectCallback()
    {
        // Arrange
        bool tapCalled = false;
        bool tapErrorCalled = false;

        // Act
        Executor!.ExecuteAndBind<decimal>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, "tap_test_ora")
                .WithInputParameter("p_Email", DbType.String, 100, "tap@example.com")
                .WithInputParameter("p_Age", DbType.Int32, 27)
                .WithOutputParameter("p_UserId", DbType.Decimal, 0),
            cmd => cmd.GetParameterValue<decimal>("p_UserId")
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
    public void ExecuteAndBind_WithMap_ShouldTransformResult()
    {
        // Act
        var result = Executor!.ExecuteAndBind<decimal>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, "map_test_ora")
                .WithInputParameter("p_Email", DbType.String, 100, "map@example.com")
                .WithInputParameter("p_Age", DbType.Int32, 25)
                .WithOutputParameter("p_UserId", DbType.Decimal, 0),
            cmd => cmd.GetParameterValue<decimal>("p_UserId")
        )
        .Map(userId => $"User-{userId}");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Does.StartWith("User-"));
    }

    [Test]
    public async Task ExecuteNonQueryAsync_WithStoredProcedure_ShouldInsertUser()
    {
        // Arrange
        const string username = "async_user_ora";
        const string email = "async@example.com";
        const int age = 32;

        // Act
        var result = await Executor!.ExecuteNonQueryAsync(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, username)
                .WithInputParameter("p_Email", DbType.String, 100, email)
                .WithInputParameter("p_Age", DbType.Int32, age)
                .WithOutputParameter("p_UserId", DbType.Decimal, 0),
            null,
            CancellationToken.None
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ExecuteNonQuery_DeleteUser_ShouldSucceed()
    {
        // Arrange - Create a user first
        var createResult = Executor!.ExecuteAndBind<decimal>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, "delete_test_ora")
                .WithInputParameter("p_Email", DbType.String, 100, "delete@example.com")
                .WithInputParameter("p_Age", DbType.Int32, 30)
                .WithOutputParameter("p_UserId", DbType.Decimal, 0),
            cmd => cmd.GetParameterValue<decimal>("p_UserId")
        );
        var userId = Convert.ToInt32(createResult.Value);

        // Act
        var result = Executor!.ExecuteNonQuery(
            db => db.GetSqlCommand($"DELETE FROM Users WHERE UserId = {userId}")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1)); // 1 row affected
    }

    [Test]
    public void ExecuteScalar_GetUserById_ShouldReturnUsername()
    {
        // Arrange - First create a user
        var createResult = Executor!.ExecuteAndBind<decimal>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, "get_user_test_ora")
                .WithInputParameter("p_Email", DbType.String, 100, "getuser@example.com")
                .WithInputParameter("p_Age", DbType.Int32, 25)
                .WithOutputParameter("p_UserId", DbType.Decimal, 0),
            cmd => cmd.GetParameterValue<decimal>("p_UserId")
        );
        var userId = Convert.ToInt32(createResult.Value);

        // Act - Oracle GetUserById returns a cursor, so we'll use a simpler query
        var result = Executor!.ExecuteScalar(
            db => db.GetSqlCommand($"SELECT Username FROM Users WHERE UserId = {userId}")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("get_user_test_ora"));
    }
}
