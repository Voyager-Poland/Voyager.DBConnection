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
    public void ExecuteNonQuery_CreateTable_ShouldSucceed()
    {
        // Arrange
        const string createTableSql = @"
            CREATE TEMPORARY TABLE test_table (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100),
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )";

        // Act
        var result = ExecuteNonQuery(createTableSql);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void ExecuteNonQuery_InsertData_ShouldReturnRowsAffected()
    {
        // Arrange
        ExecuteNonQuery("CREATE TEMPORARY TABLE test_insert (id SERIAL PRIMARY KEY, value TEXT)");
        const string insertSql = "INSERT INTO test_insert (value) VALUES ('test_value')";

        // Act
        var result = ExecuteNonQuery(insertSql);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1));
    }

    [Test]
    public void ExecuteScalar_Count_ShouldReturnCorrectValue()
    {
        // Arrange
        ExecuteNonQuery("CREATE TEMPORARY TABLE test_count (id SERIAL PRIMARY KEY, value TEXT)");
        ExecuteNonQuery("INSERT INTO test_count (value) VALUES ('test1')");
        ExecuteNonQuery("INSERT INTO test_count (value) VALUES ('test2')");
        ExecuteNonQuery("INSERT INTO test_count (value) VALUES ('test3')");

        // Act
        var result = ExecuteScalar("SELECT COUNT(*) FROM test_count");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(Convert.ToInt64(result.Value), Is.EqualTo(3));
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
}
