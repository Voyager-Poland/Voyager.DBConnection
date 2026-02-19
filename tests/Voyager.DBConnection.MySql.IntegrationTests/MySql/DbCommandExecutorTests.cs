using System.Data;
using Voyager.Common.Results;

namespace Voyager.DBConnection.MySql.IntegrationTests.MySql;

[TestFixture]
public class DbCommandExecutorTests : MySqlTestBase
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
        Assert.That(Convert.ToInt32(result.Value), Is.EqualTo(1));
    }

    [Test]
    public void ExecuteScalar_CountUsers_ShouldReturnCorrectCount()
    {
        // Arrange - Insert test users to ensure we have data
        _ = ExecuteNonQuery("INSERT INTO Users (Username, Email, Age) VALUES ('count_test1', 'count1@example.com', 25)");
        _ = ExecuteNonQuery("INSERT INTO Users (Username, Email, Age) VALUES ('count_test2', 'count2@example.com', 30)");

        // Act
        var result = ExecuteScalar("SELECT COUNT(*) FROM Users");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(Convert.ToInt64(result.Value), Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void ExecuteAndBind_CreateUser_ShouldReturnUserId()
    {
        // Arrange
        const string username = "test_user_mysql";
        const string email = "test@example.com";
        const int age = 28;

        // Act
        var result = Executor!.ExecuteAndBind<int>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, username)
                .WithInputParameter("p_Email", DbType.String, 100, email)
                .WithInputParameter("p_Age", DbType.Int32, age)
                .WithOutputParameter("p_UserId", DbType.Int32, 0),
            cmd => cmd.GetParameterValue<int>("p_UserId")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.GreaterThan(0));
    }

    [Test]
    public void ExecuteNonQuery_InsertUser_ShouldSucceed()
    {
        // Arrange
        const string insertSql = @"
            INSERT INTO Users (Username, Email, Age)
            VALUES ('direct_insert_mysql', 'direct@example.com', 30)";

        // Act
        var result = ExecuteNonQuery(insertSql);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1));
    }

    [Test]
    public void ExecuteAndBind_CreateOrder_ShouldReturnOrderId()
    {
        // Arrange - First create a user
        var userResult = Executor!.ExecuteAndBind<int>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, "order_test_user")
                .WithInputParameter("p_Email", DbType.String, 100, "order@example.com")
                .WithInputParameter("p_Age", DbType.Int32, 25)
                .WithOutputParameter("p_UserId", DbType.Int32, 0),
            cmd => cmd.GetParameterValue<int>("p_UserId")
        );

        Assert.That(userResult.IsSuccess, Is.True);
        var userId = userResult.Value;

        // Act - Create an order
        var result = Executor!.ExecuteAndBind<int>(
            "CreateOrder",
            cmd => cmd
                .WithInputParameter("p_UserId", DbType.Int32, userId)
                .WithInputParameter("p_TotalAmount", DbType.Decimal, 150.50m)
                .WithOutputParameter("p_OrderId", DbType.Int32, 0)
                .WithOutputParameter("p_OrderNumber", DbType.String, 50),
            cmd => cmd.GetParameterValue<int>("p_OrderId")
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.GreaterThan(0));
    }

    [Test]
    public void ExecuteNonQuery_CallGetUserById_ShouldSucceed()
    {
        // Arrange - Create a test user first
        var userResult = Executor!.ExecuteAndBind<int>(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("p_Username", DbType.String, 50, "getbyid_test")
                .WithInputParameter("p_Email", DbType.String, 100, "getbyid@example.com")
                .WithInputParameter("p_Age", DbType.Int32, 26)
                .WithOutputParameter("p_UserId", DbType.Int32, 0),
            cmd => cmd.GetParameterValue<int>("p_UserId")
        );

        Assert.That(userResult.IsSuccess, Is.True);
        var userId = userResult.Value;

        // Act - Call stored procedure (it returns result set, but we use ExecuteNonQuery)
        var result = Executor!.ExecuteNonQuery(
            "GetUserById",
            cmd => cmd.WithInputParameter("p_UserId", DbType.Int32, userId)
        );

        // Assert - ExecuteNonQuery returns 0 for SELECT statements
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ExecuteScalar_GetUsernameById_ShouldReturnUsername()
    {
        // Arrange - Create a test user first
        const string username = "get_username_test";
        _ = ExecuteNonQuery($"INSERT INTO Users (Username, Email, Age) VALUES ('{username}', 'getuser@example.com', 27)");

        // Get the user ID
        var userIdResult = ExecuteScalar($"SELECT UserId FROM Users WHERE Username = '{username}'");
        Assert.That(userIdResult.IsSuccess, Is.True);
        var userId = Convert.ToInt32(userIdResult.Value);

        // Act
        var result = ExecuteScalar($"SELECT Username FROM Users WHERE UserId = {userId}");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(username));
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
        _ = ExecuteNonQuery("INSERT INTO Users (Username, Email, Age) VALUES ('duplicate_test', 'test1@example.com', 25)");

        // Act - Try to insert duplicate username (unique constraint violation)
        var result = ExecuteNonQuery("INSERT INTO Users (Username, Email, Age) VALUES ('duplicate_test', 'test2@example.com', 30)");

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Conflict)); // MySQL error mapper should map unique violations to Conflict
    }
}
