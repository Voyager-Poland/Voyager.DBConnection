# Voyager.DBConnection
Library providing connection to SQL database using DbProviderFactory.

## How to use it

Implement interface  Voyager.DBConnection.Interfaces.ICommandFactory:

```C#
	public interface ICommandFactory : IReadOutParameters
	{
		DbCommand ConstructDbCommand(Database db);
	}
```

Example code is like:
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
Using your DbProviderFactory create your type of database object Voyager.DBConnection.Database and Voyager.DBConnection.Connection. On the connection object call ExecuteNonQuery the command factory:

```C#
			SetPilotiDoPowiadomieniaFactory factory = new SetPilotiDoPowiadomieniaFactory(tagItem.IdBusMapRNp, tagItem.BusMapDate, tagItem.IdAkwizytor, tagItem.Raport);
			con.ExecuteNonQuery(factory);
```

For reading data implement IGetConsumer interface:

```C#
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

Next, call the GetReader method:

```C#
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

There is an extension used to log operations. Voyager.DBConnection.Logging. After installing on the connection obcjet is needed to call extension:

```C#

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

## MS Sql provider

For using MS SQL Provider is prepared the Nuget Voyager.DBConnection.MySql. There is provided implementation of the connection object:

```C#
namespace Voyager.DBConnection.MsSql
{
	public class SqlConnection : Connection
	{
		public SqlConnection(string sqlConnectionString) : base(new SqlDatabase(sqlConnectionString), new ExceptionFactory())
		{
		}
	}
}

```

## ✍️ Authors 

- [@andrzejswistowski](https://github.com/AndrzejSwistowski) - Idea & work. Please let me know if you find out an error or suggestions.

[contributors](https://github.com/Voyager-Poland).

## 🎉 Acknowledgements 

- Przemysław Wróbel - for the icon.