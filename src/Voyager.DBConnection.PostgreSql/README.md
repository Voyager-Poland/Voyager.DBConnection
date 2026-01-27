# Voyager.DBConnection.PostgreSql

PostgreSQL-specific database connection library built on top of `Voyager.DBConnection`.

## Features

- **PostgreSqlDbCommandExecutor**: Pre-configured executor for PostgreSQL databases
- **PostgreSqlErrorMapper**: Maps PostgreSQL error codes to typed `Error` results
- **Built-in Npgsql support**

## Installation

```bash
dotnet add package Voyager.DBConnection.PostgreSql
```

## Quick Start

```csharp
using Voyager.DBConnection.PostgreSql;

var connectionString = "Host=localhost;Database=mydb;Username=user;Password=pass";
var executor = new PostgreSqlDbCommandExecutor(connectionString);

// Execute queries with automatic error mapping
var result = executor.ExecuteScalar(
    db => db.GetSqlCommand("SELECT COUNT(*) FROM users")
);
```

## PostgreSQL Error Mapping

The `PostgreSqlErrorMapper` automatically maps PostgreSQL-specific errors:

- **23505**: Unique constraint violation → `ErrorType.Database`
- **57014**: Query cancelled/timeout → `ErrorType.Timeout`
- **40P01**: Deadlock detected → `ErrorType.Unavailable`
- **Foreign key violations** → `ErrorType.Database`

## See Also

- [Voyager.DBConnection](https://www.nuget.org/packages/Voyager.DBConnection/) - Base library
- [Npgsql Documentation](https://www.npgsql.org/)
