#!/bin/bash

# Wait for SQL Server to start
sleep 30s

# Create database
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'testdb') CREATE DATABASE testdb"

echo "Database testdb initialized"
