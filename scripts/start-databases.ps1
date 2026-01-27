# Start all database containers for integration testing

Write-Host "Starting database containers..." -ForegroundColor Green

# Start all databases
docker-compose up -d

Write-Host "`nWaiting for databases to be healthy..." -ForegroundColor Yellow

# Wait for health checks
$maxRetries = 30
$retryCount = 0

while ($retryCount -lt $maxRetries) {
    $unhealthy = docker-compose ps --filter "health=starting" --format json | ConvertFrom-Json

    if ($null -eq $unhealthy -or $unhealthy.Count -eq 0) {
        Write-Host "`nAll databases are healthy!" -ForegroundColor Green
        docker-compose ps
        exit 0
    }

    $retryCount++
    Write-Host "." -NoNewline
    Start-Sleep -Seconds 2
}

Write-Host "`nTimeout waiting for databases to be healthy" -ForegroundColor Red
docker-compose ps
exit 1
