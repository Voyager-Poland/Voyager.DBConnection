#!/bin/bash
# Start all database containers for integration testing

echo "Starting database containers..."

# Start all databases
docker-compose up -d

echo "Waiting for databases to be healthy..."

# Wait for health checks
max_retries=30
retry_count=0

while [ $retry_count -lt $max_retries ]; do
    unhealthy=$(docker-compose ps --filter "health=starting" -q)

    if [ -z "$unhealthy" ]; then
        echo ""
        echo "All databases are healthy!"
        docker-compose ps
        exit 0
    fi

    retry_count=$((retry_count + 1))
    echo -n "."
    sleep 2
done

echo ""
echo "Timeout waiting for databases to be healthy"
docker-compose ps
exit 1
