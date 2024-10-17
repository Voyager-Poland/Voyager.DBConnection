using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
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

		[Obsolete("This object is only for mock purposese")]
		public Connection() : this(new Database()) { }

		public Connection(Database db, IExceptionPolicy exceptionPolicy)
		{
			DBPolicyGuard(exceptionPolicy);
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
			this.db.RealseConnection();
			featureHost.Dispose();
		}

		private T ProcCallEvent<T>(DbCommand command, Func<DbCommand, T> action)
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
				proc.ErrorPublish(handled);
				throw handled;
			}
		}


		protected virtual IDataReader GetDataReader(DbCommand command)
		{
			Func<DbCommand, IDataReader> lambdaReader = (commandPara) => commandPara.ExecuteReader();
			return ProcCallEvent(command, lambdaReader);
		}

		protected virtual int Exec(DbCommand command)
		{
			Func<DbCommand, Int32> lambdaNoQuery = (commandPara) => commandPara.ExecuteNonQuery();
			return ProcCallEvent(command, lambdaNoQuery);
		}

		protected virtual object ExecScalar(DbCommand command)
		{
			Func<DbCommand, object> lambdaScalar = (commandPara) => commandPara.ExecuteScalar();
			return ProcCallEvent(command, lambdaScalar);
		}

		protected virtual DbCommand GetCommand(ICommandFactory storedProcedure)
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

		private object ProcessExecuteScalar(IReadOutParameters factory, DbCommand command)
		{
			object result = ExecScalar(command);

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

		private static void DBPolicyGuard(IExceptionPolicy exceptionPolicy)
		{
			if (exceptionPolicy == null)
				throw new LackExceptionPolicyException();
		}

	}
}
