# Voyager.DBConnection.Oracle

Oracle-specific database connection library built on top of `Voyager.DBConnection`.

## Features

- **OracleDbCommandExecutor**: Pre-configured executor for Oracle databases
- **OracleErrorMapper**: Maps Oracle error codes to typed `Error` results
- **Built-in Oracle.ManagedDataAccess.Core support**

## Installation

```bash
dotnet add package Voyager.DBConnection.Oracle
```

## Quick Start

```csharp
using Voyager.DBConnection.Oracle;

var connectionString = "Data Source=...;User Id=...;Password=...";
var executor = new OracleDbCommandExecutor(connectionString);

// Execute queries with automatic error mapping
var result = executor.ExecuteScalar(
    db => db.GetSqlCommand("SELECT COUNT(*) FROM users")
);
```

## Oracle Error Mapping

The `OracleErrorMapper` automatically maps Oracle-specific errors:

- **ORA-00001**: Unique constraint violation → `ErrorType.Database`
- **ORA-02049**: Timeout/Distributed transaction timeout → `ErrorType.Timeout`
- **ORA-00060**: Deadlock detected → `ErrorType.Unavailable`
- **Foreign key violations** → `ErrorType.Database`

## See Also

- [Voyager.DBConnection](https://www.nuget.org/packages/Voyager.DBConnection/) - Base library
- [Oracle Documentation](https://www.oracle.com/database/technologies/appdev/dotnet/odp.html)
