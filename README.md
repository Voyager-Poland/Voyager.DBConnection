

# Voyager.DBConnection


**Why use this library?**

- **Unified API for all ADO.NET providers**: Write code once, use with SQL Server, PostgreSQL, Oracle, MySQL, SQLite, and more.
- **Result-based error handling**: Modern, functional Result<T> pattern (no nulls, no exceptions for business logic) for robust, predictable code.
- **Fluent, type-safe parameters**: Add parameters with clear, discoverable extension methods—no more stringly-typed or error-prone code.
- **Async and sync parity**: Every operation is available in both synchronous and asynchronous versions, with identical API.
- **Testability**: Easy to mock and test commands without a real database (see: test doubles in the repository).
- **Extensibility**: Add your own features and subscribe to SQL events without modifying business code.
- **Transaction management**: Simple, safe transaction model with automatic command enlistment.
- **No vendor lock-in**: Change provider or database—no need to change your application code.
- **Organizational standards**: Enforces Result<T>, no nulls, XML-docs, and clean separation of responsibilities (SOLID).

---

Provider-agnostic ADO.NET wrapper over `DbProviderFactory` for building and executing SQL commands. The preferred execution style is Result-based via `DbCommandExecutor`.

## Why this library
- One abstraction across providers: pass your `DbProviderFactory` and connection string.
- Two calling styles when executing commands:
  - Preferred: `DbCommandExecutor` returning `Voyager.Common.Results.Result<T>`.
  - Legacy: `Connection` that throws exceptions (kept for interop).

Targets: net48, net6.0, net8.0 (see [Directory.Build.props](src/Voyager.DBConnection/../Directory.Build.props)).


## Quickstart: Step-by-step Examples

### 1. Call a stored procedure without parameters
```csharp
// Define a command factory for a procedure with no parameters
internal sealed class SimpleProc : IDbCommandFactory
{
	public DbCommand ConstructDbCommand(IDatabase db)
		=> db.GetStoredProcCommand("MySimpleProcedure");
}

// Execute (sync)
var result = executor.ExecuteNonQuery(new SimpleProc());
result.Switch(
	onSuccess: rows => Console.WriteLine($"Affected: {rows}"),
	onFailure: err => Console.WriteLine(err.Message)
);
```

### 2. Add parameters to a command
```csharp
internal sealed class WithParams : IDbCommandFactory
{
	public DbCommand ConstructDbCommand(IDatabase db)
	{
		var cmd = db.GetStoredProcCommand("MyProcWithParams");
		cmd.WithInputParameter("UserId", DbType.Int32, 123)
		   .WithInputParameter("Name", DbType.String, "Alice");
		return cmd;
	}
}

var result = executor.ExecuteNonQuery(new WithParams());
```

### 3. Read a scalar result
```csharp
internal sealed class GetCount : IDbCommandFactory
{
	public DbCommand ConstructDbCommand(IDatabase db)
	{
		var cmd = db.GetStoredProcCommand("GetUserCount");
		return cmd;
	}
}

var result = executor.ExecuteScalar(new GetCount());
result.Match(
	onSuccess: count => Console.WriteLine($"User count: {count}"),
	onFailure: err => Console.WriteLine(err.Message)
);
```

### 4. Read results with a data reader
```csharp
internal sealed class GetUsers : IDbCommandFactory
{
	public DbCommand ConstructDbCommand(IDatabase db)
		=> db.GetStoredProcCommand("GetAllUsers");
}

internal sealed class UserConsumer : IGetConsumer<List<User>>
{
	public List<User> GetResults(IDataReader dr)
	{
		var users = new List<User>();
		while (dr.Read())
		{
			users.Add(new User
			{
				Id = dr.GetInt32(0),
				Name = dr.GetString(1)
			});
		}
		return users;
	}
}

var result = executor.ExecuteReader(new GetUsers(), new UserConsumer());
result.Match(
	onSuccess: users => Console.WriteLine($"Users: {users.Count}"),
	onFailure: err => Console.WriteLine(err.Message)
);
```

// All overloads are available for ExecuteNonQuery, ExecuteScalar, ExecuteAndBind, ExecuteReader (sync and async):
// - By IDbCommandFactory
executor.ExecuteNonQuery(factory);
executor.ExecuteScalar(factory);
executor.ExecuteReader(factory, consumer);
// - By Func<IDatabase, DbCommand>
executor.ExecuteNonQuery(db => ...);
executor.ExecuteScalar(db => ...);
executor.ExecuteReader(db => ..., consumer);
// - By procedure name + parameter action
executor.ExecuteNonQuery("procName", cmd => { /* add params */ });
executor.ExecuteScalar("procName", cmd => { /* add params */ });
executor.ExecuteReader("procName", cmd => { /* add params */ }, consumer);

// Async variants (all overloads):
await executor.ExecuteNonQueryAsync(factory);
await executor.ExecuteNonQueryAsync(db => ...);
await executor.ExecuteNonQueryAsync("procName", cmd => { /* add params */ });
await executor.ExecuteScalarAsync(factory);
await executor.ExecuteScalarAsync(db => ...);
await executor.ExecuteScalarAsync("procName", cmd => { /* add params */ });
await executor.ExecuteReaderAsync(factory, consumer);
await executor.ExecuteReaderAsync(db => ..., consumer);
await executor.ExecuteReaderAsync("procName", cmd => { /* add params */ }, consumer);
```


			## Reading data (Result pattern)
			`DbCommandExecutor.ExecuteReader` and `ExecuteReaderAsync` take an `IDbCommandFactory`, a `Func<IDatabase, DbCommand>`, or a procedure name + parameter action, and an `IGetConsumer<T>` that projects from `IDataReader` to your domain type.

			```csharp
			internal sealed class RegionalSaleCommand : IDbCommandFactory
			{
				private readonly Request req;
				public RegionalSaleCommand(Request req) { this.req = req; }
				public DbCommand ConstructDbCommand(IDatabase db)
				{
					var cmd = db.GetStoredProcCommand("[dbo].[TestSaleReport]");
					cmd.WithInputParameter("IdAkwizytorRowNo", DbType.Int32, req.IdAkwizytorRowNo)
					   .WithInputParameter("IdPrzewoznikRowNo", DbType.Int32, req.IdPrzewoznikRowNo)
					   .WithInputParameter("DataPocz", DbType.DateTime, req.DateFrom)
					   .WithInputParameter("DataKon", DbType.DateTime, req.DateTo);
					return cmd;
				}
			}

			internal sealed class RegionalSaleConsumer : IGetConsumer<SaleItem[]>
			{
				public SaleItem[] GetResults(IDataReader dr)
				{
					var items = new List<SaleItem>();
					while (dr.Read())
					{
						// Map columns → SaleItem (example only)
						items.Add(new SaleItem { /* ... */ });
					}
					return items.ToArray();
				}
			}

			// Synchronous
			var result = executor.ExecuteReader(new RegionalSaleCommand(request), new RegionalSaleConsumer());
			result.Match(
				onSuccess: items => Use(items),
				onFailure: err => Log(err)
			);

			// Asynchronous
			var resultAsync = await executor.ExecuteReaderAsync(new RegionalSaleCommand(request), new RegionalSaleConsumer());
			resultAsync.Match(
				onSuccess: items => Use(items),
				onFailure: err => Log(err)
			);
			```

			## Fluent parameters and provider prefixes
			Use extension methods from [src/Voyager.DBConnection/Extensions/DbCommandExtensions.cs](src/Voyager.DBConnection/Extensions/DbCommandExtensions.cs):
			- `WithInputParameter(name, dbType, value)` / overloads with `size`.
			- `WithOutputParameter(name, dbType, size)` and `WithInputOutputParameter(...)`.
			- `GetParameterValue<T>(name)` to read outputs.

			Pass parameter names without `@`/`:`. The extension infers the correct prefix from the command type (e.g., `@` for SQL Server/Postgres/MySQL/SQLite; `:` for Oracle).

			## Transactions, events, features
			- Begin/commit with `using var tx = executor.BeginTransaction();` and your subsequent commands will enlist.
			- Subscribe to SQL call events via `IRegisterEvents.AddEvent(Action<SqlCallEvent>)`.
			- Attach cross‑cutting behavior with `AddFeature(IFeature)`.

			## Legacy exception style
			`Connection` exposes `ExecuteNonQuery`, `ExecuteScalar`, and `GetReader` that throw mapped exceptions via `IExceptionPolicy`. Prefer `DbCommandExecutor` for new code.

			## Packages and CI
			- NuGet package includes this README and targets multiple TFMs. Versioning is driven by MinVer tags (prefix `v`) — see [build/Build.Versioning.props](build/Build.Versioning.props).
			- CI builds Release, tests net6.0/net8.0, then packs/publishes on main/tags — see [.github/workflows/ci.yml](.github/workflows/ci.yml).

			## Credits
			- [@andrzejswistowski](https://github.com/AndrzejSwistowski)