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
	/// <summary>
	/// Provides a high-level interface for executing database commands with exception handling and event support.
	/// Throws exceptions on errors (unlike DbCommandExecutor which returns Result&lt;T&gt;).
	/// </summary>
	public class Connection : IDisposable, IRegisterEvents, IFeatureHost
	{
		private readonly Database db;
		private readonly IExceptionPolicy exceptionPolicy;
		private readonly EventHost eventHost = new EventHost();
		private readonly FeatureHost featureHost = new FeatureHost();
		private bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="Connection"/> class.
		/// </summary>
		/// <remarks>This constructor is only for mock purposes and should not be used in production code.</remarks>
		[Obsolete("This object is only for mock purposes")]
		public Connection() : this(new Database()) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Connection"/> class with a custom exception handling policy.
		/// </summary>
		/// <param name="db">The database instance.</param>
		/// <param name="exceptionPolicy">The exception policy for transforming thrown exceptions.</param>
		/// <exception cref="LackExceptionPolicyException">Thrown when exceptionPolicy is null.</exception>
		/// <exception cref="NoDbException">Thrown when db is null.</exception>
		public Connection(Database db, IExceptionPolicy exceptionPolicy)
		{
			Guard.DBPolicyGuard(exceptionPolicy);
			Guard.DbGuard(db);

			this.db = db;
			this.exceptionPolicy = exceptionPolicy;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Connection"/> class with default exception handling.
		/// </summary>
		/// <param name="db">The database instance.</param>
		/// <exception cref="NoDbException">Thrown when db is null.</exception>
		public Connection(Database db)
		{
			Guard.DbGuard(db);
			this.db = db;
			this.exceptionPolicy = new NoExceptionPolicy();
		}

		/// <summary>
		/// Begins a new database transaction.
		/// </summary>
		/// <returns>A <see cref="Transaction"/> object representing the new transaction.</returns>
		public virtual Transaction BeginTransaction()
		{
			return db.BeginTransaction();
		}

		/// <summary>
		/// Executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
		/// </summary>
		/// <param name="factory">The factory that creates the database command.</param>
		/// <returns>The number of rows affected by the command.</returns>
		/// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
		/// <exception cref="Exception">Throws transformed exceptions according to the exception policy.</exception>
		public virtual int ExecuteNonQuery(ICommandFactory factory)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			using (DbCommand command = GetCommand(factory))
				return ProcessExecuteNoQuery(factory, command);
		}

		/// <summary>
		/// Executes a command and returns the first column of the first row in the result set.
		/// </summary>
		/// <param name="factory">The factory that creates the database command.</param>
		/// <returns>The scalar value from the query, or null if no results.</returns>
		/// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
		/// <exception cref="Exception">Throws transformed exceptions according to the exception policy.</exception>
		public virtual object ExecuteScalar(ICommandFactory factory)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			using (DbCommand command = GetCommand(factory))
				return ProcessExecuteScalar(factory, command);
		}

		/// <summary>
		/// Asynchronously executes a command and returns the first column of the first row in the result set.
		/// </summary>
		/// <param name="factory">The factory that creates the database command.</param>
		/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
		/// <returns>A task representing the asynchronous operation, containing the scalar value or null.</returns>
		/// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
		public Task<object> ExecuteScalarAsync(ICommandFactory factory, CancellationToken cancellationToken)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			return Task.Run(() => ExecuteScalar(factory), cancellationToken);
		}

		/// <summary>
		/// Asynchronously executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
		/// </summary>
		/// <param name="factory">The factory that creates the database command.</param>
		/// <returns>A task representing the asynchronous operation, containing the number of rows affected.</returns>
		/// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
		public virtual Task<int> ExecuteNonQueryAsync(ICommandFactory factory) => ExecuteNonQueryAsync(factory, CancellationToken.None);

		/// <summary>
		/// Asynchronously executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
		/// </summary>
		/// <param name="factory">The factory that creates the database command.</param>
		/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
		/// <returns>A task representing the asynchronous operation, containing the number of rows affected.</returns>
		/// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
		public virtual Task<int> ExecuteNonQueryAsync(ICommandFactory factory, CancellationToken cancellationToken)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			return Task.Run(() => ExecuteNonQuery(factory), cancellationToken);
		}

		/// <summary>
		/// Asynchronously executes a command and processes the result set using a consumer.
		/// </summary>
		/// <typeparam name="TDomain">The type of the result produced by the consumer.</typeparam>
		/// <param name="factory">The factory that creates the database command.</param>
		/// <param name="consumer">The consumer that processes the data reader.</param>
		/// <returns>A task representing the asynchronous operation, containing the processed result.</returns>
		/// <exception cref="ArgumentNullException">Thrown when factory or consumer is null.</exception>
		public virtual Task<TDomain> GetReaderAsync<TDomain>(ICommandFactory factory, IGetConsumer<TDomain> consumer) => GetReaderAsync(factory, consumer, CancellationToken.None);

		/// <summary>
		/// Asynchronously executes a command and processes the result set using a consumer.
		/// </summary>
		/// <typeparam name="TDomain">The type of the result produced by the consumer.</typeparam>
		/// <param name="factory">The factory that creates the database command.</param>
		/// <param name="consumer">The consumer that processes the data reader.</param>
		/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
		/// <returns>A task representing the asynchronous operation, containing the processed result.</returns>
		/// <exception cref="ArgumentNullException">Thrown when factory or consumer is null.</exception>
		public virtual Task<TDomain> GetReaderAsync<TDomain>(ICommandFactory factory, IGetConsumer<TDomain> consumer, CancellationToken cancellationToken)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (consumer == null) throw new ArgumentNullException(nameof(consumer));
			return Task.Run(() => GetReader(factory, consumer), cancellationToken);
		}

		/// <summary>
		/// Executes a command and processes the result set using a consumer.
		/// </summary>
		/// <typeparam name="TDomain">The type of the result produced by the consumer.</typeparam>
		/// <param name="factory">The factory that creates the database command.</param>
		/// <param name="consumer">The consumer that processes the data reader.</param>
		/// <returns>The result produced by the consumer after processing the data reader.</returns>
		/// <exception cref="ArgumentNullException">Thrown when factory or consumer is null.</exception>
		/// <exception cref="Exception">Throws transformed exceptions according to the exception policy.</exception>
		public virtual TDomain GetReader<TDomain>(ICommandFactory factory, IGetConsumer<TDomain> consumer)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			if (consumer == null) throw new ArgumentNullException(nameof(consumer));
			using (DbCommand command = GetCommand(factory))
				return ProcessReader(factory, consumer, command);
		}

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				// free managed
				this.db.Dispose();
				featureHost.Dispose();
				// if eventHost implements IDisposable, dispose it here
				// (kept as-is to avoid breaking changes)
			}
			// free unmanaged (none)
			disposed = true;
		}

		~Connection()
		{
			Dispose(false);
		}

		private T ExecuteWithEvents<T>(DbCommand command, Func<DbCommand, T> action)
		{
			var proc = new ExecutionEventPublisher<T>(this.eventHost);

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
			Func<DbCommand, IDataReader> executeReader = (cmd) => cmd.ExecuteReader();
			return ExecuteWithEvents(command, executeReader);
		}

		protected virtual int Exec(DbCommand command)
		{
			Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
			return ExecuteWithEvents(command, executeNonQuery);
		}

		protected virtual object ExecScalar(DbCommand command)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));
			Func<DbCommand, object> executeScalar = (cmd) => cmd.ExecuteScalar();
			return ExecuteWithEvents(command, executeScalar);
		}

		protected virtual DbCommand GetCommand(ICommandFactory storedProcedure)
		{
			if (storedProcedure == null) throw new ArgumentNullException(nameof(storedProcedure));
			return storedProcedure.ConstructDbCommand(db);
		}

		protected virtual Exception HandleSqlException(Exception ex)
		{
			return exceptionPolicy.GetException(ex);
		}

		protected virtual void ReadOutParameters(IReadOutParameters factory, DbCommand command)
		{
			if (factory == null) return;
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
			// dr.Close(); // removed — Dispose is called in Finally()

			return result;
		}

		/// <summary>
		/// Registers an event handler for SQL call events.
		/// </summary>
		/// <param name="logEvent">The event handler to register.</param>
		public void AddEvent(Action<SqlCallEvent> logEvent)
		{
			eventHost.AddEvent(logEvent);
		}

		/// <summary>
		/// Unregisters an event handler for SQL call events.
		/// </summary>
		/// <param name="logEvent">The event handler to unregister.</param>
		public void RemoveEvent(Action<SqlCallEvent> logEvent)
		{
			eventHost.RemoveEvent(logEvent);
		}

		/// <summary>
		/// Adds a feature to extend the functionality of the connection.
		/// </summary>
		/// <param name="feature">The feature to add.</param>
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
