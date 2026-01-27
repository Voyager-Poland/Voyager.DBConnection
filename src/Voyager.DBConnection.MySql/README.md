# Voyager.DBConnection.MySql

MySQL/MariaDB-specific database connection library built on top of `Voyager.DBConnection`.

## Features

- **MySqlDbCommandExecutor**: Pre-configured executor for MySQL/MariaDB databases
- **MySqlErrorMapper**: Maps MySQL error codes to typed `Error` results
- **Built-in MySql.Data support**

## Installation

```bash
dotnet add package Voyager.DBConnection.MySql
```

## Quick Start

```csharp
using Voyager.DBConnection.MySql;

var connectionString = "Server=localhost;Database=mydb;User=root;Password=pass;";
var executor = new MySqlDbCommandExecutor(connectionString);

// Execute queries with automatic error mapping
var result = executor.ExecuteScalar(
    db => db.GetSqlCommand("SELECT COUNT(*) FROM users")
);
```

## MySQL Error Mapping

The `MySqlErrorMapper` automatically maps MySQL-specific errors:

- **1062**: Duplicate entry (unique constraint) → `ErrorType.Database`
- **1205**: Lock wait timeout → `ErrorType.Timeout`
- **1213**: Deadlock found → `ErrorType.Unavailable`
- **Foreign key violations** → `ErrorType.Database`

## See Also

- [Voyager.DBConnection](https://www.nuget.org/packages/Voyager.DBConnection/) - Base library
- [MySQL Documentation](https://dev.mysql.com/doc/)
