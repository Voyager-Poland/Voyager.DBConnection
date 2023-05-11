using System.Data;
using System.Data.Common;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	public class Connection : IDisposable, IRegisterEvents, IFeatureHost
	{
		private readonly Database db;
		private readonly IExceptionPolicy exceptionPolicy;
		private readonly EventHost eventHost = new EventHost();
		private readonly FeatureHost featureHost = new FeatureHost();

		public Connection(Database db, IExceptionPolicy exceptionPolicy)
		{
			if (exceptionPolicy == null)
				throw new LackExceptionPolicyException();
			DbGuard(db);

			this.db = db;
			this.exceptionPolicy = exceptionPolicy;
		}

		public Connection(Database db)
		{
			DbGuard(db);
			this.db = db;
			this.exceptionPolicy = new NoExceptiopnPolicy();
		}

		public Transaction BeginTransaction()
		{
			return db.BeginTransaction();
		}

		public int ExecuteNonQuery(ICommandFactory factory)
		{
			using DbCommand command = GetCommand(factory);
			return ProcessExecuteNoQuery(factory, command);
		}

		public Task<int> ExecuteNonQueryAsync(ICommandFactory factory)
		{
			return Task.Run(() =>
			{
				return ExecuteNonQuery(factory);
			});
		}

		public Task<TDomain> GetReaderAsync<TDomain>(ICommandFactory factory, IGetConsumer<TDomain> consumer)
		{
			return Task.Run(() =>
			{
				return GetReader(factory, consumer);
			});
		}

		public TDomain GetReader<TDomain>(ICommandFactory factory, IGetConsumer<TDomain> consumer)
		{
			using DbCommand command = GetCommand(factory);
			return ProcessReader(factory, consumer, command);
		}

		public void Dispose()
		{
			this.db.RealseConnection();
			featureHost.Dispose();
		}

		private IDataReader GetDataReader(DbCommand command)
		{
			var proc = new ProcEvent<IDataReader>(this.eventHost);

			try
			{
				db.OpenCmd(command);

				return proc.Call(() =>
				{
					return command.ExecuteReader();
				}, command);
			}
			catch (Exception ex)
			{
				var handled = HandleSqlException(ex);
				proc.ErrorPublish(handled);
				throw handled;
			}
		}

		private int Exec(DbCommand command)
		{
			var proc = new ProcEvent<int>(this.eventHost);
			try
			{
				db.OpenCmd(command);

				return proc.Call(() =>
				{
					return command.ExecuteNonQuery();
				}, command);

			}
			catch (Exception ex)
			{
				var handled = HandleSqlException(ex);
				proc.ErrorPublish(handled);
				throw handled;
			}
		}

		public virtual DbCommand GetCommand(ICommandFactory storedProcedure)
		{
			return storedProcedure.ConstructDbCommand(db);
		}

		protected virtual Exception HandleSqlException(Exception ex)
		{
			return exceptionPolicy.GetException(ex);
		}

		protected virtual void ReadOutPrameters(IReadOutParameters factory, DbCommand command)
		{
			factory.ReadOutParameters(db, command);
		}

		private int ProcessExecuteNoQuery(IReadOutParameters factory, DbCommand command)
		{
			int result = Exec(command);

			ReadOutPrameters(factory, command);
			return result;
		}

		private TDomain ProcessReader<TDomain>(IReadOutParameters factory, IGetConsumer<TDomain> consumer, DbCommand command)
		{
			IDataReader dr = GetDataReader(command);
			TDomain result = HandleReader(consumer, dr);
			ReadOutPrameters(factory, command);
			return result;
		}

		private TDomain HandleReader<TDomain>(IGetConsumer<TDomain> consumer, IDataReader dr)
		{
			TDomain result = consumer.GetResults(dr);
			while (dr.NextResult())
			{ }
			dr.Close();

			return result;
		}

		public void AddEvent(Action<SqlCallEvent> logEvent)
		{
			eventHost.AddEvent(logEvent);
		}

		public void RemoveEvent(Action<SqlCallEvent> logEvent)
		{
			eventHost.RemoveEvent(logEvent);
		}

		public void AddFeature(IFeature feature)
		{
			featureHost.AddFeature(feature);
		}

		private static void DbGuard(Database db)
		{
			if (db == null)
				throw new NoDbException();
		}
	}
}
