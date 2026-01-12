

# Voyager.DBConnection

## Overview

Voyager.DBConnection is a library providing a structured and type-safe way to connect to SQL databases using `DbProviderFactory`. It implements the Command Factory pattern to encapsulate database operations and provides a clean abstraction over ADO.NET.

**Key Features:**
- Type-safe database command construction using the Command Factory pattern
- Support for stored procedures and parameterized queries
- **Event-driven architecture** - `SqlCallEvent` provides rich telemetry for logging, monitoring, and custom analytics
- Built-in logging support through extensions
- MS SQL Server provider implementation included
- Clean separation between command construction and execution
- Result-based error handling with Result monad pattern
- Docker support for multi-database integration testing

## NuGet Packages

This repository provides multiple NuGet packages in a monorepo structure:

| Package | Description | NuGet |
|---------|-------------|-------|
| **Voyager.DBConnection** | Core library with DbProviderFactory support for all databases | [![NuGet](https://img.shields.io/nuget/v/Voyager.DBConnection.svg)](https://www.nuget.org/packages/Voyager.DBConnection/) |
| **Voyager.DBConnection.MsSql** | SQL Server-specific implementation with error mapping for both .NET Framework 4.8 and .NET 6.0+ | [![NuGet](https://img.shields.io/nuget/v/Voyager.DBConnection.MsSql.svg)](https://www.nuget.org/packages/Voyager.DBConnection.MsSql/) |
| **Voyager.DBConnection.Oracle** | Oracle Database-specific implementation with OracleDbCommandExecutor and error mapping | [![NuGet](https://img.shields.io/nuget/v/Voyager.DBConnection.Oracle.svg)](https://www.nuget.org/packages/Voyager.DBConnection.Oracle/) |
| **Voyager.DBConnection.PostgreSql** | PostgreSQL-specific implementation with PostgreSqlDbCommandExecutor and error mapping | [![NuGet](https://img.shields.io/nuget/v/Voyager.DBConnection.PostgreSql.svg)](https://www.nuget.org/packages/Voyager.DBConnection.PostgreSql/) |
| **Voyager.DBConnection.MySql** | MySQL-specific implementation with MySqlDbCommandExecutor and error mapping | [![NuGet](https://img.shields.io/nuget/v/Voyager.DBConnection.MySql.svg)](https://www.nuget.org/packages/Voyager.DBConnection.MySql/) |
| **Voyager.DBConnection.Logging** | Logging extension with support for both DbCommandExecutor and Connection | [![NuGet](https://img.shields.io/nuget/v/Voyager.DBConnection.Logging.svg)](https://www.nuget.org/packages/Voyager.DBConnection.Logging/) |

### Monorepo Benefits

All packages are developed and versioned together:
- ✅ **Synchronized Versioning** - All packages share the same version number
- ✅ **Single Build** - `dotnet build` builds all packages at once
- ✅ **ProjectReference in Development** - Use project references locally for immediate changes
- ✅ **PackageReference in NuGet** - Published packages correctly reference each other as NuGet dependencies
- ✅ **Unified CI/CD** - Single GitHub Actions workflow for all packages

## Docker Support

The project includes Docker Compose configuration for testing against multiple database providers:
- SQL Server 2022
- PostgreSQL 16
- MySQL 8.0
- Oracle XE 21

See [README.Docker.md](README.Docker.md) for detailed Docker setup and usage instructions.

**Quick Start:**
```bash
# Start all databases
docker-compose up -d

# Initialize test schema (SQL Server)
.\scripts\run-init-script.ps1 -Database mssql

# Run integration tests
dotnet test --filter "Category=Integration"

# Stop all databases
docker-compose down
```

## Getting Started - Recommended Approach

The modern way to use Voyager.DBConnection is through the `DbCommandExecutor` class with the `IDbCommandExecutor` interface. This approach provides:

- **Excellent Testability**: Easy to mock with `IDbCommandExecutor` interface - no need for complex database mocking
- **Separation of Concerns**: Command factories (`IDbCommandFactory`) keep command construction separate from execution logic
- **Result Monad Pattern**: Explicit error handling with `Result<T>` - no hidden exceptions or try-catch blocks
- **Multiple Command Patterns**: Use command factories, lambda functions, or direct stored procedure calls
- **Full Async Support**: Built-in async/await with CancellationToken support

### Basic Usage Example

```csharp
// Create executor
var errorPolicy = new SqlServerErrorPolicy(); // Maps SQL exceptions to typed errors
var executor = new DbCommandExecutor(database, errorPolicy);

// Execute with fluent API and Result monad
executor.ExecuteNonQuery(
    "InsertUser",
    cmd => cmd
        .WithInputParameter("Username", DbType.String, 50, username)
        .WithInputParameter("Email", DbType.String, 100, email)
        .WithOutputParameter("UserId", DbType.Int32, 0)
)
.Tap(rowsAffected => Console.WriteLine($"User created, {rowsAffected} rows affected"))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

### Why This Approach?

**Better Testability**: With `IDbCommandExecutor` interface, you can easily create unit tests:

```csharp
// Unit test with mock - no real database needed
var mockExecutor = new Mock<IDbCommandExecutor>();
mockExecutor.Setup(x => x.ExecuteNonQuery(It.IsAny<IDbCommandFactory>(), null))
    .Returns(Result<int>.Success(1));

var service = new UserService(mockExecutor.Object);
var result = service.CreateUser("john", "john@example.com");

Assert.That(result.IsSuccess, Is.True);
```

**Separation of Responsibilities**: Command factories encapsulate command construction:

```csharp
// Command factory - single responsibility: build the command
public class CreateUserCommandFactory : IDbCommandFactory
{
    private readonly string username;
    private readonly string email;

    public CreateUserCommandFactory(string username, string email)
    {
        this.username = username;
        this.email = email;
    }

    public DbCommand ConstructDbCommand(IDatabase db)
    {
        return db.GetStoredProcCommand("CreateUser")
            .WithInputParameter("Username", DbType.String, 50, username)
            .WithInputParameter("Email", DbType.String, 100, email);
    }
}

// Service class - single responsibility: business logic
public class UserService
{
    private readonly IDbCommandExecutor executor;

    public UserService(IDbCommandExecutor executor) => this.executor = executor;

    public Result<int> CreateUser(string username, string email)
    {
        var factory = new CreateUserCommandFactory(username, email);
        return executor.ExecuteNonQuery(factory);
    }
}
```

## Result-Based Error Handling

The `DbCommandExecutor` uses the Result monad pattern to encapsulate either a successful value or an error, eliminating the need for exception handling.

### Key Concepts

- **No Exception Throwing**: All methods return `Result<T>` which contains either a success value or an error
- **Implicit Conversions**: Cleaner code with automatic conversions:
  - `TValue` → `Result<TValue>` (success)
  - `Error` → `Result<TValue>` (failure)
  - No need to explicitly call `Result<T>.Success()` or `Result<T>.Failure()`
- **Typed Errors**: Categorize failures (ValidationError, DatabaseError, BusinessError, etc.)
- **Composable**: Chain operations with `Bind`, `Map`, `Ensure`, `OrElse`

### Usage Examples

#### ExecuteNonQuery - Using Command Factory

```csharp
var executor = new DbCommandExecutor(database, errorPolicy);
var factory = new SetPilotiDoPowiadomieniaFactory(id, date, user, report);

Result<int> result = executor.ExecuteNonQuery(factory)
    .Tap(rows => Console.WriteLine($"Rows affected: {rows}"))
    .TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

#### ExecuteNonQuery - Using Function

```csharp
Result<int> result = executor.ExecuteNonQuery(
    db => db.GetStoredProcCommand("MyStoredProc"),
    cmd => Console.WriteLine("Command executed")
);
```

#### ExecuteNonQuery - Direct Stored Procedure Call

```csharp
Result<int> result = executor.ExecuteNonQuery(
    "MyStoredProc",
    cmd => cmd
        .WithInputParameter("Param1", DbType.String, 50, value1)
        .WithInputParameter("Param2", DbType.Int32, value2)
        .WithOutputParameter("RowCount", DbType.Int32, 0),
    cmd => Console.WriteLine($"Command executed, rows affected: {cmd.GetParameterValue<int>("RowCount")}")
);
```

#### ExecuteScalar - Getting Single Value

```csharp
executor.ExecuteScalar(
    "GetUserCount",
    cmd => cmd.WithInputParameter("Active", DbType.Boolean, true)
)
.Tap(value =>
{
    int count = Convert.ToInt32(value);
    Console.WriteLine($"User count: {count}");
})
.TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

#### ExecuteReader - Reading Data

```csharp
// Using IResultsConsumer (classic approach)
var consumer = new RegionalSaleCommand(request);
executor.ExecuteReader(consumer, consumer)
    .Tap(items =>
    {
        foreach (var item in items)
        {
            Console.WriteLine($"Sale: {item.GidRezerwacji}");
        }
    })
    .TapError(error => Console.WriteLine($"Error: {error.Message}"));

// Using Func<IDataReader, TValue> (functional approach - recommended for simple cases)
executor.ExecuteReader(
    "GetUsers",
    cmd => cmd.WithInputParameter("Active", DbType.Boolean, true),
    reader =>
    {
        var users = new List<User>();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2)
            });
        }
        return users;
    }
)
.Tap(users => users.ForEach(u => Console.WriteLine($"{u.Name}: {u.Email}")))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));

// Async version with functional approach
var result = await executor.ExecuteReaderAsync(
    "GetActiveUsers",
    cmd => cmd.WithInputParameter("MinAge", DbType.Int32, 18),
    reader =>
    {
        var users = new List<User>();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("UserId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Age = reader.GetInt32(reader.GetOrdinal("Age"))
            });
        }
        return users;
    },
    afterCall: null,
    cancellationToken
);
```

#### ExecuteAndBind - Binding Results

```csharp
executor.ExecuteAndBind(
    factory,
    cmd =>
    {
        int outputId = cmd.GetParameterValue<int>("OutputId");
        if (outputId > 0)
            return outputId; // Implicit conversion to Result<int>

        return new Error("No output parameter"); // Implicit conversion to Result<int>
    }
)
.Tap(id => Console.WriteLine($"Output ID: {id}"))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

#### Async Operations

```csharp
// With CancellationToken
CancellationToken cancellationToken = GetCancellationToken();

Result<int> result = await executor.ExecuteNonQueryAsync(
    factory,
    afterCall: null,
    cancellationToken
);

// ExecuteReaderAsync
Result<SaleItem[]> result = await executor.ExecuteReaderAsync(
    "GetSalesReport",
    cmd => cmd
        .WithInputParameter("DateFrom", DbType.DateTime, dateFrom)
        .WithInputParameter("DateTo", DbType.DateTime, dateTo),
    consumer,
    afterCall: null,
    cancellationToken
);
```

### Method Overloads

All execution methods (`ExecuteNonQuery`, `ExecuteScalar`, `ExecuteReader`, `ExecuteAndBind`) support three patterns:

1. **IDbCommandFactory**: Using command factory pattern
2. **Func<IDatabase, DbCommand>**: Using a function to create the command
3. **string procedureName**: Direct stored procedure call with parameter configuration

**ExecuteReader** additionally supports two ways to process results:
- **IResultsConsumer<TValue>**: Interface-based approach (recommended for complex scenarios, reusable consumers)
- **Func<IDataReader, TValue>**: Functional approach (recommended for simple, inline processing)

Each pattern has both synchronous and asynchronous versions, with async versions supporting `CancellationToken`.

**Example - All ExecuteReader patterns:**

```csharp
// Pattern 1: IDbCommandFactory + IResultsConsumer
var factory = new GetUsersCommandFactory(activeOnly: true);
var consumer = new UserListConsumer();
Result<List<User>> result1 = executor.ExecuteReader(factory, consumer);

// Pattern 2: IDbCommandFactory + Func<IDataReader, TValue>
var factory2 = new GetUsersCommandFactory(activeOnly: true);
Result<List<User>> result2 = executor.ExecuteReader(factory2, reader =>
{
    var users = new List<User>();
    while (reader.Read())
        users.Add(MapUser(reader));
    return users;
});

// Pattern 3: Func<IDatabase, DbCommand> + IResultsConsumer
Result<List<User>> result3 = executor.ExecuteReader(
    db => db.GetStoredProcCommand("GetUsers")
          .WithInputParameter("Active", DbType.Boolean, true),
    new UserListConsumer()
);

// Pattern 4: Func<IDatabase, DbCommand> + Func<IDataReader, TValue>
Result<List<User>> result4 = executor.ExecuteReader(
    db => db.GetStoredProcCommand("GetUsers")
          .WithInputParameter("Active", DbType.Boolean, true),
    reader =>
    {
        var users = new List<User>();
        while (reader.Read())
            users.Add(MapUser(reader));
        return users;
    }
);

// Pattern 5: string procedureName + IResultsConsumer
Result<List<User>> result5 = executor.ExecuteReader(
    "GetUsers",
    cmd => cmd.WithInputParameter("Active", DbType.Boolean, true),
    new UserListConsumer()
);

// Pattern 6: string procedureName + Func<IDataReader, TValue> (most concise)
Result<List<User>> result6 = executor.ExecuteReader(
    "GetUsers",
    cmd => cmd.WithInputParameter("Active", DbType.Boolean, true),
    reader =>
    {
        var users = new List<User>();
        while (reader.Read())
            users.Add(MapUser(reader));
        return users;
    }
);

static User MapUser(IDataReader reader) => new User
{
    Id = reader.GetInt32(0),
    Name = reader.GetString(1),
    Email = reader.GetString(2)
};
```

### Advanced Result Monad Usage

The Result type provides powerful functional programming methods for validation, transformation, and error handling.

#### Input Validation with Ensure

Use `Ensure` to validate input parameters before executing database operations. Use typed errors (`Error.ValidationError`, `Error.BusinessError`, etc.) to categorize failures:

```csharp
// Validate user input before database operation
Result<User> CreateUser(string username, string email, int age)
{
    return Result<User>.Success(new User { Username = username, Email = email, Age = age })
        .Ensure(u => !string.IsNullOrWhiteSpace(u.Username),
            Error.ValidationError("User.InvalidUsername", "Username cannot be empty"))
        .Ensure(u => u.Username.Length >= 3,
            Error.ValidationError("User.UsernameTooShort", "Username must be at least 3 characters"))
        .Ensure(u => u.Email.Contains("@"),
            Error.ValidationError("User.InvalidEmail", "Invalid email format"))
        .Ensure(u => u.Age >= 18,
            Error.BusinessError("User.AgeLimitNotMet", "User must be 18 or older"))
        .Bind(user => executor.ExecuteAndBind(
            db => db.GetStoredProcCommand("CreateUser")
                .WithInputParameter("Username", DbType.String, 50, user.Username)
                .WithInputParameter("Email", DbType.String, 100, user.Email)
                .WithInputParameter("Age", DbType.Int32, user.Age)
                .WithOutputParameter("UserId", DbType.Int32, 0),
            cmd => new User
            {
                UserId = cmd.GetParameterValue<int>("UserId"),
                Username = user.Username,
                Email = user.Email,
                Age = user.Age
            }
        ));
}

// Usage
CreateUser("john", "john@example.com", 25)
    .Tap(user => Console.WriteLine($"User created: {user.UserId}"))
    .TapError(error => Console.WriteLine($"[{error.Type}] {error.Code}: {error.Message}"));
```

#### Result Mapping and Transformation

Use `Map` to transform successful results without unwrapping:

```csharp
// Map database result to DTO
executor.ExecuteAndBind(
    db => db.GetStoredProcCommand("GetUserById")
        .WithInputParameter("UserId", DbType.Int32, userId),
    cmd => new User
    {
        UserId = cmd.GetParameterValue<int>("UserId"),
        Username = cmd.GetParameterValue<string>("Username"),
        Email = cmd.GetParameterValue<string>("Email"),
        IsActive = cmd.GetParameterValue<bool>("IsActive")
    }
)
.Map(user => new UserDTO  // Transform User to UserDTO
{
    Id = user.UserId,
    DisplayName = user.Username.ToUpper(),
    ContactEmail = user.Email,
    Status = user.IsActive ? "Active" : "Inactive"
})
.Tap(dto => Console.WriteLine($"User: {dto.DisplayName} ({dto.Status})"))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

#### Chaining Multiple Operations with Bind

Use `Bind` to chain dependent database operations:

```csharp
// Create order and then add order items
Result<Order> CreateOrderWithItems(int userId, List<OrderItem> items)
{
    return executor.ExecuteAndBind(
        db => db.GetStoredProcCommand("CreateOrder")
            .WithInputParameter("UserId", DbType.Int32, userId)
            .WithInputParameter("OrderDate", DbType.DateTime, DateTime.Now)
            .WithOutputParameter("OrderId", DbType.Int32, 0),
        cmd => new Order
        {
            OrderId = cmd.GetParameterValue<int>("OrderId"),
            UserId = userId,
            OrderDate = DateTime.Now
        }
    )
    .Bind(order =>
    {
        // Chain: Add items to the created order
        var itemResults = items.Select(item =>
            executor.ExecuteNonQuery(
                db => db.GetStoredProcCommand("AddOrderItem")
                    .WithInputParameter("OrderId", DbType.Int32, order.OrderId)
                    .WithInputParameter("ProductId", DbType.Int32, item.ProductId)
                    .WithInputParameter("Quantity", DbType.Int32, item.Quantity)
                    .WithInputParameter("Price", DbType.Decimal, item.Price)
            )
        ).ToList();

        // If any item failed, return error; otherwise return order
        var failedItem = itemResults.FirstOrDefault(r => !r.IsSuccess);
        return failedItem != null
            ? failedItem.Error  // Implicit conversion Error -> Result<Order>
            : order;            // Implicit conversion Order -> Result<Order>
    });
}

// Usage with validation
CreateOrderWithItems(userId, orderItems)
    .Ensure(order => order.OrderId > 0,
        Error.BusinessError("Order.InvalidId", "Invalid order ID"))
    .Tap(order => Console.WriteLine($"Order created: {order.OrderId}"))
    .TapError(error => Console.WriteLine($"Order creation failed: {error.Message}"));
```

#### Fallback with OrElse

Use `OrElse` to provide fallback values or alternative operations. **Important**: Use `TapError` before `OrElse` for logging, as `OrElse` always returns success and subsequent `TapError` won't execute:

```csharp
// Try to get user from database, fallback to default user
Result<User> GetUserOrDefault(int userId)
{
    return executor.ExecuteAndBind(
        db => db.GetStoredProcCommand("GetUserById")
            .WithInputParameter("UserId", DbType.Int32, userId),
        cmd =>
        {
            var id = cmd.GetParameterValue<int>("UserId");
            if (id == 0)
                return Error.NotFoundError("User.NotFound", $"User {userId} not found");

            return new User
            {
                UserId = id,
                Username = cmd.GetParameterValue<string>("Username"),
                Email = cmd.GetParameterValue<string>("Email")
            };
        }
    )
    .TapError(error => _logger.LogWarning($"User not found, using guest: {error.Message}"))
    .OrElse(() => new User  // Fallback to guest user
    {
        UserId = 0,
        Username = "Guest",
        Email = "guest@example.com"
    });
}

// Usage - TapError won't execute because OrElse always succeeds
GetUserOrDefault(userId)
    .Tap(user => Console.WriteLine($"User: {user.Username}"));

// Fallback to alternative database query
Result<Product> GetProductWithFallback(int productId)
{
    return executor.ExecuteAndBind(
        db => db.GetStoredProcCommand("GetActiveProduct")
            .WithInputParameter("ProductId", DbType.Int32, productId),
        cmd => MapProduct(cmd)
    )
    .TapError(error => _logger.LogInformation($"Active product not found, trying archived: {error.Message}"))
    .OrElse(() =>
        // Fallback: Try to get archived product
        executor.ExecuteAndBind(
            db => db.GetStoredProcCommand("GetArchivedProduct")
                .WithInputParameter("ProductId", DbType.Int32, productId),
            cmd => MapProduct(cmd)
        )
    );
}

static Product MapProduct(DbCommand cmd) => new Product
{
    ProductId = cmd.GetParameterValue<int>("ProductId"),
    Name = cmd.GetParameterValue<string>("Name"),
    Price = cmd.GetParameterValue<decimal>("Price")
};
```

#### Complex Example: Validation, Transformation, and Logging

```csharp
// Complete workflow with validation, mapping, and error logging
Result<OrderSummary> ProcessOrder(OrderRequest request)
{
    // Step 1: Validate input
    return Result<OrderRequest>.Success(request)
        .Ensure(r => r.UserId > 0,
            Error.ValidationError("Order.InvalidUserId", "Invalid user ID"))
        .Ensure(r => r.Items?.Count > 0,
            Error.ValidationError("Order.EmptyItems", "Order must contain at least one item"))
        .Ensure(r => r.Items.All(i => i.Quantity > 0),
            Error.ValidationError("Order.InvalidQuantity", "All items must have positive quantity"))

        // Step 2: Execute database operation
        .Bind(validRequest => executor.ExecuteAndBind(
            db => db.GetStoredProcCommand("CreateOrder")
                .WithInputParameter("UserId", DbType.Int32, validRequest.UserId)
                .WithInputParameter("TotalAmount", DbType.Decimal, validRequest.TotalAmount)
                .WithOutputParameter("OrderId", DbType.Int32, 0)
                .WithOutputParameter("OrderNumber", DbType.String, 50),
            cmd => new OrderResult
            {
                OrderId = cmd.GetParameterValue<int>("OrderId"),
                OrderNumber = cmd.GetParameterValue<string>("OrderNumber"),
                TotalAmount = validRequest.TotalAmount
            }
        ))

        // Step 3: Ensure database operation succeeded
        .Ensure(result => result.OrderId > 0,
            Error.DatabaseError("Order.CreationFailed", "Order creation failed"))

        // Step 4: Transform to summary DTO
        .Map(orderResult => new OrderSummary
        {
            OrderId = orderResult.OrderId,
            OrderNumber = orderResult.OrderNumber,
            Total = orderResult.TotalAmount,
            Status = "Created",
            CreatedDate = DateTime.Now
        });
}

// Usage - TapError is useful here since we don't use OrElse
ProcessOrder(orderRequest)
    .Tap(summary =>
    {
        _logger.LogInformation($"Order processed: {summary.OrderNumber} - ${summary.Total}");
        Console.WriteLine($"Order #{summary.OrderNumber} created successfully");
    })
    .TapError(error =>
    {
        _logger.LogError($"[{error.Type}] {error.Code}: {error.Message}");
        Console.WriteLine($"Failed to process order: {error.Message}");
    });

// Alternative: Using fallback with TapError BEFORE OrElse for logging
Result<OrderSummary> ProcessOrderWithFallback(OrderRequest request)
{
    return ProcessOrder(request)
        .TapError(error => _logger.LogWarning($"Order creation failed, creating draft: {error.Message}"))
        .OrElse(() => new OrderSummary  // Fallback to draft order
        {
            OrderId = 0,
            OrderNumber = "DRAFT",
            Total = request.TotalAmount,
            Status = "Draft",
            CreatedDate = DateTime.Now
        });
}

// Usage - After OrElse, TapError won't execute
ProcessOrderWithFallback(orderRequest)
    .Tap(summary => Console.WriteLine($"Order: {summary.OrderNumber} ({summary.Status})"));
```

These patterns enable:
- **Defensive Programming**: Validate inputs before expensive database operations
- **Composability**: Chain multiple operations without nested if-statements
- **Error Recovery**: Provide fallback values or alternative data sources
- **Transformation**: Map database results to DTOs cleanly
- **Maintainability**: Clear, declarative code flow

### Error Type Categorization

The library uses typed errors to categorize failures:

- **`Error.ValidationError`**: Input validation failures (empty fields, format errors)
- **`Error.BusinessError`**: Business rule violations (age restrictions, insufficient funds)
- **`Error.DatabaseError`**: Database operation failures (mapped from exceptions via `IMapErrorPolicy`)
- **`Error.NotFoundError`**: Entity not found errors
- **`Error.ConflictError`**: Concurrency or uniqueness violations
- **`Error.UnauthorizedError`**: Authentication failures
- **`Error.PermissionError`**: Authorization failures
- **`Error.TimeoutError`**: Operation timeout
- **`Error.CancelledError`**: Operation cancelled
- **`Error.UnavailableError`**: Service temporarily unavailable
- **`Error.UnexpectedError`**: Unexpected system errors

### Custom Error Mapping with IMapErrorPolicy

The `DbCommandExecutor` uses `IMapErrorPolicy` to map database exceptions to domain-specific errors. This ensures that low-level database exceptions are converted to meaningful, typed errors.

```csharp
public interface IMapErrorPolicy
{
    Error MapError(Exception ex);
}

// Example: Custom error policy for SQL Server
public class SqlServerErrorPolicy : IMapErrorPolicy
{
    public Error MapError(Exception ex)
    {
        return ex switch
        {
            // Unique constraint violations - conflicts that can be handled by application logic
            SqlException sqlEx when sqlEx.Number == 2627 || sqlEx.Number == 2601 =>
                Error.ConflictError("Database.UniqueConstraint", "Record already exists"),

            // Foreign key constraint violations - business rule violations, not retryable
            SqlException sqlEx when sqlEx.Number == 547 =>
                Error.BusinessError("Database.ForeignKeyViolation", "Referenced record does not exist or cannot be deleted due to existing references"),

            // Deadlock - transient error, can be retried
            SqlException sqlEx when sqlEx.Number == 1205 =>
                Error.DatabaseError("Database.Deadlock", "Deadlock detected, operation can be retried"),

            // Timeout - transient error, can be retried
            SqlException sqlEx when sqlEx.Number == -2 =>
                Error.TimeoutError("Database.Timeout", "Database operation timed out"),

            TimeoutException =>
                Error.TimeoutError("Database.ConnectionTimeout", ex.Message),

            InvalidOperationException =>
                Error.DatabaseError("Database.InvalidOperation", ex.Message),

            _ =>
                Error.UnexpectedError("Database.UnexpectedError", ex.Message)
        };
    }
}

// Usage: Create executor with custom error policy
var errorPolicy = new SqlServerErrorPolicy();
var executor = new DbCommandExecutor(database, errorPolicy);

// Database errors are automatically mapped
executor.ExecuteNonQuery(
    "InsertUser",
    cmd => cmd
        .WithInputParameter("Email", DbType.String, 100, email)
        .WithInputParameter("Username", DbType.String, 50, username)
)
.TapError(error =>
{
    // Error is typed based on the exception - handle differently based on whether retry makes sense
    switch (error.Type)
    {
        case ErrorType.Conflict:
            _logger.LogWarning($"Duplicate user: {error.Message}");
            // Conflict (unique constraint) - can be handled by application logic, no retry
            break;

        case ErrorType.Business:
            _logger.LogWarning($"Business rule violation: {error.Message}");
            // Business error (e.g., foreign key violation) - indicates domain rule violation, no retry
            break;

        case ErrorType.Timeout:
            _logger.LogError($"Database timeout: {error.Message}");
            // Timeout - transient error, can be retried
            break;

        case ErrorType.Database:
            // Database errors (e.g., deadlock) - transient errors that can be retried
            if (error.Code.Contains("Deadlock"))
            {
                _logger.LogWarning($"Deadlock detected, will retry: {error.Message}");
            }
            else
            {
                _logger.LogError($"Database error: {error.Message}");
            }
            break;

        default:
            _logger.LogError($"Unexpected error: {error.Message}");
            break;
    }
});

// Example: Retry logic for transient errors only
Result<int> InsertUserWithRetry(string email, string username)
{
    int maxRetries = 3;
    int attempt = 0;

    while (attempt < maxRetries)
    {
        var result = executor.ExecuteNonQuery(
            "InsertUser",
            cmd => cmd
                .WithInputParameter("Email", DbType.String, 100, email)
                .WithInputParameter("Username", DbType.String, 50, username)
        );

        if (result.IsSuccess)
            return result;

        // Only retry for transient errors (Database, Timeout)
        var shouldRetry = result.Error.Type == ErrorType.Database ||
                         result.Error.Type == ErrorType.Timeout;

        if (!shouldRetry || attempt >= maxRetries - 1)
        {
            _logger.LogError($"Operation failed: {result.Error.Message}");
            return result;
        }

        attempt++;
        _logger.LogWarning($"Transient error, retrying (attempt {attempt}/{maxRetries}): {result.Error.Message}");
        Thread.Sleep(TimeSpan.FromMilliseconds(100 * attempt)); // Exponential backoff
    }

    return Error.UnexpectedError("Retry.MaxAttemptsExceeded", "Maximum retry attempts exceeded");
}
```

**Error Type Guidelines for Retry Logic:**

- **Retryable** (transient errors):
  - `ErrorType.Database` - Deadlocks, connection issues
  - `ErrorType.Timeout` - Operation timeouts
  - `ErrorType.Unavailable` - Service temporarily unavailable

- **Not Retryable** (permanent errors):
  - `ErrorType.Validation` - Input validation failures
  - `ErrorType.Business` - Business rule violations (including foreign key constraints)
  - `ErrorType.Conflict` - Unique constraint violations
  - `ErrorType.NotFound` - Entity not found
  - `ErrorType.Permission` - Authorization failures
  - `ErrorType.Unauthorized` - Authentication failures

If no custom policy is provided, `DbCommandExecutor` uses `DefaultMapError` which maps all exceptions to generic database errors.

## Fluent Parameter Management

The library provides fluent API extension methods for managing `DbCommand` parameters through the `DbCommandExtensions` class. These extensions simplify parameter handling and automatically handle parameter naming conventions for different database providers.

### Supported Database Providers

The extensions automatically detect and apply the correct parameter prefix:
- **MS SQL Server, PostgreSQL, MySQL, SQLite**: `@` prefix
- **Oracle**: `:` prefix
- **Other providers**: No prefix (as-is)

### Adding Parameters

#### Input Parameters

```csharp
// Basic input parameter
command.WithInputParameter("UserId", DbType.Int32, 123);

// Input parameter with size (for strings, etc.)
command.WithInputParameter("UserName", DbType.String, 50, "John Doe");

// Fluent chaining
var result = executor.ExecuteNonQuery(
    db => db.GetStoredProcCommand("UpdateUser")
        .WithInputParameter("UserId", DbType.Int32, userId)
        .WithInputParameter("UserName", DbType.String, 50, userName)
        .WithInputParameter("Email", DbType.String, 100, email)
);
```

#### Output Parameters

```csharp
// Add output parameter
command.WithOutputParameter("NewId", DbType.Int32, 0);

// Using with ExecuteAndBind to read output
var result = executor.ExecuteAndBind(
    db => db.GetStoredProcCommand("CreateUser")
        .WithInputParameter("UserName", DbType.String, 50, "John Doe")
        .WithOutputParameter("NewId", DbType.Int32, 0),
    cmd => cmd.GetParameterValue<int>("NewId") // Implicit conversion to Result<int>
);
```

#### Input/Output Parameters

```csharp
// Add input/output parameter
command.WithInputOutputParameter("Counter", DbType.Int32, currentCount);

// With size specification
command.WithInputOutputParameter("Status", DbType.String, 20, "Pending");

// Example usage
var result = executor.ExecuteAndBind(
    db => db.GetStoredProcCommand("ProcessOrder")
        .WithInputParameter("OrderId", DbType.Int32, orderId)
        .WithInputOutputParameter("Status", DbType.String, 20, "Processing"),
    cmd => cmd.GetParameterValue<string>("Status") // Implicit conversion to Result<string>
);
```

#### Custom Parameter Configuration

```csharp
// Full parameter configuration for standard types
command.WithParameter(
    name: "CustomParam",
    dbType: DbType.Decimal,
    size: 18,
    direction: ParameterDirection.ReturnValue,
    value: 0
);

// For non-standard parameters (table-valued, structured types, etc.)
// use SqlParameter directly
var result = executor.ExecuteNonQuery(
    "BulkInsertProducts",
    cmd =>
    {
        // Table-valued parameter - requires SqlParameter
        var tableParam = new SqlParameter("@ProductTable", SqlDbType.Structured)
        {
            TypeName = "dbo.ProductTableType",
            Value = productDataTable
        };
        cmd.Parameters.Add(tableParam);

        // Regular parameters - use fluent API
        cmd.WithInputParameter("BatchId", DbType.Int32, batchId)
           .WithInputParameter("ProcessedBy", DbType.String, 50, userName);
    }
);
```

### Reading Parameter Values

#### Generic Type Reading

```csharp
// Read parameter value with type conversion
int newId = command.GetParameterValue<int>("NewId");
string status = command.GetParameterValue<string>("Status");
decimal? amount = command.GetParameterValue<decimal?>("Amount");

// Returns default(T) for null/DBNull values
int count = command.GetParameterValue<int>("Count"); // Returns 0 if null
```

#### Object Reading

```csharp
// Read parameter value as object
object value = command.GetParameterValue("SomeParam");

// Manual type conversion
if (value != null && value != DBNull.Value)
{
    int intValue = Convert.ToInt32(value);
}
```

### Complete Example with Parameters

```csharp
var executor = new DbCommandExecutor(database, errorPolicy);

// Example 1: Insert with output parameter
executor.ExecuteAndBind(
    db => db.GetStoredProcCommand("InsertProduct")
        .WithInputParameter("ProductName", DbType.String, 100, "Laptop")
        .WithInputParameter("Price", DbType.Decimal, 999.99m)
        .WithInputParameter("CategoryId", DbType.Int32, 5)
        .WithOutputParameter("ProductId", DbType.Int32, 0),
    cmd => cmd.GetParameterValue<int>("ProductId") // Implicit conversion to Result<int>
)
.Tap(productId => Console.WriteLine($"New Product ID: {productId}"))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));

// Example 2: Update with input/output parameter
executor.ExecuteAndBind(
    db => db.GetStoredProcCommand("UpdateInventory")
        .WithInputParameter("ProductId", DbType.Int32, productId)
        .WithInputOutputParameter("Quantity", DbType.Int32, requestedQuantity),
    cmd => cmd.GetParameterValue<int>("Quantity") // Implicit conversion to Result<int>
)
.Tap(actualQuantity => Console.WriteLine($"Actual quantity updated: {actualQuantity}"))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));

// Example 3: Complex stored procedure call
executor.ExecuteAndBind(
    db => db.GetStoredProcCommand("ProcessOrder")
        .WithInputParameter("OrderId", DbType.Int32, orderId)
        .WithInputParameter("UserId", DbType.Int32, userId)
        .WithInputParameter("TotalAmount", DbType.Decimal, totalAmount)
        .WithOutputParameter("TransactionId", DbType.Int32, 0)
        .WithOutputParameter("Status", DbType.String, 50)
        .WithInputOutputParameter("AvailableCredit", DbType.Decimal, currentCredit),
    cmd => new OrderProcessingResult // Implicit conversion to Result<OrderProcessingResult>
    {
        TransactionId = cmd.GetParameterValue<int>("TransactionId"),
        Status = cmd.GetParameterValue<string>("Status"),
        RemainingCredit = cmd.GetParameterValue<decimal>("AvailableCredit")
    }
)
.Tap(result => Console.WriteLine($"Order processed: TX#{result.TransactionId}, Status: {result.Status}, Credit: {result.RemainingCredit}"))
.TapError(error => Console.WriteLine($"Error processing order: {error.Message}"));
```

### Benefits of Fluent API

- **Fluent Chaining**: Build commands with multiple parameters in a readable, chainable way
- **Type Safety**: Strongly typed parameter values and return types
- **Automatic Prefix Handling**: No need to remember provider-specific parameter prefixes
- **Simplified Output Reading**: Generic `GetParameterValue<T>` handles type conversion and null values
- **Cleaner Code**: Reduces boilerplate code for parameter management

## Legacy API - Connection Class (Deprecated)

> **⚠️ Warning**: The `Connection` class API is being phased out. While still supported, it is recommended to use `DbCommandExecutor` with `IDbCommandExecutor` interface for new development.
>
> **Why switch to DbCommandExecutor?**
> - **Better Testability**: `IDbCommandExecutor` interface makes unit testing easier with mocking frameworks
> - **Separation of Concerns**: Command factories (`IDbCommandFactory`) separate command construction from execution
> - **No Exception Handling**: Result monad pattern (`Result<T>`) provides explicit error handling without try-catch blocks
> - **Cleaner Code**: Fluent parameter API and implicit conversions reduce boilerplate

### Legacy: Using ICommandFactory with Connection

The `ICommandFactory` interface allows you to encapsulate database command construction:

```csharp
public interface ICommandFactory : IReadOutParameters
{
    DbCommand ConstructDbCommand(Database db);
}
```

Example implementation for executing a stored procedure:

```csharp
internal class SetPilotiDoPowiadomieniaFactory : ICommandFactory
{
    private readonly int idBusMapRNo;
    private readonly DateTime busMapDate;
    private readonly string idAkwizytor;
    private readonly string raport;

    public SetPilotiDoPowiadomieniaFactory(int idBusMapRNo, DateTime busMapDate, string idAkwizytor, string raport)
    {
        this.idBusMapRNo = idBusMapRNo;
        this.busMapDate = busMapDate;
        this.idAkwizytor = idAkwizytor;
        this.raport = raport;
    }

    public DbCommand ConstructDbCommand(Database db)
    {
        var cmd = db.GetStoredProcCommand("BusMap.p_SetPilotiDoPowiadomienia");
        db.AddInParameter(cmd, "IdBusMapRNo", DbType.Int32, this.idBusMapRNo);
        db.AddInParameter(cmd, "BusMapDate", DbType.Date, this.busMapDate);
        db.AddInParameter(cmd, "IdAkwizytor", DbType.AnsiString, this.idAkwizytor);
        db.AddInParameter(cmd, "Raport", DbType.AnsiString, this.raport);
        return cmd;
    }

    public void ReadOutParameters(Database db, DbCommand command) { }
}
```

Execute the command using `Connection`:

```csharp
var factory = new SetPilotiDoPowiadomieniaFactory(
    tagItem.IdBusMapRNp,
    tagItem.BusMapDate,
    tagItem.IdAkwizytor,
    tagItem.Raport
);
connection.ExecuteNonQuery(factory); // Throws exception on error
```

### Legacy: Reading Data with Connection

For reading data, implement the `IResultsConsumer` interface:

```csharp
internal class RegionalSaleCommand : ICommandFactory, IResultsConsumer<SaleItem[]>
{
    private RaportRequest request;

    public RegionalSaleCommand(RaportRequest request)
    {
        this.request = request;
    }

    public DbCommand ConstructDbCommand(Database db)
    {
        var cmd = db.GetStoredProcCommand("[dbo].[TestSaleReport]");
        db.AddInParameter(cmd, "IdAkwizytorRowNo", DbType.Int32, request.IdAkwizytorRowNo);
        db.AddInParameter(cmd, "IdPrzewoznikRowNo", DbType.Int32, request.IdPrzewoznikRowNo);
        db.AddInParameter(cmd, "DataPocz", DbType.DateTime, request.DateFrom);
        db.AddInParameter(cmd, "DataKon", DbType.DateTime, request.DateTo);
        return cmd;
    }

    public SaleItem[] GetResults(IDataReader dataReader)
    {
        var lista = new List<SaleItem>();
        while (dataReader.Read())
        {
            int col = 0;
            var item = new SaleItem
            {
                GidRezerwacji = dataReader.GetString(col++),
                GIDL = dataReader.GetString(col++),
                // ... more field mappings
            };
            lista.Add(item);
        }
        return lista.ToArray();
    }

    public void ReadOutParameters(Database db, DbCommand command) { }
}
```

Call the `GetReader` method:

```csharp
public class RaportDB
{
    private readonly Connection connection;

    public RaportDB(Connection connection)
    {
        this.connection = connection;
    }

    public RaportResponse GetRaport(RaportRequest request)
    {
        var command = new RegionalSaleCommand(request);
        return new RaportResponse
        {
            Items = connection.GetReader(command, command) // Throws exception on error
        };
    }
}
```

> **Migration Tip**: To migrate from `Connection` to `DbCommandExecutor`:
> 1. Keep your `ICommandFactory` implementations unchanged
> 2. Replace `Connection` with `IDbCommandExecutor` in your constructors
> 3. Change method calls to use `Result<T>` return types:
>    - `connection.ExecuteNonQuery(factory)` → `executor.ExecuteNonQuery(factory).Tap(...).TapError(...)`
>    - `connection.GetReader(factory, consumer)` → `executor.ExecuteReader(factory, consumer).Tap(...).TapError(...)`
> 4. Replace try-catch blocks with `.TapError()` for error handling
>
> **Note**: The legacy `IGetConsumer<T>` interface is still supported but deprecated. It will be removed in version 5.0. Please use `IResultsConsumer<T>` instead.

## Logging

> **Note**: `Voyager.DBConnection.Logging` is available as a separate NuGet package with comprehensive documentation including Application Insights integration. See [src/Voyager.DBConnection.Logging/README.md](src/Voyager.DBConnection.Logging/README.md) for detailed usage examples and advanced scenarios.

The `Voyager.DBConnection.Logging` extension provides logging capabilities for database operations. It supports both the modern `DbCommandExecutor` and legacy `Connection` classes.

**Installation**:
```bash
dotnet add package Voyager.DBConnection.Logging
```

### Using with DbCommandExecutor (Recommended)

```csharp
using Microsoft.Extensions.Logging;
using Voyager.DBConnection;

// Create executor with logging
var database = new Database(factory, connectionString);
var executor = new DbCommandExecutor(database, errorPolicy);

// Add logger - logs all database operations
executor.AddLogger(logger);

// Now all operations are automatically logged
executor.ExecuteNonQuery(
    "InsertUser",
    cmd => cmd
        .WithInputParameter("Username", DbType.String, 50, "john")
        .WithInputParameter("Email", DbType.String, 100, "john@example.com")
)
.Tap(rows => Console.WriteLine($"{rows} row(s) inserted"))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

### Using with Connection (Legacy)

```csharp
using Microsoft.Extensions.Logging;
using Voyager.DBConnection;

var connection = new Connection(database, exceptionPolicy);
connection.AddLogger(logger);
```

### Database Events and Telemetry

The logging extension uses the `SqlCallEvent` event system, which provides rich telemetry data for each database operation:

**Available event data (`SqlCallEvent`)**:
- `CommandText` - SQL command or stored procedure name
- `CommandType` - Type of command (StoredProcedure, Text, etc.)
- `ExecutionTime` - Precise execution duration (TimeSpan)
- `Parameters` - Dictionary of parameter names and values
- `DatabaseName` - Target database name
- `IsError` - Success/failure indicator
- `ErrorMessage` - Detailed error information (if failed)

**Use cases for events**:
- **Logging**: Built-in support via `AddLogger()` extension
- **Application Insights**: Track database operations as dependencies with custom telemetry
- **Performance monitoring**: Measure and alert on slow queries
- **Audit trails**: Record all database operations for compliance
- **Custom metrics**: Export to Prometheus, DataDog, or other monitoring systems

See the [Voyager.DBConnection.Logging README](src/Voyager.DBConnection.Logging/README.md) for detailed examples of Application Insights integration and custom telemetry features

## ODBC Support

Voyager.DBConnection fully supports ODBC connections through `System.Data.Odbc`. ODBC enables connectivity to virtually any database with an ODBC driver, including legacy systems and databases without native .NET providers.

### Using ODBC Connections

```csharp
using System.Data.Common;
using System.Data.Odbc;
using Voyager.DBConnection;

// Register ODBC provider (if not already registered)
DbProviderFactories.RegisterFactory("System.Data.Odbc", OdbcFactory.Instance);

// Create database with ODBC
var factory = DbProviderFactories.GetFactory("System.Data.Odbc");

// DSN-based connection string
var connectionString = "DSN=MyDataSource;UID=user;PWD=password";
// OR driver-based connection string
// var connectionString = "Driver={PostgreSQL Unicode};Server=localhost;Port=5432;Database=mydb;Uid=user;Pwd=password;";

var database = new Database(factory, connectionString);
var executor = new DbCommandExecutor(database, new DefaultMapError());

// Use fluent API with named parameters - ODBC supports @ prefix
executor.ExecuteNonQuery(
    "INSERT INTO Users (Username, Email, Age) VALUES (@Username, @Email, @Age)",
    cmd => cmd
        .WithInputParameter("Username", DbType.String, 50, "john")
        .WithInputParameter("Email", DbType.String, 100, "john@example.com")
        .WithInputParameter("Age", DbType.Int32, 25)
)
.Tap(rows => Console.WriteLine($"{rows} row(s) inserted"))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));

// Reading data
executor.ExecuteReader(
    "SELECT Username, Email FROM Users WHERE Age > @MinAge",
    cmd => cmd.WithInputParameter("MinAge", DbType.Int32, 18),
    new UserListConsumer()
)
.Tap(users => users.ForEach(u => Console.WriteLine($"{u.Username}: {u.Email}")))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));
```

### ODBC Parameter Handling

System.Data.Odbc in .NET supports **named parameters** using the `@` prefix, even though native ODBC uses positional `?` parameters. The .NET provider automatically converts named parameters to the correct format.

**Key points:**
- ✅ Use `@` prefix for parameter names (handled automatically by fluent API)
- ✅ Named parameters work with all ODBC drivers
- ✅ Parameter order must match the order in SQL for some drivers
- ✅ Stored procedures are supported
- ⚠️ Requires ODBC driver installation for target database

### Supported Databases via ODBC

- **PostgreSQL** - PostgreSQL ODBC Driver (psqlODBC)
- **MySQL** - MySQL Connector/ODBC
- **Oracle** - Oracle ODBC Driver
- **SQL Server** - SQL Server Native Client / ODBC Driver
- **IBM DB2** - IBM DB2 ODBC Driver
- **SAP HANA** - SAP HANA ODBC Driver
- **Teradata** - Teradata ODBC Driver
- **Legacy systems** - Any database with ODBC driver support

### Cross-Platform ODBC

ODBC works on all platforms:
- **Windows**: Windows ODBC Driver Manager
- **Linux/macOS**: unixODBC driver manager

## MS SQL Provider

The NuGet package `Voyager.DBConnection.MsSql` provides a ready-to-use implementation for MS SQL Server connections:

```csharp
namespace Voyager.DBConnection.MsSql
{
    public class SqlConnection : Connection
    {
        public SqlConnection(string sqlConnectionString)
            : base(new SqlDatabase(sqlConnectionString), new ExceptionFactory())
        {
        }
    }
}
```

## Credits
- [@andrzejswistowski](https://github.com/AndrzejSwistowski)