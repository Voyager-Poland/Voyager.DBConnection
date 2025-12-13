using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;
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

		[Obsolete("This object is only for mock purposes")]
		public Connection() : this(new Database()) { }

		public Connection(Database db, IExceptionPolicy exceptionPolicy)
		{
			Guard.DBPolicyGuard(exceptionPolicy);
			Guard.DbGuard(db);

			this.db = db;
			this.exceptionPolicy = exceptionPolicy;
		}


		public Connection(Database db)
		{
			Guard.DbGuard(db);
			this.db = db;
			this.exceptionPolicy = new NoExceptionPolicy();
		}

		public virtual Transaction BeginTransaction()
		{
			return db.BeginTransaction();
		}

		public virtual int ExecuteNonQuery(ICommandFactory factory)
		{
			using (DbCommand command = GetCommand(factory))
				return ProcessExecuteNoQuery(factory, command);
		}

		public virtual object ExecuteScalar(ICommandFactory factory)
		{
			using (DbCommand command = GetCommand(factory))
				return ProcessExecuteScalar(factory, command);
		}

		public Task<object> ExecuteScalarAsync(ICommandFactory factory, CancellationToken cancellationToken)
		{
			return Task.Run(() =>
			{
				return ExecuteScalar(factory);
			}, cancellationToken);
		}

		public virtual Task<int> ExecuteNonQueryAsync(ICommandFactory factory) => ExecuteNonQueryAsync(factory, CancellationToken.None);

		public virtual Task<int> ExecuteNonQueryAsync(ICommandFactory factory, CancellationToken cancellationToken)
		{
			return Task.Run(() =>
			{
				return ExecuteNonQuery(factory);
			}, cancellationToken);
		}

		public virtual Task<TDomain> GetReaderAsync<TDomain>(ICommandFactory factory, IGetConsumer<TDomain> consumer) => GetReaderAsync(factory, consumer, CancellationToken.None);

		public virtual Task<TDomain> GetReaderAsync<TDomain>(ICommandFactory factory, IGetConsumer<TDomain> consumer, CancellationToken cancellationToken)
		{
			return Task.Run(() =>
			{
				return GetReader(factory, consumer);
			}, cancellationToken);
		}

		public virtual TDomain GetReader<TDomain>(ICommandFactory factory, IGetConsumer<TDomain> consumer)
		{
			using (DbCommand command = GetCommand(factory))
				return ProcessReader(factory, consumer, command);
		}

		public virtual void Dispose()
		{
			this.db.Dispose();
			featureHost.Dispose();
		}

		private T ExecuteWithEvents<T>(DbCommand command, Func<DbCommand, T> action)
		{
			var proc = new ProcEvent<T>(this.eventHost);

			try
			{
				db.OpenCmd(command);
				return proc.Call(() => action.Invoke(command), command);
			}
			catch (Exception ex)
			{
				var handled = HandleSqlException(ex);
				proc.ExceptionPublish(handled);
				throw handled;
			}
		}


		protected virtual IDataReader GetDataReader(DbCommand command)
		{
			Func<DbCommand, IDataReader> executeReader = (commandPara) => commandPara.ExecuteReader();
			return ExecuteWithEvents(command, executeReader);
		}

		protected virtual int Exec(DbCommand command)
		{
			Func<DbCommand, Int32> executeNonQuery = (commandPara) => commandPara.ExecuteNonQuery();
			return ExecuteWithEvents(command, executeNonQuery);
		}

		protected virtual object ExecScalar(DbCommand command)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));
			Func<DbCommand, object> executeScalar = (commandPara) => commandPara.ExecuteScalar();
			return ExecuteWithEvents(command, executeScalar);
		}

		protected virtual DbCommand GetCommand(ICommandFactory storedProcedure)
		{
			return storedProcedure.ConstructDbCommand(db);
		}

		protected virtual Exception HandleSqlException(Exception ex)
		{
			return exceptionPolicy.GetException(ex);
		}

		protected virtual void ReadOutParameters(IReadOutParameters factory, DbCommand command)
		{
			factory.ReadOutParameters(db, command);
		}

		private int ProcessExecuteNoQuery(IReadOutParameters factory, DbCommand command)
		{
			int result = Exec(command);

			ReadOutParameters(factory, command);
			return result;
		}

		private object ProcessExecuteScalar(IReadOutParameters factory, DbCommand command)
		{
			object result = ExecScalar(command);

			ReadOutParameters(factory, command);
			return result;
		}

		private TDomain ProcessReader<TDomain>(IReadOutParameters factory, IGetConsumer<TDomain> consumer, DbCommand command)
		{
			using (IDataReader dr = GetDataReader(command))
			{
				TDomain result = HandleReader(consumer, dr);
				ReadOutParameters(factory, command);
				return result;
			}
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

	}

	internal static class Guard
	{

		public static void DbGuard(Database db)
		{
			if (db == null)
				throw new NoDbException();
		}

		public static void DBPolicyGuard(IExceptionPolicy exceptionPolicy)
		{
			if (exceptionPolicy == null)
				throw new LackExceptionPolicyException();
		}

		public static void DBPolicyGuard(IMapErrorPolicy exceptionPolicy)
		{
			if (exceptionPolicy == null)
				throw new LackExceptionPolicyException();
		}
	}
}
