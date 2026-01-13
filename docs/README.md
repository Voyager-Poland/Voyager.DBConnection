# Voyager.DBConnection Documentation

## Quick Links

- **[DataReaderExtensions Guide](./DataReaderExtensions.md)** - LINQ-like extension methods for IDataReader
- [API Reference](#) - Complete API documentation (coming soon)
- [Migration Guide](#) - Upgrading from older versions (coming soon)

## Extension Methods

### DataReaderExtensions

Simplify data extraction from `IDataReader` with LINQ-like methods:

```csharp
// Get all rows
var users = reader.FetchAll(r => new User 
{
    Id = r.GetInt32(0),
    Name = r.GetString(1)
});

// Use LINQ operations (NEW!)
var activeUsers = reader
    .AsEnumerable(r => new User { ... })
    .Where(u => u.IsActive)
    .Take(10)
    .ToList();
```

**Available Methods:**
- `AsEnumerable<T>()` - Enable LINQ operations (lazy evaluation)
- `FetchAll<T>()` - Get all rows as array
- `FetchList<T>()` - Get all rows as List
- `FetchFirst<T>()` - Get first row (throws if none)
- `FetchFirstOrDefault<T>()` - Get first row or default
- `FetchSingle<T>()` - Get exactly one row (throws if 0 or >1)
- `FetchSingleOrDefault<T>()` - Get 0-1 row (throws if >1)

📖 [Read full guide](./DataReaderExtensions.md)

---

## Getting Started

1. Install the package:
   ```bash
   dotnet add package Voyager.DBConnection
   ```

2. Use extension methods:
   ```csharp
   using Voyager.DBConnection;
   
   var executor = new DbCommandExecutor(database);
   var users = executor.ExecuteReader(
       db => db.GetSqlCommand("SELECT * FROM Users"),
       reader => reader.FetchAll(r => new User { ... })
   );
   ```

---

## Documentation Index

- **Guides**
  - [DataReaderExtensions](./DataReaderExtensions.md) ✅
  - DbCommandExecutor (coming soon)
  - Connection (coming soon)
  - Transaction Handling (coming soon)

- **Patterns**
  - Result Pattern (coming soon)
  - IResultsConsumer (coming soon)
  - ICommandFactory (coming soon)

- **Integration**
  - SQL Server (coming soon)
  - PostgreSQL (coming soon)
  - MySQL (coming soon)
  - Oracle (coming soon)

---

## Contributing

Documentation contributions are welcome! Please:
1. Use Markdown format
2. Include code examples
3. Follow existing structure
4. Test code examples

---

## Support

- Issues: [GitHub Issues](https://github.com/Voyager-Poland/Voyager.DBConnection/issues)
- Discussions: [GitHub Discussions](https://github.com/Voyager-Poland/Voyager.DBConnection/discussions)
