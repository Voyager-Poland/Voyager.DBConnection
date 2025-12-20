# Voyager.DBConnection Integration Tests

Integration tests for the Voyager.DBConnection library against multiple database providers.

## Prerequisites

- Docker Desktop installed and running
- .NET 8.0 SDK

## Supported Databases

- **SQL Server 2022**
- **PostgreSQL 16**
- **MySQL 8.0**
- **Oracle XE 21**
- **SQLite** (file-based)

## Quick Start

### 1. Start Database Containers

```bash
# From repository root
docker-compose up -d

# Wait for containers to be healthy
.\scripts\start-databases.ps1
```

### 2. Initialize Databases

```bash
# Initialize all databases
.\scripts\run-init-script.ps1 -Database all

# Or initialize specific database
.\scripts\run-init-script.ps1 -Database mssql
.\scripts\run-init-script.ps1 -Database postgres
.\scripts\run-init-script.ps1 -Database mysql
.\scripts\run-init-script.ps1 -Database oracle
```

### 3. Run Tests

```bash
# Run all integration tests
dotnet test --filter "Category=Integration"

# Run SQL Server tests only
dotnet test --filter "Category=Integration&Category=SqlServer"

# Run specific database tests
dotnet test --filter "Category=Integration&Category=PostgreSQL"
dotnet test --filter "Category=Integration&Category=MySQL"
dotnet test --filter "Category=Integration&Category=Oracle"
dotnet test --filter "Category=Integration&Category=SQLite"
```

## Project Structure

```
Voyager.DBConnection.IntegrationTests/
├── Infrastructure/
│   ├── DatabaseTestBase.cs       # Base class for all database tests
│   ├── TestConfiguration.cs      # Configuration helper
│   └── ErrorPolicies.cs          # Database-specific error mapping
├── SqlServer/
│   ├── SqlServerTestBase.cs      # SQL Server test base
│   └── DbCommandExecutorTests.cs # SQL Server integration tests
├── PostgreSQL/                    # (To be implemented)
├── MySQL/                         # (To be implemented)
├── Oracle/                        # (To be implemented)
└── SQLite/                        # (To be implemented)
```

## Test Categories

Tests are organized using NUnit categories:

- `[Category("Integration")]` - All integration tests
- `[Category("SqlServer")]` - SQL Server specific tests
- `[Category("PostgreSQL")]` - PostgreSQL specific tests
- `[Category("MySQL")]` - MySQL specific tests
- `[Category("Oracle")]` - Oracle specific tests
- `[Category("SQLite")]` - SQLite specific tests

## Configuration

Connection strings are configured in [appsettings.json](appsettings.json):

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost,1433;Database=testdb;...",
    "PostgreSQL": "Host=localhost;Port=5432;...",
    "MySQL": "Server=localhost;Port=3306;...",
    "Oracle": "Data Source=...",
    "SQLite": "Data Source=testdb.sqlite;"
  }
}
```

Connection strings can be overridden with environment variables.

## Writing Tests

### Create a Test Class

```csharp
using Voyager.DBConnection.IntegrationTests.Infrastructure;

namespace Voyager.DBConnection.IntegrationTests.SqlServer;

[TestFixture]
public class MyFeatureTests : SqlServerTestBase
{
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        CleanupTestData(); // Clean state before each test
    }

    [Test]
    public void MyTest()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = Executor!.ExecuteScalar(
            "GetUserCount",
            cmd => cmd.WithInputParameter("Active", DbType.Boolean, true)
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }
}
```

### Test Result Monad Patterns

```csharp
[Test]
public void TestEnsure_WithValidation()
{
    var result = Executor!.ExecuteAndBind(...)
        .Ensure(value => value > 0,
            Error.ValidationError("Code", "Message"))
        .Tap(value => Console.WriteLine($"Success: {value}"))
        .TapError(error => Console.WriteLine($"Error: {error.Message}"));

    Assert.That(result.IsSuccess, Is.True);
}
```

### Test Error Handling

```csharp
[Test]
public void TestDuplicateKey_ShouldReturnConflictError()
{
    // First insert
    Executor!.ExecuteNonQuery(...);

    // Duplicate insert
    var result = Executor!.ExecuteNonQuery(...);

    // Assert error type
    Assert.That(result.IsSuccess, Is.False);
    Assert.That(result.Error.Type, Is.EqualTo(ErrorType.Conflict));
    Assert.That(result.Error.Code, Is.EqualTo("Database.UniqueConstraint"));
}
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  integration-tests:
    runs-on: ubuntu-latest

    services:
      mssql:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourStrong@Passw0rd
        ports:
          - 1433:1433

      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_PASSWORD: postgres
        ports:
          - 5432:5432

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Run Integration Tests
        run: dotnet test --filter "Category=Integration" --logger "console;verbosity=detailed"
```

## Troubleshooting

### Containers Not Starting

```bash
# Check container status
docker-compose ps

# View logs
docker-compose logs mssql
docker-compose logs postgres

# Restart specific service
docker-compose restart mssql
```

### Connection Failures

1. Ensure containers are healthy:
   ```bash
   docker-compose ps
   ```

2. Test connection manually:
   ```bash
   # SQL Server
   docker exec -it voyager-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1"
   ```

3. Check if ports are available:
   ```bash
   netstat -an | findstr "1433"
   ```

### Test Failures

- Ensure databases are initialized:
  ```bash
  .\scripts\run-init-script.ps1 -Database all
  ```

- Clean test data between runs:
  ```csharp
  [SetUp]
  public override void SetUp()
  {
      base.SetUp();
      CleanupTestData();
  }
  ```

## Cleanup

```bash
# Stop containers
docker-compose down

# Remove volumes (clean slate)
docker-compose down -v

# Remove test SQLite database
Remove-Item testdb.sqlite -ErrorAction SilentlyContinue
```

## Next Steps

1. Implement tests for PostgreSQL, MySQL, Oracle, SQLite
2. Add performance benchmark tests
3. Add transaction isolation tests
4. Add connection pooling tests
5. Add concurrent access tests
