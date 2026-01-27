# DataReaderExtensions - User Guide

## Overview

`DataReaderExtensions` provides a set of LINQ-like extension methods for `IDataReader` that simplify data extraction and object mapping. These methods eliminate boilerplate code and provide familiar patterns for working with database results.

## Installation

The extensions are included in the `Voyager.DBConnection` package. No additional installation required.

```bash
dotnet add package Voyager.DBConnection
```

## Quick Start

```csharp
using Voyager.DBConnection;

// Simple usage with DbCommandExecutor
var users = executor.ExecuteReader(
    db => db.GetSqlCommand("SELECT UserId, Username, Email FROM Users"),
    reader => reader.FetchAll(r => new User
    {
        Id = r.GetInt32(r.GetOrdinal("UserId")),
        Name = r.GetString(r.GetOrdinal("Username")),
        Email = r.GetString(r.GetOrdinal("Email"))
    })
);
```

## Available Methods

### Summary Table

| Method | Return Type | When to Use | Throws on No Rows | Throws on >1 Row |
|--------|-------------|-------------|-------------------|------------------|
| `AsEnumerable<T>()` | `IEnumerable<T>` | Use LINQ operations on reader | ❌ No | ❌ No |
| `FetchAll<T>()` | `T[]` | Get all rows as array | ❌ No (returns empty array) | ❌ No |
| `FetchList<T>()` | `List<T>` | Get all rows as modifiable list | ❌ No (returns empty list) | ❌ No |
| `FetchFirstOrDefault<T>()` | `T` | Get first row or default | ❌ No (returns default) | ❌ No |
| `FetchFirst<T>()` | `T` | Get first row (must exist) | ✅ Yes | ❌ No |
| `FetchSingleOrDefault<T>()` | `T` | Get 0-1 row (enforce uniqueness) | ❌ No (returns default) | ✅ Yes |
| `FetchSingle<T>()` | `T` | Get exactly 1 row | ✅ Yes | ✅ Yes |

---

## Detailed Documentation

### 1. AsEnumerable\<T\>()

**Converts IDataReader into an enumerable sequence for LINQ operations.**

```csharp
public static IEnumerable<TValue> AsEnumerable<TValue>(
    this IDataReader dataReader, 
    Func<IDataReader, TValue> createItem)
```

#### When to Use
- You want to use LINQ operations (Where, Select, Take, Skip, etc.)
- You need lazy evaluation (rows read on-demand)
- You want to filter or transform data while reading

#### Important Notes
⚠️ **Lazy Evaluation**: Rows are read on-demand as you iterate  
⚠️ **Some LINQ operations enumerate fully**: `Count()`, `OrderBy()`, `ToArray()`, `ToList()`  
⚠️ **No Reset**: `IDataReader` doesn't support rewinding  

#### Examples

**Basic LINQ usage:**
```csharp
var users = executor.ExecuteReader(
    db => db.GetSqlCommand("SELECT UserId, Username, Email, IsActive FROM Users"),
    reader => reader
        .AsEnumerable(r => new User
        {
            Id = r.GetInt32(r.GetOrdinal("UserId")),
            Name = r.GetString(r.GetOrdinal("Username")),
            Email = r.GetString(r.GetOrdinal("Email")),
            IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
        })
        .Where(u => u.IsActive)
        .Take(10)
        .ToList()
);
```

**Filtering with multiple conditions:**
```csharp
var premiumUsers = executor.ExecuteReader(
    "GetAllUsers",
    reader => reader
        .AsEnumerable(r => new User
        {
            Id = r.GetInt32(0),
            Name = r.GetString(1),
            Email = r.GetString(2),
            SubscriptionLevel = r.GetString(3)
        })
        .Where(u => u.SubscriptionLevel == "Premium")
        .Where(u => u.Email.EndsWith("@company.com"))
        .OrderBy(u => u.Name)
        .ToArray()
);
```

**Projection (Select):**
```csharp
var userNames = executor.ExecuteReader(
    db => db.GetSqlCommand("SELECT UserId, Username FROM Users"),
    reader => reader
        .AsEnumerable(r => new { Id = r.GetInt32(0), Name = r.GetString(1) })
        .Select(u => u.Name.ToUpper())
        .ToList()
);
```

**Pagination (Skip + Take):**
```csharp
int page = 2;
int pageSize = 20;

var users = executor.ExecuteReader(
    db => db.GetSqlCommand("SELECT * FROM Users ORDER BY UserId"),
    reader => reader
        .AsEnumerable(r => new User { Id = r.GetInt32(0), Name = r.GetString(1) })
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList()
);
```

**Grouping (client-side):**
```csharp
var usersByDomain = executor.ExecuteReader(
    "GetUsers",
    reader => reader
        .AsEnumerable(r => new User
        {
            Id = r.GetInt32(0),
            Email = r.GetString(1)
        })
        .GroupBy(u => u.Email.Split('@')[1])
        .ToDictionary(g => g.Key, g => g.ToList())
);
```

**Combined with Any/All:**
```csharp
bool hasActiveAdmins = executor.ExecuteReader(
    db => db.GetSqlCommand("SELECT * FROM Users WHERE Role = 'Admin'"),
    reader => reader
        .AsEnumerable(r => new User
        {
            Id = r.GetInt32(0),
            IsActive = r.GetBoolean(1)
        })
        .Any(u => u.IsActive)
);
```

---

### 2. FetchAll\<T\>()

**Reads all rows and returns an array.**

```csharp
public static TValue[] FetchAll<TValue>(
    this IDataReader dataReader, 
    Func<IDataReader, TValue> createItem)
```

#### When to Use
- You need all rows from a query
- You want an immutable collection (array)
- Performance is critical (arrays are faster than lists)

#### Examples

**Basic usage:**
```csharp
var products = executor.ExecuteReader(
    "GetProducts",
    reader => reader.FetchAll(r => new Product
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        Price = r.GetDecimal(2)
    })
);

Console.WriteLine($"Found {products.Length} products");
```

**With named columns:**
```csharp
var users = executor.ExecuteReader(
    db => db.GetSqlCommand("SELECT UserId, Username, Email FROM Users WHERE IsActive = 1"),
    reader => reader.FetchAll(r => new User
    {
        Id = r.GetInt32(r.GetOrdinal("UserId")),
        Name = r.GetString(r.GetOrdinal("Username")),
        Email = r.GetString(r.GetOrdinal("Email"))
    })
);
```

**Empty result handling:**
```csharp
var users = reader.FetchAll(r => new User { ... });
// Returns empty array if no rows (never null)
if (users.Length == 0)
{
    Console.WriteLine("No users found");
}
```

---

### 2. FetchList\<T\>()

**Reads all rows and returns a mutable list.**

```csharp
public static List<TValue> FetchList<TValue>(
    this IDataReader dataReader, 
    Func<IDataReader, TValue> createItem)
```

#### When to Use
- You need to modify the collection after retrieval
- You want to add/remove/sort items
- You prefer List over array

#### Examples

**Basic usage:**
```csharp
var users = executor.ExecuteReader(
    "GetUsers",
    reader => reader.FetchList(r => new User
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1)
    })
);

// List allows modification
users.Add(new User { Id = 0, Name = "Guest" });
users.RemoveAll(u => u.Name.StartsWith("Test"));
users.Sort((a, b) => a.Name.CompareTo(b.Name));
```

**Post-processing:**
```csharp
var activeUsers = executor.ExecuteReader(
    "GetAllUsers",
    reader => reader.FetchList(r => new User
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        IsActive = r.GetBoolean(2)
    })
);

// Filter in memory if needed
var inactiveUsers = activeUsers.Where(u => !u.IsActive).ToList();
```

---

### 3. FetchFirstOrDefault\<T\>()

**Returns the first row or default value if no rows.**

```csharp
public static TValue FetchFirstOrDefault<TValue>(
    this IDataReader dataReader, 
    Func<IDataReader, TValue> createItem)
```

#### When to Use
- Optional lookups (may or may not exist)
- "Get latest" scenarios
- Safe retrievals where missing data is acceptable

#### Examples

**Optional configuration:**
```csharp
var setting = executor.ExecuteReader(
    "GetConfigValue",
    cmd => cmd.WithInputParameter("key", DbType.String, "MaxRetries"),
    reader => reader.FetchFirstOrDefault(r => r.GetString(0))
);

var maxRetries = int.Parse(setting ?? "3"); // Use default if not found
```

**Latest record:**
```csharp
var lastOrder = executor.ExecuteReader(
    db => db.GetSqlCommand(@"
        SELECT TOP 1 OrderId, OrderDate, TotalAmount 
        FROM Orders 
        ORDER BY OrderDate DESC"),
    reader => reader.FetchFirstOrDefault(r => new Order
    {
        Id = r.GetInt32(0),
        Date = r.GetDateTime(1),
        Amount = r.GetDecimal(2)
    })
);

if (lastOrder != null)
{
    Console.WriteLine($"Last order: {lastOrder.Id} on {lastOrder.Date}");
}
```

**Value types return default (0, false, etc.):**
```csharp
var count = executor.ExecuteReader(
    db => db.GetSqlCommand("SELECT COUNT(*) FROM NonExistentTable"),
    reader => reader.FetchFirstOrDefault(r => r.GetInt32(0))
);
// Returns 0 if no rows (not null!)
```

---

### 4. FetchFirst\<T\>()

**Returns the first row. Throws if no rows.**

```csharp
public static TValue FetchFirst<TValue>(
    this IDataReader dataReader, 
    Func<IDataReader, TValue> createItem)
```

#### When to Use
- You **expect** at least one row
- Missing data is an error condition
- Fast-fail behavior is desired

#### Examples

**Required lookup:**
```csharp
try
{
    var currentUser = executor.ExecuteReader(
        "GetCurrentUser",
        reader => reader.FetchFirst(r => new User
        {
            Id = r.GetInt32(r.GetOrdinal("UserId")),
            Name = r.GetString(r.GetOrdinal("Username"))
        })
    ).Value; // .Value because ExecuteReader returns Result<T>

    Console.WriteLine($"Current user: {currentUser.Name}");
}
catch (InvalidOperationException ex)
{
    // Handle missing user
    Console.WriteLine("No current user found!");
}
```

**With Result pattern:**
```csharp
var result = executor.ExecuteReader(
    "GetSystemConfig",
    reader => reader.FetchFirst(r => new Config { ... })
);

result
    .Tap(config => Console.WriteLine($"Config loaded: {config.Version}"))
    .TapError(error => Console.WriteLine($"Failed to load config: {error.Message}"));
```

---

### 5. FetchSingle\<T\>()

**Returns exactly one row. Throws if 0 or >1 rows.**

```csharp
public static TValue FetchSingle<TValue>(
    this IDataReader dataReader, 
    Func<IDataReader, TValue> createItem)
```

#### When to Use
- Primary key lookups
- Unique constraint queries
- Data integrity validation
- You **expect and require** exactly one row

#### Examples

**Primary key lookup:**
```csharp
var user = executor.ExecuteReader(
    "GetUserById",
    cmd => cmd.WithInputParameter("userId", DbType.Int32, 42),
    reader => reader.FetchSingle(r => new User
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        Email = r.GetString(2)
    })
);
// Throws if user doesn't exist OR if duplicate IDs exist (data corruption!)
```

**Aggregate query:**
```csharp
var totalSales = executor.ExecuteReader(
    db => db.GetSqlCommand(@"
        SELECT SUM(Amount) 
        FROM Sales 
        WHERE Year = @year"),
    cmd => cmd.WithInputParameter("year", DbType.Int32, 2024),
    reader => reader.FetchSingle(r => r.GetDecimal(0))
);
// Aggregate queries always return exactly one row
```

**Error handling:**
```csharp
try
{
    var user = reader.FetchSingle(r => new User { ... });
}
catch (InvalidOperationException ex)
{
    if (ex.Message.Contains("no rows"))
    {
        // Handle missing record
    }
    else if (ex.Message.Contains("more than one"))
    {
        // Handle data integrity issue!
        logger.Error("Duplicate primary key detected!");
    }
}
```

---

### 6. FetchSingleOrDefault\<T\>()

**Returns 0-1 row. Throws if >1 row.**

```csharp
public static TValue FetchSingleOrDefault<TValue>(
    this IDataReader dataReader, 
    Func<IDataReader, TValue> createItem)
```

#### When to Use
- Optional unique lookups
- "Get current" scenarios
- Enforce uniqueness but allow missing data

#### Examples

**Optional user preference:**
```csharp
var theme = executor.ExecuteReader(
    "GetUserPreference",
    cmd => cmd
        .WithInputParameter("userId", DbType.Int32, currentUserId)
        .WithInputParameter("key", DbType.String, "Theme"),
    reader => reader.FetchSingleOrDefault(r => r.GetString(0))
);

var userTheme = theme ?? "Dark"; // Use default if preference not set
// Throws InvalidOperationException if user has duplicate preferences (data corruption)
```

**Current session:**
```csharp
var session = executor.ExecuteReader(
    db => db.GetSqlCommand(@"
        SELECT SessionId, Token, ExpiresAt 
        FROM UserSessions 
        WHERE UserId = @userId AND IsActive = 1"),
    cmd => cmd.WithInputParameter("userId", DbType.Int32, userId),
    reader => reader.FetchSingleOrDefault(r => new Session
    {
        Id = r.GetInt32(0),
        Token = r.GetString(1),
        ExpiresAt = r.GetDateTime(2)
    })
);

if (session == null)
{
    // Create new session
}
else
{
    // Use existing session
}
```

---

## Usage Patterns

### With DbCommandExecutor (Recommended)

```csharp
var executor = new DbCommandExecutor(database);

// Pattern 1: Lambda with IResultsConsumer
var users = executor.ExecuteReader(
    db => db.GetSqlCommand("SELECT * FROM Users"),
    reader => reader.FetchAll(r => new User { ... })
);

// Pattern 2: Stored procedure
var user = executor.ExecuteReader(
    "GetUserById",
    cmd => cmd.WithInputParameter("userId", DbType.Int32, 42),
    reader => reader.FetchFirst(r => new User { ... })
);

// Pattern 3: With IDbCommandFactory
public class GetUsersCommand : IDbCommandFactory
{
    public DbCommand ConstructDbCommand(IDatabase database)
    {
        return database.GetSqlCommand("SELECT * FROM Users");
    }
}

var users = executor.ExecuteReader(
    new GetUsersCommand(),
    reader => reader.FetchAll(r => new User { ... })
);
```

### With Connection (Legacy)

```csharp
var connection = new Connection(database);

// Using IResultsConsumer
public class UserListConsumer : IResultsConsumer<User[]>
{
    public User[] GetResults(IDataReader reader)
    {
        return reader.FetchAll(r => new User
        {
            Id = r.GetInt32(0),
            Name = r.GetString(1)
        });
    }
}

var users = connection.GetReader(
    new GetUsersCommand(),
    new UserListConsumer()
);
```

### Direct IDataReader Usage

```csharp
// If you have direct access to IDataReader
using (var reader = command.ExecuteReader())
{
    var users = reader.FetchAll(r => new User { ... });
    
    // Multiple result sets
    var products = reader.NextResult() 
        ? reader.FetchAll(r => new Product { ... }) 
        : Array.Empty<Product>();
}
```

---

## Best Practices

### ✅ DO

**Use AsEnumerable for LINQ operations:**
```csharp
// ✅ Good - lazy evaluation, only reads needed rows
var activeUsers = reader
    .AsEnumerable(r => new User { ... })
    .Where(u => u.IsActive)
    .Take(10)
    .ToList();

// ✅ Also good - if you need all rows anyway
var allUsers = reader.FetchAll(r => new User { ... });
```

**Use GetOrdinal for column names:**
```csharp
reader.FetchAll(r => new User
{
    Id = r.GetInt32(r.GetOrdinal("UserId")),
    Name = r.GetString(r.GetOrdinal("Username"))
});
```

**Choose the right method:**
```csharp
// Multiple rows → FetchAll or FetchList
var allUsers = reader.FetchAll(r => new User { ... });

// Optional single row → FetchFirstOrDefault or FetchSingleOrDefault
var config = reader.FetchFirstOrDefault(r => new Config { ... });

// Required single row → FetchFirst or FetchSingle
var user = reader.FetchSingle(r => new User { ... });
```

**Handle DBNull:**
```csharp
reader.FetchAll(r => new User
{
    Id = r.GetInt32(0),
    Email = r.IsDBNull(2) ? null : r.GetString(2),
    Age = r.IsDBNull(3) ? (int?)null : r.GetInt32(3)
});
```

### ❌ DON'T

**Don't ignore exceptions:**
```csharp
// ❌ Bad
try
{
    var user = reader.FetchSingle(r => new User { ... });
}
catch { } // Silent failure!

// ✅ Good
var user = reader.FetchSingleOrDefault(r => new User { ... });
if (user == null)
{
    logger.Warning("User not found");
}
```

**Don't use FetchSingle when you mean FetchFirst:**
```csharp
// ❌ Bad - throws if query returns 10 rows!
var firstUser = reader.FetchSingle(r => new User { ... });

// ✅ Good
var firstUser = reader.FetchFirst(r => new User { ... });
```

**Don't mix reader methods with extensions:**
```csharp
// ❌ Bad - confusing
if (reader.Read())
{
    var firstUser = CreateUser(reader);
}
var otherUsers = reader.FetchAll(r => CreateUser(r)); // Wrong!

// ✅ Good - consistent
var allUsers = reader.FetchAll(r => CreateUser(r));
var firstUser = allUsers.FirstOrDefault();
```

---

## Performance Considerations

### Memory Usage

```csharp
// Arrays are more memory efficient
var users = reader.FetchAll(r => new User { ... }); // Better for read-only

// Lists have overhead but allow growth
var users = reader.FetchList(r => new User { ... }); // Better for modification

// AsEnumerable is lazy - best for filtering/limiting
var users = reader
    .AsEnumerable(r => new User { ... })
    .Where(u => u.IsActive)
    .Take(100) // Only reads 100 rows
    .ToList();
```

### Large Result Sets

```csharp
// ❌ Bad - loads ALL data into memory first, then filters
var activeUsers = reader.FetchAll(r => new User { ... })
    .Where(u => u.IsActive)
    .ToList();

// ✅ Good - filters while reading (lazy evaluation)
var activeUsers = reader
    .AsEnumerable(r => new User { ... })
    .Where(u => u.IsActive)
    .ToList();

// ✅ Best - let database do the filtering
var activeUsers = executor.ExecuteReader(
    db => db.GetSqlCommand("SELECT * FROM Users WHERE IsActive = 1"),
    reader => reader.FetchAll(r => new User { ... })
);

// For VERY large datasets, stream processing
while (reader.Read())
{
    var user = new User { Id = reader.GetInt32(0), ... };
    ProcessUser(user); // Stream processing - minimal memory
}
```

### LINQ Operations Performance

```csharp
// ⚠️ These LINQ operations enumerate the ENTIRE sequence:
reader.AsEnumerable(r => new User { ... }).Count();      // Reads all rows
reader.AsEnumerable(r => new User { ... }).OrderBy(...); // Reads all rows
reader.AsEnumerable(r => new User { ... }).ToArray();    // Reads all rows
reader.AsEnumerable(r => new User { ... }).ToList();     // Reads all rows

// ✅ These LINQ operations can short-circuit (lazy):
reader.AsEnumerable(r => new User { ... }).Take(10);     // Stops after 10 rows
reader.AsEnumerable(r => new User { ... }).First();      // Stops after 1 row
reader.AsEnumerable(r => new User { ... }).Any();        // Stops on first match
reader.AsEnumerable(r => new User { ... }).FirstOrDefault(); // Stops after 1 row
```

---

## Exception Handling

### Common Exceptions

| Exception | Thrown By | Reason |
|-----------|-----------|--------|
| `ArgumentNullException` | All methods | `dataReader` or `createItem` is null |
| `InvalidOperationException` | `FetchFirst()` | No rows in result set |
| `InvalidOperationException` | `FetchSingle()` | No rows OR more than one row |
| `InvalidOperationException` | `FetchSingleOrDefault()` | More than one row |

### Handling Examples

```csharp
// Pattern 1: Try-catch
try
{
    var user = reader.FetchSingle(r => new User { ... });
    return Result.Ok(user);
}
catch (InvalidOperationException ex)
{
    return Result.Fail<User>(ex.Message);
}

// Pattern 2: OrDefault methods
var user = reader.FetchSingleOrDefault(r => new User { ... });
return user != null 
    ? Result.Ok(user) 
    : Result.Fail<User>("User not found");
```

---

## Testing

See `DataReaderExtensionsTests.cs` for complete test examples.

**Basic test pattern:**
```csharp
[Test]
public void FetchAll_WithMultipleRows_ShouldReturnArray()
{
    // Arrange
    var mockReader = new Mock<IDataReader>();
    var readSequence = new Queue<bool>(new[] { true, true, true, false });
    mockReader.Setup(r => r.Read()).Returns(() => readSequence.Dequeue());
    mockReader.Setup(r => r.GetInt32(0)).Returns(1);

    // Act
    var result = mockReader.Object.FetchAll(r => new User 
    { 
        Id = r.GetInt32(0) 
    });

    // Assert
    Assert.That(result.Length, Is.EqualTo(3));
}
```

---

## Migration Guide

### From Manual Loops

**Before:**
```csharp
var users = new List<User>();
while (reader.Read())
{
    users.Add(new User
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1)
    });
}
return users.ToArray();
```

**After:**
```csharp
return reader.FetchAll(r => new User
{
    Id = r.GetInt32(0),
    Name = r.GetString(1)
});
```

### From IResultsConsumer

**Before:**
```csharp
public class UserListConsumer : IResultsConsumer<User[]>
{
    public User[] GetResults(IDataReader reader)
    {
        var users = new List<User>();
        while (reader.Read())
        {
            users.Add(new User { ... });
        }
        return users.ToArray();
    }
}
```

**After:**
```csharp
public class UserListConsumer : IResultsConsumer<User[]>
{
    public User[] GetResults(IDataReader reader)
    {
        return reader.FetchAll(r => new User { ... });
    }
}
```

---

## FAQ

**Q: When should I use AsEnumerable vs FetchAll?**  
A: Use `AsEnumerable` when you want LINQ operations or need lazy evaluation. Use `FetchAll` when you need all rows as an array. If you're going to filter/limit, `AsEnumerable` is more efficient.

**Q: Why FetchAll instead of ToArray()?**  
A: `FetchAll` is specific to `IDataReader` and provides better error messages and null safety.

**Q: Can I use async versions?**  
A: These methods are synchronous. Use them within `ExecuteReaderAsync` callbacks, which handles async execution.

**Q: What about multiple result sets?**  
A: Call `reader.NextResult()` between calls:
```csharp
var users = reader.FetchAll(r => new User { ... });
reader.NextResult();
var products = reader.FetchAll(r => new Product { ... });
```

**Q: Are these methods thread-safe?**  
A: No, `IDataReader` itself is not thread-safe. Use one reader per thread.

**Q: Can I reuse AsEnumerable results?**  
A: No, `IDataReader` doesn't support rewinding. Enumerate once and store results if needed:
```csharp
// ❌ Bad - will fail on second enumeration
var enumerable = reader.AsEnumerable(r => new User { ... });
var list1 = enumerable.ToList(); // Works
var list2 = enumerable.ToList(); // Fails! Reader already consumed

// ✅ Good - materialize once
var users = reader.AsEnumerable(r => new User { ... }).ToList();
var list1 = users.Where(u => u.IsActive).ToList(); // Works
var list2 = users.Where(u => !u.IsActive).ToList(); // Works
```

---

## Related Documentation

- [DbCommandExecutor Guide](./DbCommandExecutor.md)
- [IResultsConsumer Pattern](./IResultsConsumer.md)
- [Result Pattern](./ResultPattern.md)

---

## Changelog

### Version 4.5.0
- ✨ Added `DataReaderExtensions` with 7 methods:
  - `AsEnumerable<T>()` - LINQ support for IDataReader
  - `FetchAll<T>()` - Get all rows as array
  - `FetchList<T>()` - Get all rows as List
  - `FetchFirst<T>()` / `FetchFirstOrDefault<T>()` - Get first row
  - `FetchSingle<T>()` / `FetchSingleOrDefault<T>()` - Get exactly one row
- 📝 Complete XML documentation with examples
- ✅ 24 unit tests with Moq
- 🔧 Internal `DataReaderEnumerable<T>` and `DataReaderEnumerator<T>` for LINQ support

---

## Support

For issues or questions:
- GitHub Issues: [Voyager.DBConnection/issues](https://github.com/Voyager-Poland/Voyager.DBConnection/issues)
- See also: [API Documentation](./API.md)
