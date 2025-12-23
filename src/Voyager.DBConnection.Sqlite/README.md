# Voyager.DBConnection.Sqlite

SQLite-specific database connection library built on top of `Voyager.DBConnection`.

## Features

- **SqliteDbCommandExecutor**: Pre-configured executor for SQLite databases
- **SqliteErrorMapper**: Maps SQLite error codes to typed `Error` results
- **Built-in Microsoft.Data.Sqlite support**

## Installation

```bash
dotnet add package Voyager.DBConnection.Sqlite
```

## Quick Start

```csharp
using Voyager.DBConnection.Sqlite;

var connectionString = "Data Source=mydb.db";
var executor = new SqliteDbCommandExecutor(connectionString);

// Execute queries with automatic error mapping
var result = executor.ExecuteScalar(
    db => db.GetSqlCommand("SELECT COUNT(*) FROM users")
);
```

## SQLite Error Mapping

The `SqliteErrorMapper` automatically maps SQLite-specific errors:

- **SQLITE_CONSTRAINT**: Constraint violation (unique, foreign key) → `ErrorType.Database`
- **SQLITE_BUSY**: Database locked → `ErrorType.Unavailable`
- **SQLITE_LOCKED**: Database table locked → `ErrorType.Unavailable`
- **Generic errors** → Appropriate error types

## See Also

- [Voyager.DBConnection](https://www.nuget.org/packages/Voyager.DBConnection/) - Base library
- [SQLite Documentation](https://www.sqlite.org/docs.html)
