using System.Data;
using Voyager.Common.Results;

namespace Voyager.DBConnection.IntegrationTests.PostgreSQL;

[TestFixture]
public class DbCommandExecutorTests : PostgreSqlTestBase
{
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        CleanupTestData();
    }

    [Test]
    public void ExecuteScalar_CreateUser_ShouldReturnUserId()
    {
        // Arrange
        const string username = "test_user_pg";
        const string email = "test@example.com";
        const int age = 28;

        // Act - PostgreSQL functions return values directly via SELECT
        var result = Executor!.ExecuteScalar(
            db => db.GetSqlCommand($"SELECT CreateUser('{username}', '{email}', {age})")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        var userId = Convert.ToInt32(result.Value);
        Assert.That(userId, Is.GreaterThan(0));
    }

    [Test]
    public void ExecuteScalar_GetUserCount_ShouldReturnCount()
    {
        // Arrange - Create some test users
        _ = Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT CreateUser('user1_pg', 'user1@example.com', 20)"));
        _ = Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT CreateUser('user2_pg', 'user2@example.com', 30)"));
        _ = Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT CreateUser('user3_pg', 'user3@example.com', 40)"));

        // Act
        var result = Executor!.ExecuteScalar(
            db => db.GetSqlCommand("SELECT GetUserCount(NULL)")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var count = Convert.ToInt64(result.Value!);
        Assert.That(count, Is.GreaterThanOrEqualTo(6)); // 3 initial + 3 created
    }

    [Test]
    public void ExecuteScalar_DuplicateUsername_ShouldReturnConflictError()
    {
        // Arrange - First insert
        _ = Executor!.ExecuteScalar(db => db.GetSqlCommand("SELECT CreateUser('duplicate_user_pg', 'dup1@example.com', 25)"));

        // Act - Try to insert duplicate
        var result = Executor!.ExecuteScalar(
            db => db.GetSqlCommand("SELECT CreateUser('duplicate_user_pg', 'dup2@example.com', 30)")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Conflict));
        Assert.That(result.Error.Code, Is.EqualTo("Database.UniqueConstraint"));
    }

    [Test]
    public void ExecuteScalar_WithTapAndTapError_ShouldExecuteCorrectCallback()
    {
        // Arrange
        bool tapCalled = false;
        bool tapErrorCalled = false;

        // Act
        _ = Executor!.ExecuteScalar(
            db => db.GetSqlCommand("SELECT CreateUser('tap_test_pg', 'tap@example.com', 27)")
        )
        .Tap(userId =>
        {
            tapCalled = true;
            Assert.That(userId, Is.Not.Null);
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
    public void ExecuteScalar_WithMap_ShouldTransformResult()
    {
        // Act
        var result = Executor!.ExecuteScalar(
            db => db.GetSqlCommand("SELECT CreateUser('map_test_pg', 'map@example.com', 25)")
        )
        .Map(userId => $"User-{userId}");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Does.StartWith("User-"));
    }

    [Test]
    public async Task ExecuteScalarAsync_CreateUser_ShouldReturnUserId()
    {
        // Arrange
        const string username = "async_user_pg";
        const string email = "async@example.com";
        const int age = 32;

        // Act
        var result = await Executor!.ExecuteScalarAsync(
            db => db.GetSqlCommand($"SELECT CreateUser('{username}', '{email}', {age})"),
            null,
            CancellationToken.None
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        var userId = Convert.ToInt32(result.Value);
        Assert.That(userId, Is.GreaterThan(0));
    }

    [Test]
    public void ExecuteNonQuery_DeleteUser_ShouldSucceed()
    {
        // Arrange - Create a user first
        var createResult = Executor!.ExecuteScalar(
            db => db.GetSqlCommand("SELECT CreateUser('delete_test_pg', 'delete@example.com', 30)")
        );
        Assert.That(createResult.IsSuccess, Is.True);
        var userId = Convert.ToInt32(createResult.Value!);

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
        const string username = "get_user_test_pg";
        var createResult = Executor!.ExecuteScalar(
            db => db.GetSqlCommand($"SELECT CreateUser('{username}', 'getuser@example.com', 25)")
        );
        Assert.That(createResult.IsSuccess, Is.True);
        var userId = Convert.ToInt32(createResult.Value!);

        // Act - Use direct SQL for simple SELECT
        var result = Executor!.ExecuteScalar(
            db => db.GetSqlCommand($"SELECT Username FROM Users WHERE UserId = {userId}")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(username));
    }
}
