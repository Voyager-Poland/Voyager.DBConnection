# Voyager.DBConnection.Logging

Logging extension for [Voyager.DBConnection](https://www.nuget.org/packages/Voyager.DBConnection/) that provides automatic logging of all database operations.

[![NuGet](https://img.shields.io/nuget/v/Voyager.DBConnection.Logging.svg)](https://www.nuget.org/packages/Voyager.DBConnection.Logging/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Features

- ✅ Automatic logging of all SQL operations (stored procedures, queries)
- ✅ Execution time tracking
- ✅ Parameter logging
- ✅ Error and exception logging
- ✅ Support for both `DbCommandExecutor` (modern) and `Connection` (legacy)
- ✅ Multi-framework support: .NET Framework 4.8, .NET 6.0, .NET 8.0
- ✅ Uses `Microsoft.Extensions.Logging.Abstractions` for flexible logging backends

## Installation

```bash
dotnet add package Voyager.DBConnection.Logging
```

## Quick Start

### Using with DbCommandExecutor (Recommended)

```csharp
using Microsoft.Extensions.Logging;
using Voyager.DBConnection;

// Create your logger (e.g., using Microsoft.Extensions.Logging)
ILogger logger = loggerFactory.CreateLogger("DatabaseOperations");

// Create database and executor
var database = new Database(factory, connectionString);
var executor = new DbCommandExecutor(database, errorPolicy);

// Add logging - logs all database operations automatically
executor.AddLogger(logger);

// All operations are now logged
var result = executor.ExecuteNonQuery(
    "InsertUser",
    cmd => cmd
        .WithInputParameter("Username", DbType.String, 50, "john_doe")
        .WithInputParameter("Email", DbType.String, 100, "john@example.com")
)
.Tap(rows => Console.WriteLine($"Inserted {rows} row(s)"))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

### Using with Connection (Legacy)

```csharp
using Microsoft.Extensions.Logging;
using Voyager.DBConnection;

var connection = new Connection(database, exceptionPolicy);
connection.AddLogger(logger);

// Operations are logged automatically
connection.ExecuteNonQuery(commandFactory);
```

## What Gets Logged

The logging extension captures:

- **Command Type**: SQL query, stored procedure name
- **Execution Time**: Duration of the operation in milliseconds
- **Parameters**: Input, output, and input/output parameters with values
- **Results**: Number of rows affected, scalar values returned
- **Errors**: Exception details, error messages, stack traces
- **Success/Failure**: Operation outcome

### Example Log Output

**Successful Operation:**
```
[Information] Executing stored procedure 'InsertUser' with parameters: Username='john_doe', Email='john@example.com'
[Information] Completed 'InsertUser' in 45ms - 1 row(s) affected
```

**Error:**
```
[Error] Executing stored procedure 'InsertUser' with parameters: Username='john_doe', Email='john@example.com'
[Error] Failed 'InsertUser' after 12ms - SqlException: Violation of UNIQUE KEY constraint 'UQ_Users_Username'
```

## Logging Backends

This package uses `Microsoft.Extensions.Logging.Abstractions`, which means you can use any logging provider:

### Console Logging

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var logger = loggerFactory.CreateLogger("Database");
executor.AddLogger(logger);
```

### Serilog

```csharp
using Serilog;
using Microsoft.Extensions.Logging;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/database.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSerilog();
});

var logger = loggerFactory.CreateLogger("Database");
executor.AddLogger(logger);
```

### NLog

```csharp
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddNLog();
});

var logger = loggerFactory.CreateLogger("Database");
executor.AddLogger(logger);
```

## Advanced Usage

### Filtering Logs

Use your logging framework's filtering capabilities:

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Database", LogLevel.Information) // Only log Info and above
        .AddConsole();
});
```

### Multiple Loggers

You can add the same logger to multiple executors:

```csharp
var sharedLogger = loggerFactory.CreateLogger("Database");

var executor1 = new DbCommandExecutor(database1, errorPolicy);
executor1.AddLogger(sharedLogger);

var executor2 = new DbCommandExecutor(database2, errorPolicy);
executor2.AddLogger(sharedLogger);
```

### Removing Logging

Logging is implemented as a feature that can be disposed:

```csharp
// The logger is automatically cleaned up when the executor is disposed
executor.Dispose();
```

## Performance Considerations

- Logging adds minimal overhead to database operations
- Parameter values are only serialized when logging is enabled
- Use appropriate log levels (`Information` for normal operations, `Error` for failures)
- Consider using structured logging for better performance in production

## Integration with ASP.NET Core

```csharp
// In Startup.cs or Program.cs
services.AddSingleton<IDbCommandExecutor>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<DbCommandExecutor>>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
    var database = new Database(factory, connectionString);
    var executor = new DbCommandExecutor(database, new SqlServerErrorPolicy());

    executor.AddLogger(logger);

    return executor;
});
```

## Integration with Application Insights

Azure Application Insights can track database operations as dependencies, providing rich telemetry including execution time, success/failure rates, and correlation with requests.

### Using Application Insights with Logging Extension

The simplest approach is to use the built-in logging extension with Application Insights logger:

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

// Configure Application Insights
var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
telemetryConfiguration.InstrumentationKey = "your-instrumentation-key";

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddApplicationInsights(telemetryConfiguration, options => { });
});

var logger = loggerFactory.CreateLogger("Database");

// Add logging to executor
var executor = new DbCommandExecutor(database, errorPolicy);
executor.AddLogger(logger);

// All database operations are now logged to Application Insights
executor.ExecuteNonQuery("InsertUser", cmd => cmd
    .WithInputParameter("Username", DbType.String, 50, "john_doe")
);
```

### Custom Telemetry with SqlCallEvent

For more granular control and richer telemetry, you can subscribe to database events directly and send custom telemetry:

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

public class ApplicationInsightsDatabaseFeature : IFeature
{
    private readonly TelemetryClient telemetryClient;
    private readonly IRegisterEvents registerEvents;

    public ApplicationInsightsDatabaseFeature(TelemetryClient telemetryClient, IRegisterEvents registerEvents)
    {
        this.telemetryClient = telemetryClient;
        this.registerEvents = registerEvents;
        this.registerEvents.AddEvent(TrackDatabaseOperation);
    }

    public void Dispose()
    {
        this.registerEvents.RemoveEvent(TrackDatabaseOperation);
    }

    private void TrackDatabaseOperation(SqlCallEvent sqlEvent)
    {
        var dependency = new DependencyTelemetry
        {
            Type = "SQL",
            Target = sqlEvent.DatabaseName ?? "Database",
            Name = sqlEvent.CommandText ?? "SQL Query",
            Data = sqlEvent.CommandText,
            Duration = sqlEvent.ExecutionTime,
            Success = !sqlEvent.IsError,
            Timestamp = DateTimeOffset.Now
        };

        // Add custom properties
        dependency.Properties["CommandType"] = sqlEvent.CommandType.ToString();
        dependency.Properties["ParameterCount"] = sqlEvent.Parameters?.Count.ToString() ?? "0";

        if (sqlEvent.IsError && !string.IsNullOrEmpty(sqlEvent.ErrorMessage))
        {
            dependency.Properties["ErrorMessage"] = sqlEvent.ErrorMessage;
        }

        // Add parameters (be careful with sensitive data!)
        if (sqlEvent.Parameters != null)
        {
            foreach (var param in sqlEvent.Parameters)
            {
                dependency.Properties[$"Param_{param.Key}"] = param.Value?.ToString() ?? "null";
            }
        }

        telemetryClient.TrackDependency(dependency);
    }
}

// Extension method for easy integration
public static class ApplicationInsightsExtensions
{
    public static void AddApplicationInsights(this DbCommandExecutor executor, TelemetryClient telemetryClient)
    {
        executor.AddFeature(new ApplicationInsightsDatabaseFeature(telemetryClient, executor));
    }

    public static void AddApplicationInsights(this Connection connection, TelemetryClient telemetryClient)
    {
        connection.AddFeature(new ApplicationInsightsDatabaseFeature(telemetryClient, connection));
    }
}

// Usage
var telemetryClient = new TelemetryClient(telemetryConfiguration);
var executor = new DbCommandExecutor(database, errorPolicy);
executor.AddApplicationInsights(telemetryClient);
```

### Application Insights in ASP.NET Core

For ASP.NET Core applications, use dependency injection:

```csharp
// In Program.cs or Startup.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.InstrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];
});

builder.Services.AddSingleton<IDbCommandExecutor>(provider =>
{
    var telemetryClient = provider.GetRequiredService<TelemetryClient>();
    var logger = provider.GetRequiredService<ILogger<DbCommandExecutor>>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
    var database = new Database(factory, connectionString);
    var executor = new DbCommandExecutor(database, new SqlServerErrorPolicy());

    // Option 1: Use logging extension (simpler)
    executor.AddLogger(logger);

    // Option 2: Use custom Application Insights feature (more control)
    executor.AddApplicationInsights(telemetryClient);

    return executor;
});
```

### Benefits of Application Insights Integration

When using Application Insights with Voyager.DBConnection.Logging, you get:

- **Dependency Tracking**: Database calls appear as dependencies in Application Insights
- **Performance Monitoring**: Track slow queries and execution times
- **Failure Analysis**: Identify and diagnose database errors
- **Query Analytics**: Analyze which queries are most frequently executed
- **Correlation**: Link database operations to specific user requests
- **Alerting**: Set up alerts for slow queries or high error rates
- **Custom Dashboards**: Create dashboards showing database performance metrics

### Example Query in Application Insights

```kusto
// Find slow database operations
dependencies
| where type == "SQL"
| where duration > 1000 // More than 1 second
| project timestamp, name, target, duration, success
| order by duration desc

// Database error rate
dependencies
| where type == "SQL"
| summarize Total = count(), Failures = countif(success == false) by name
| extend ErrorRate = (Failures * 100.0) / Total
| where ErrorRate > 5 // More than 5% error rate
| order by ErrorRate desc
```

### Security Considerations

When logging to Application Insights:

- **Never log sensitive data** (passwords, credit card numbers, PII)
- Use parameter names instead of values for sensitive parameters
- Consider implementing a filtering mechanism to exclude sensitive parameters
- Be mindful of logging volume and costs in high-traffic applications

```csharp
// Example: Filter sensitive parameters
private void TrackDatabaseOperation(SqlCallEvent sqlEvent)
{
    var dependency = new DependencyTelemetry { /* ... */ };

    // Only log non-sensitive parameters
    var sensitiveParams = new[] { "Password", "CreditCard", "SSN" };

    if (sqlEvent.Parameters != null)
    {
        foreach (var param in sqlEvent.Parameters.Where(p => !sensitiveParams.Contains(p.Key)))
        {
            dependency.Properties[$"Param_{param.Key}"] = param.Value?.ToString() ?? "null";
        }
    }

    telemetryClient.TrackDependency(dependency);
}
```

## Related Packages

- [Voyager.DBConnection](https://www.nuget.org/packages/Voyager.DBConnection/) - Core library
- [Voyager.DBConnection.MsSql](https://www.nuget.org/packages/Voyager.DBConnection.MsSql/) - SQL Server support
- [Voyager.DBConnection.Oracle](https://www.nuget.org/packages/Voyager.DBConnection.Oracle/) - Oracle support
- [Voyager.DBConnection.PostgreSql](https://www.nuget.org/packages/Voyager.DBConnection.PostgreSql/) - PostgreSQL support
- [Voyager.DBConnection.MySql](https://www.nuget.org/packages/Voyager.DBConnection.MySql/) - MySQL support

## License

MIT License - see [LICENSE](../../LICENSE) for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and questions:
- [GitHub Issues](https://github.com/Voyager-Poland/Voyager.DBConnection/issues)
- [Documentation](https://github.com/Voyager-Poland/Voyager.DBConnection)
