using System.Data;
using Voyager.Common.Results;

namespace Voyager.DBConnection.PostgreSql.IntegrationTests.PostgreSql;

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
    public void ExecuteScalar_SimpleQuery_ShouldReturnValue()
    {
        // Arrange & Act
        var result = ExecuteScalar("SELECT 1");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1));
    }

    [Test]
    public void ExecuteScalar_GetUserCount_ShouldReturnCorrectValue()
    {
        // Act - Get count of all users (should be 3 from test data)
        var result = ExecuteScalar("SELECT GetUserCount(NULL)");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(Convert.ToInt32(result.Value), Is.EqualTo(3));
    }

    [Test]
    public void ExecuteScalar_CreateUser_ShouldReturnUserId()
    {
        // Arrange
        const string username = "test_user_pg";
        const string email = "test@example.com";
        const int age = 28;

        // Act - PostgreSQL functions are called with SELECT
        var result = Executor!.ExecuteScalar(
            db => db.GetSqlCommand($"SELECT * FROM CreateUser('{username}', '{email}', {age})")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(Convert.ToInt32(result.Value), Is.GreaterThan(0));
    }

    [Test]
    public void ExecuteNonQuery_InsertUser_ShouldSucceed()
    {
        // Arrange
        const string insertSql = @"
            INSERT INTO Users (Username, Email, Age)
            VALUES ('direct_insert_pg', 'direct@example.com', 30)";

        // Act
        var result = ExecuteNonQuery(insertSql);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1));
    }

    [Test]
    public void ExecuteScalar_CreateOrder_ShouldReturnOrderId()
    {
        // Arrange - First create a user
        var userIdResult = Executor!.ExecuteScalar(
            db => db.GetSqlCommand("SELECT * FROM CreateUser('order_test_user', 'order@example.com', 25)")
        );

        Assert.That(userIdResult.IsSuccess, Is.True);
        var userId = Convert.ToInt32(userIdResult.Value);

        // Act - Create an order (function returns row with orderid and ordernumber)
        var result = Executor!.ExecuteScalar(
            db => db.GetSqlCommand($"SELECT p_OrderId FROM CreateOrder({userId}, 150.50)")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(Convert.ToInt32(result.Value), Is.GreaterThan(0));
    }

    [Test]
    public void ExecuteScalar_GetUserById_ShouldReturnUsername()
    {
        // Arrange - Use existing test user (UserId = 1)
        const int userId = 1;

        // Act - Get username from GetUserById function
        var result = Executor!.ExecuteScalar(
            db => db.GetSqlCommand($"SELECT Username FROM GetUserById({userId})")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("john_doe"));
    }

    [Test]
    public void ExecuteScalar_CountUsers_ShouldReturnCorrectCount()
    {
        // Act
        var result = ExecuteScalar("SELECT COUNT(*) FROM Users");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(Convert.ToInt64(result.Value), Is.EqualTo(3)); // Initial test data
    }

    [Test]
    public void ExecuteNonQuery_WithInvalidSql_ShouldReturnFailure()
    {
        // Act
        var result = ExecuteNonQuery("SELECT * FROM non_existent_table");

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Database));
    }

    [Test]
    public void ExecuteNonQuery_DuplicateUsername_ShouldReturnConflictError()
    {
        // Arrange - First insert
        ExecuteNonQuery("INSERT INTO Users (Username, Email, Age) VALUES ('duplicate_test', 'test1@example.com', 25)");

        // Act - Try to insert duplicate username (unique constraint violation)
        var result = ExecuteNonQuery("INSERT INTO Users (Username, Email, Age) VALUES ('duplicate_test', 'test2@example.com', 30)");

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Conflict)); // PostgreSQL error mapper correctly maps unique violations to Conflict
    }
}
