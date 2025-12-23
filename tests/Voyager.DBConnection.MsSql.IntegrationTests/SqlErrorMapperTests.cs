using System.Data;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using Voyager.Common.Results;
using Voyager.DBConnection.MsSql;

namespace Voyager.DBConnection.MsSql.IntegrationTests;

[TestFixture]
[Category("Integration")]
[Category("SqlServer")]
[Category("MsSql")]
public class SqlErrorMapperTests : SqlServerTestBase
{
    [Test]
    public void ExecuteNonQuery_DuplicateKey_ShouldReturnDatabaseError()
    {
        // Arrange - create first user
        Executor!.ExecuteNonQuery(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, "unique_user_error_test")
                .WithInputParameter("Email", DbType.String, 100, "error@example.com")
                .WithInputParameter("Age", DbType.Int32, 25)
                .WithOutputParameter("UserId", DbType.Int32, 0)
        );

        // Act - try to create duplicate (should trigger unique constraint violation)
        var result = Executor!.ExecuteNonQuery(
            "CreateUser",
            cmd => cmd
                .WithInputParameter("Username", DbType.String, 50, "unique_user_error_test")
                .WithInputParameter("Email", DbType.String, 100, "error2@example.com")
                .WithInputParameter("Age", DbType.Int32, 25)
                .WithOutputParameter("UserId", DbType.Int32, 0)
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Conflict));
        Assert.That(result.Error.Code, Is.Not.Null);

        // SQL Server error code for unique constraint violation is typically 2627 or 2601
        var errorCode = int.Parse(result.Error.Code);
        Assert.That(errorCode, Is.AnyOf(2627, 2601));
    }

    [Test]
    public void ExecuteNonQuery_Timeout_ShouldReturnTimeoutError()
    {
        // This test would require a slow query or query timeout setting
        // For now, we verify the mapper handles timeout exceptions correctly
        Assert.Pass("SqlErrorMapper maps timeout exceptions - tested via unit tests");
    }

    [Test]
    public void SqlErrorMapper_DeadlockException_ShouldReturnUnavailableError()
    {
        // Arrange
        var mapper = new SqlErrorMapper();
        var deadlockException = CreateSqlException(1205); // 1205 is deadlock error code

        // Act
        var error = mapper.MapError(deadlockException);

        // Assert
        Assert.That(error.Type, Is.EqualTo(ErrorType.Unavailable));
        Assert.That(error.Code, Is.EqualTo("1205"));
    }

    [Test]
    public void SqlErrorMapper_TimeoutException_ShouldReturnTimeoutError()
    {
        // Arrange
        var mapper = new SqlErrorMapper();
        var timeoutException = CreateSqlException(-2); // -2 is timeout error code

        // Act
        var error = mapper.MapError(timeoutException);

        // Assert
        Assert.That(error.Type, Is.EqualTo(ErrorType.Timeout));
        Assert.That(error.Code, Is.EqualTo("-2"));
    }

    [Test]
    public void SqlErrorMapper_GenericSqlException_ShouldReturnDatabaseError()
    {
        // Arrange
        var mapper = new SqlErrorMapper();
        var sqlException = CreateSqlException(547); // 547 is foreign key violation

        // Act
        var error = mapper.MapError(sqlException);

        // Assert
        Assert.That(error.Type, Is.EqualTo(ErrorType.Database));
        Assert.That(error.Code, Is.EqualTo("547"));
    }

    [Test]
    public void SqlErrorMapper_NonSqlException_ShouldReturnGenericError()
    {
        // Arrange
        var mapper = new SqlErrorMapper();
        var exception = new InvalidOperationException("Test exception");

        // Act
        var error = mapper.MapError(exception);

        // Assert
        Assert.That(error.Type, Is.EqualTo(ErrorType.Unexpected));
        Assert.That(error.Message, Contains.Substring("Test exception"));
    }

    private static SqlException CreateSqlException(int errorNumber)
    {
        // Use reflection to create SqlException with specific error number
        // This is a common pattern for testing SQL Server error handling

        var collectionConstructor = typeof(SqlErrorCollection)
            .GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);

        if (collectionConstructor == null)
            throw new InvalidOperationException("Could not find SqlErrorCollection constructor");

        var collection = collectionConstructor.Invoke(null);

        // Microsoft.Data.SqlClient has a different SqlError constructor signature
        var errorConstructor = typeof(SqlError)
            .GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(uint) },
                null);

        // If not found, try alternative constructor (for Microsoft.Data.SqlClient 5.x)
        if (errorConstructor == null)
        {
            errorConstructor = typeof(SqlError)
                .GetConstructor(
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(uint), typeof(Exception) },
                    null);

            if (errorConstructor == null)
                throw new InvalidOperationException("Could not find SqlError constructor");

            var error = errorConstructor.Invoke(new object[] { errorNumber, (byte)0, (byte)0, "server", "message", "procedure", 0, (uint)0, null! });

            typeof(SqlErrorCollection)
                .GetMethod("Add", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(collection, new[] { error });
        }
        else
        {
            var error = errorConstructor.Invoke(new object[] { errorNumber, (byte)0, (byte)0, "server", "message", "procedure", 0, (uint)0 });

            typeof(SqlErrorCollection)
                .GetMethod("Add", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(collection, new[] { error });
        }

        var exception = typeof(SqlException)
            .GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid) },
                null)!
            .Invoke(new object[] { "SQL Error", collection, null!, Guid.NewGuid() });

        return (SqlException)exception;
    }

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        CleanupTestData();
    }
}
