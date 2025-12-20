# Docker Integration Testing

This directory contains Docker Compose configuration for testing `Voyager.DBConnection` library against multiple database providers.

## Supported Databases

- **SQL Server 2022** (port 1433)
- **PostgreSQL 16** (port 5432)
- **MySQL 8.0** (port 3306)
- **Oracle XE 21** (port 1521)
- **SQLite** (file-based, no container)

## Quick Start

### 1. Start All Databases

```bash
docker-compose up -d
```

### 2. Check Database Health

```bash
docker-compose ps
```

All services should show status as `healthy` after startup.

### 3. Stop All Databases

```bash
docker-compose down
```

### 4. Stop and Remove Volumes (clean slate)

```bash
docker-compose down -v
```

## Individual Database Management

### Start Specific Database

```bash
# SQL Server only
docker-compose up -d mssql

# PostgreSQL only
docker-compose up -d postgres

# MySQL only
docker-compose up -d mysql

# Oracle only
docker-compose up -d oracle
```

### View Logs

```bash
# All databases
docker-compose logs -f

# Specific database
docker-compose logs -f mssql
docker-compose logs -f postgres
docker-compose logs -f mysql
docker-compose logs -f oracle
```

### Connect to Databases

#### SQL Server
```bash
docker exec -it voyager-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd"
```

#### PostgreSQL
```bash
docker exec -it voyager-postgres psql -U postgres -d testdb
```

#### MySQL
```bash
docker exec -it voyager-mysql mysql -u testuser -ptestpass testdb
```

#### Oracle
```bash
docker exec -it voyager-oracle sqlplus testuser/testpass@XEPDB1
```

## Connection Strings

Connection strings are configured in `.env` file (copy from `.env.example`):

```bash
cp .env.example .env
```

### SQL Server
```
Server=localhost,1433;Database=testdb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

### PostgreSQL
```
Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres;
```

### MySQL
```
Server=localhost;Port=3306;Database=testdb;Uid=testuser;Pwd=testpass;
```

### Oracle
```
Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));User Id=testuser;Password=testpass;
```

### SQLite
```
Data Source=./testdb.sqlite;
```

## Running Integration Tests

### All Databases
```bash
dotnet test --filter "Category=Integration"
```

### Specific Database
```bash
dotnet test --filter "Category=Integration&Database=SqlServer"
dotnet test --filter "Category=Integration&Database=PostgreSQL"
dotnet test --filter "Category=Integration&Database=MySQL"
dotnet test --filter "Category=Integration&Database=Oracle"
dotnet test --filter "Category=Integration&Database=SQLite"
```

## Database Initialization Scripts

### SQL Server - Create Test Database

```bash
docker exec -it voyager-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "CREATE DATABASE testdb"
```

### Create Test Tables

Example SQL script to create test tables in each database:

```sql
-- Users table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),  -- SQL Server
    -- UserId SERIAL PRIMARY KEY,            -- PostgreSQL
    -- UserId INT PRIMARY KEY AUTO_INCREMENT, -- MySQL
    -- UserId NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- Oracle

    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL,
    Age INT NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- Orders table
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    TotalAmount DECIMAL(18,2) NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
```

## Troubleshooting

### Port Conflicts

If you have local database instances running, they may conflict with Docker ports. Either:
- Stop local instances
- Change port mappings in `docker-compose.yml`

### Health Check Failures

If health checks fail:
```bash
# Check logs
docker-compose logs mssql
docker-compose logs postgres
docker-compose logs mysql
docker-compose logs oracle

# Restart specific service
docker-compose restart mssql
```

### Oracle Startup Time

Oracle XE takes longer to start (30-60 seconds). Wait for health check to pass:
```bash
docker-compose ps oracle
```

### Memory Requirements

Ensure Docker has sufficient memory allocated:
- SQL Server: minimum 2GB
- Oracle XE: minimum 2GB
- PostgreSQL: 512MB
- MySQL: 512MB

**Total recommended: 6GB+ for all databases**

## Cleanup

### Remove All Containers and Volumes
```bash
docker-compose down -v
```

### Remove All Images
```bash
docker rmi mcr.microsoft.com/mssql/server:2022-latest
docker rmi postgres:16-alpine
docker rmi mysql:8.0
docker rmi gvenzl/oracle-xe:21-slim
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

      mysql:
        image: mysql:8.0
        env:
          MYSQL_ROOT_PASSWORD: root
          MYSQL_DATABASE: testdb
        ports:
          - 3306:3306

    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Run Integration Tests
        run: dotnet test --filter "Category=Integration"
```

## Database-Specific Notes

### SQL Server
- Default SA password must be strong (uppercase, lowercase, numbers, symbols)
- TrustServerCertificate=True required for self-signed certificates

### PostgreSQL
- Case-sensitive by default
- Use double quotes for mixed-case identifiers

### MySQL
- Uses `mysql_native_password` authentication plugin
- Default charset is utf8mb4

### Oracle
- XE version has limitations (1 database, 12GB user data, 2GB RAM)
- Connection service name is `XEPDB1` for pluggable database
- Startup time is longer than other databases

### SQLite
- File-based, no container needed
- Supports most SQL features except stored procedures
- Good for unit tests, limited for integration tests
