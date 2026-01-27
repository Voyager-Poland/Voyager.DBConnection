# Voyager.DBConnection.MsSql

SQL Server implementation package for [Voyager.DBConnection](https://github.com/Voyager-Poland/Voyager.DBConnection) library.

## Installation

```bash
dotnet add package Voyager.DBConnection.MsSql
```

## Overview

This package provides SQL Server-specific implementations that simplify working with Microsoft SQL Server databases using the Voyager.DBConnection library. It includes pre-configured classes, SQL Server error mapping, and convenient constructors.

## Quick Start

### Modern Approach - Using DbCommandExecutor

```csharp
using Voyager.DBConnection.MsSql;

// Simple constructor - automatically configures SQL Server provider and error mapping
var executor = new SqlDbCommandExecutor("Server=localhost;Database=MyDb;Integrated Security=true;");

// Execute with Result monad pattern
var result = executor.ExecuteNonQuery(
    "CreateUser",
    cmd => cmd
        .WithInputParameter("Username", DbType.String, 50, "john")
        .WithInputParameter("Email", DbType.String, 100, "john@example.com")
        .WithOutputParameter("UserId", DbType.Int32, 0)
)
.Tap(rowsAffected => Console.WriteLine($"User created, {rowsAffected} row(s) affected"))
.Tap(rowsAffected =>
{
    var userId = cmd.GetParameterValue<int>("UserId");
    Console.WriteLine($"New user ID: {userId}");
})
.TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

### Reading Data

```csharp
// Execute reader with custom consumer
public class UserConsumer : IResultsConsumer<User[]>
{
    public User[] GetResults(IDataReader reader)
    {
        var users = new List<User>();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2)
            });
        }
        return users.ToArray();
    }
}

var result = executor.ExecuteReader(
    "GetActiveUsers",
    cmd => cmd.WithInputParameter("MinAge", DbType.Int32, 18),
    new UserConsumer()
)
.Tap(users => users.ForEach(u => Console.WriteLine($"{u.Username}: {u.Email}")))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

## What's Included

### SqlDbCommandExecutor

Pre-configured executor with SQL Server database and error mapping.

```csharp
public class SqlDbCommandExecutor : DbCommandExecutor
{
    public SqlDbCommandExecutor(string sqlConnectionString)
        : base(new SqlDatabase(sqlConnectionString), new SqlErrorMapper())
    {
    }
}
```

**Benefits:**
- No need to manually create `SqlDatabase` or `SqlErrorMapper`
- One-line setup with connection string
- Automatic SQL Server error code mapping

### SqlDatabase

SQL Server-specific `Database` implementation.

```csharp
var database = new SqlDatabase("Server=localhost;Database=MyDb;...");
var executor = new DbCommandExecutor(database, new SqlErrorMapper());
```

**Features:**
- Automatically uses `SqlClientFactory`
- Simplified constructor - just pass connection string
- Inherits all `Database` functionality

### SqlErrorMapper

Maps SQL Server error codes to typed `Error` results.

```csharp
public class SqlErrorMapper : IMapErrorPolicy
{
    public Error MapError(Exception ex)
    {
        if (ex is SqlException sqlEx)
        {
            if (sqlEx.Number == -2)  // Timeout
                return Error.TimeoutError(sqlEx.Number.ToString(), sqlEx.Message);

            if (sqlEx.Number == 1205)  // Deadlock
                return Error.UnavailableError(sqlEx.Number.ToString(), sqlEx.Message);

            return Error.DatabaseError(sqlEx.Number.ToString(), sqlEx.Message);
        }
        return Error.FromException(ex);
    }
}
```

**Mapped Error Codes:**
- **Timeout** (`-2`) → `Error.TimeoutError`
- **Deadlock** (`1205`) → `Error.UnavailableError`
- **Other SQL errors** → `Error.DatabaseError`
- **Non-SQL exceptions** → `Error.FromException`

### SqlConnection (Legacy)

Legacy `Connection` class for backwards compatibility.

```csharp
var connection = new SqlConnection("Server=localhost;Database=MyDb;...");
connection.ExecuteNonQuery(factory);  // Throws exception on error
```

**⚠️ Deprecated:** Use `SqlDbCommandExecutor` for new development.

## Why Use This Package?

### ✅ Simplified Setup
```csharp
// Without MsSql package
var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
var database = new Database("connection string", factory);
var executor = new DbCommandExecutor(database, new SqlErrorMapper());

// With MsSql package
var executor = new SqlDbCommandExecutor("connection string");
```

### ✅ SQL Server Error Handling
Automatic mapping of SQL Server-specific error codes:
- Deadlocks → Retry logic with `UnavailableError`
- Timeouts → Appropriate timeout handling
- Constraint violations → Business logic errors

### ✅ Type Safety
Strongly-typed error results instead of catching `SqlException`:
```csharp
result
    .TapError(error =>
    {
        if (error.ErrorType == ErrorType.Timeout)
            Console.WriteLine("Query timeout - consider optimization");
        else if (error.ErrorType == ErrorType.Unavailable)
            Console.WriteLine("Deadlock - retry operation");
    });
```

## Dependencies

- **Voyager.DBConnection** (≥ 4.4.0) - Core library
- **System.Data.SqlClient** (≥ 4.8.6) - SQL Server data provider

## Target Frameworks

- .NET 8.0
- .NET 6.0
- .NET Framework 4.8

## Documentation

For complete documentation and advanced usage:
- **Main Documentation**: [Voyager.DBConnection](https://github.com/Voyager-Poland/Voyager.DBConnection)
- **Result Monad Pattern**: See main README for `Result<T>` usage
- **Command Factory Pattern**: See main README for `IDbCommandFactory` examples
- **Testability**: See main README for mocking with `IDbCommandExecutor`

## License

MIT License - see [LICENSE](https://github.com/Voyager-Poland/Voyager.DBConnection/blob/main/LICENSE)

## Credits

- [@andrzejswistowski](https://github.com/AndrzejSwistowski)
