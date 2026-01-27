#!/bin/bash

# Wait for SQL Server to start
sleep 30s

# Run the initialization script
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -i /docker-entrypoint-initdb.d/init-databases.sql

echo "Database testdb initialized with schema and test data"
