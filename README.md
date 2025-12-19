

# Voyager.DBConnection

## Overview

Voyager.DBConnection is a library providing a structured and type-safe way to connect to SQL databases using `DbProviderFactory`. It implements the Command Factory pattern to encapsulate database operations and provides a clean abstraction over ADO.NET.

**Key Features:**
- Type-safe database command construction using the Command Factory pattern
- Support for stored procedures and parameterized queries
- Built-in logging support through extensions
- MS SQL Server provider implementation included
- Clean separation between command construction and execution


### Step 1: Implement ICommandFactory Interface

First, implement the `Voyager.DBConnection.Interfaces.ICommandFactory` interface:

```csharp
public interface ICommandFactory : IReadOutParameters
{
    DbCommand ConstructDbCommand(Database db);
}
```

### Step 2: Create Command Factory Implementation

Here's an example implementation for executing a stored procedure:

```csharp
internal class SetPilotiDoPowiadomieniaFactory : Voyager.DBConnection.Interfaces.ICommandFactory
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

    public void ReadOutParameters(Database db, DbCommand command)
    {
    }
}
```
### Step 3: Execute the Command

Using your `DbProviderFactory`, create your database object (`Voyager.DBConnection.Database`) and connection (`Voyager.DBConnection.Connection`). Then call `ExecuteNonQuery` with the command factory:

```csharp
SetPilotiDoPowiadomieniaFactory factory = new SetPilotiDoPowiadomieniaFactory(
    tagItem.IdBusMapRNp,
    tagItem.BusMapDate,
    tagItem.IdAkwizytor,
    tagItem.Raport
);
con.ExecuteNonQuery(factory);
```

## Reading Data

For reading data, implement the `IGetConsumer` interface:

```csharp
internal class RegionalSaleCommand : Voyager.DBConnection.Interfaces.ICommandFactory, IGetConsumer<SaleItem[]>
{
    private RaportRequest request;

    public RegionalSaleCommand(RaportRequest request)
    {
        this.request = request;
    }

    public DbCommand ConstructDbCommand(Database db)
    {
        var cmd = db.GetStoredProcCommand("[dbo].[TestSaleReport]");
        db.AddInParameter(cmd, "IdAkwizytorRowNo", System.Data.DbType.Int32, request.IdAkwizytorRowNo);
        db.AddInParameter(cmd, "IdPrzewoznikRowNo", System.Data.DbType.Int32, request.IdPrzewoznikRowNo);
        db.AddInParameter(cmd, "DataPocz", System.Data.DbType.DateTime, request.DateFrom);
        db.AddInParameter(cmd, "DataKon", System.Data.DbType.DateTime, request.DateTo);

        return cmd;
    }

    public SaleItem[] GetResults(IDataReader dataReader)
    {
        List<SaleItem> lista = new List<SaleItem>();

        while (dataReader.Read())
        {
            int col = 0;
            SaleItem item = new SaleItem();
            item.GidRezerwacji = dataReader.GetString(col++);
            item.GIDL = dataReader.GetString(col++);

            DateTime data = dataReader.GetDateTime(col++);
            var ts = dataReader.GetValue(col++);
            TimeSpan czas = ts.ToString().CastTimeSpan();

            item.DataSprzedazy = data.AddTicks(czas.Ticks);
            item.IdWaluta = dataReader.GetString(col++);
            item.IdWalutaBazowa = DBSafeCast.CastEmptyString(dataReader.GetValue(col++));
            item.KursDniaBaz = (Double)DBSafeCast.Cast<Decimal>(dataReader.GetValue(col++), 1);
            item.NettoZ = DBSafeCast.Cast<Decimal>(dataReader.GetValue(col++), 0);
            item.WalutaZcennika = dataReader.GetBoolean(col++);

            lista.Add(item);
        }

        return lista.ToArray();
    }

    public void ReadOutParameters(Database db, DbCommand command)
    {
    }
}
```

Then call the `GetReader` method on the connection object:

```csharp
public class RaportDB : Voyager.Raport.DBEntity.Store.Raport
{
    private readonly Connection connection;

    public RaportDB(Voyager.DBConnection.Connection connection)
    {
        this.connection = connection;
    }

    public RaportResponse GetRaport(RaportRequest request)
    {
        RegionalSaleCommand raport = new RegionalSaleCommand(request);
        return new RaportResponse()
        {
            Items = connection.GetReader(raport, raport)
        };
    }
}
```
## Result-Based Error Handling with DbCommandExecutor

The library provides `DbCommandExecutor` class that implements the `IDbCommandExecutor` interface for executing database commands with Result-based error handling instead of throwing exceptions. This approach uses the Result monad pattern to encapsulate either a successful value or an error.

### Key Benefits

- **No Exception Throwing**: All methods return `Result<T>` which contains either a success value or an error
- **Testable and Mockable**: The `IDbCommandExecutor` interface makes it easy to mock database operations in unit tests
- **Multiple Command Patterns**: Supports command factories, function-based commands, and direct stored procedure calls
- **Async Support**: Full async/await support with CancellationToken

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
```

#### ExecuteAndBind - Binding Results

```csharp
Result<int> result = executor.ExecuteAndBind(
    factory,
    cmd =>
    {
        int outputId = cmd.GetParameterValue<int>("OutputId");
        if (outputId > 0)
        {
            return Result<int>.Success(outputId);
        }
        return Result<int>.Failure(new Error("No output parameter"));
    }
);
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

Each pattern has both synchronous and asynchronous versions, with async versions supporting `CancellationToken`.

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
    cmd => Result<int>.Success(cmd.GetParameterValue<int>("NewId"))
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
    cmd => Result<string>.Success(cmd.GetParameterValue<string>("Status"))
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
    cmd => Result<int>.Success(cmd.GetParameterValue<int>("ProductId"))
)
.Tap(productId => Console.WriteLine($"New Product ID: {productId}"))
.TapError(error => Console.WriteLine($"Error: {error.Message}"));

// Example 2: Update with input/output parameter
executor.ExecuteAndBind(
    db => db.GetStoredProcCommand("UpdateInventory")
        .WithInputParameter("ProductId", DbType.Int32, productId)
        .WithInputOutputParameter("Quantity", DbType.Int32, requestedQuantity),
    cmd => Result<int>.Success(cmd.GetParameterValue<int>("Quantity"))
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
    cmd =>
    {
        var transactionId = cmd.GetParameterValue<int>("TransactionId");
        var status = cmd.GetParameterValue<string>("Status");
        var remainingCredit = cmd.GetParameterValue<decimal>("AvailableCredit");

        return Result<OrderProcessingResult>.Success(new OrderProcessingResult
        {
            TransactionId = transactionId,
            Status = status,
            RemainingCredit = remainingCredit
        });
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

## Logging

The `Voyager.DBConnection.Logging` extension provides logging capabilities for database operations. After installing the package, call the extension method on the connection object:

```csharp
namespace Voyager.DBConnection
{
    public static class ConnectionLogger
    {
        public static void AddLogger(this Connection connection, ILogger logger)
        {
            connection.AddFeature(new LogFeature(logger, connection));
        }
    }
}
```

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