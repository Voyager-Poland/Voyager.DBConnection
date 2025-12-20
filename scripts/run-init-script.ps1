# Initialize databases with test schema and data

param(
    [string]$Database = "all"
)

function Initialize-SqlServer {
    Write-Host "Initializing SQL Server..." -ForegroundColor Green
    $scriptPath = Join-Path $PSScriptRoot "init-databases.sql"
    Get-Content $scriptPath | docker exec -i voyager-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "SQL Server initialized successfully!" -ForegroundColor Green
    } else {
        Write-Host "SQL Server initialization failed!" -ForegroundColor Red
    }
}

function Initialize-PostgreSQL {
    Write-Host "Initializing PostgreSQL..." -ForegroundColor Green
    $scriptPath = Join-Path $PSScriptRoot "init-postgres.sql"
    Get-Content $scriptPath | docker exec -i voyager-postgres psql -U postgres -d testdb
    if ($LASTEXITCODE -eq 0) {
        Write-Host "PostgreSQL initialized successfully!" -ForegroundColor Green
    } else {
        Write-Host "PostgreSQL initialization failed!" -ForegroundColor Red
    }
}

function Initialize-MySQL {
    Write-Host "Initializing MySQL..." -ForegroundColor Green
    $scriptPath = Join-Path $PSScriptRoot "init-mysql.sql"
    Get-Content $scriptPath | docker exec -i voyager-mysql mysql -u testuser -ptestpass testdb
    if ($LASTEXITCODE -eq 0) {
        Write-Host "MySQL initialized successfully!" -ForegroundColor Green
    } else {
        Write-Host "MySQL initialization failed!" -ForegroundColor Red
    }
}

function Initialize-Oracle {
    Write-Host "Initializing Oracle..." -ForegroundColor Green
    $scriptPath = Join-Path $PSScriptRoot "init-oracle.sql"
    Get-Content $scriptPath | docker exec -i voyager-oracle sqlplus -S testuser/testpass@XEPDB1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Oracle initialized successfully!" -ForegroundColor Green
    } else {
        Write-Host "Oracle initialization failed!" -ForegroundColor Red
    }
}

switch ($Database.ToLower()) {
    "mssql" { Initialize-SqlServer }
    "postgres" { Initialize-PostgreSQL }
    "mysql" { Initialize-MySQL }
    "oracle" { Initialize-Oracle }
    "all" {
        Initialize-SqlServer
        Initialize-PostgreSQL
        Initialize-MySQL
        Initialize-Oracle
    }
    default {
        Write-Host "Unknown database: $Database" -ForegroundColor Red
        Write-Host "Usage: .\run-init-script.ps1 [-Database <mssql|postgres|mysql|oracle|all>]"
        exit 1
    }
}
