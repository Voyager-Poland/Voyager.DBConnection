

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