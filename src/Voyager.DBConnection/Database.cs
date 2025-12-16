using System;
using System.Data;
using System.Data.Common;
using Voyager.DBConnection.MockServcie;
using Voyager.DBConnection.Tools;

namespace Voyager.DBConnection
{
	/// <summary>
	/// Provides database connection and command execution functionality.
	/// Manages database connections, transactions, and command creation with support for stored procedures and SQL text commands.
	/// </summary>
	public class Database : IDisposable, IDatabase
	{
		private readonly DbProviderFactory dbProviderFactory;
		private readonly string sqlConnectionString;
		private readonly ConnectionHolder connectionHolder;
		private TransactionHolder transactionHolder;
		private bool disposed;

		/// <summary>
		/// Default constructor uses mock provider and a mock connection string
		/// </summary>
		[Obsolete("This constructor is only for mock purposes", true)]
		public Database() : this("Data Source=mockSql; Initial Catalog=FanyDB; Integrated Security = true;", new DbProviderFactoryMock())
		{

		}


		/// <summary>
		/// Initializes a new instance of the Database class with the specified connection string and provider factory.
		/// </summary>
		/// <param name="sqlConnectionString">The connection string for the database.</param>
		/// <param name="dbProviderFactory">The provider factory used to create database connections and commands.</param>
		/// <exception cref="ArgumentNullException">Thrown when sqlConnectionString or dbProviderFactory is null.</exception>
		public Database(string sqlConnectionString, DbProviderFactory dbProviderFactory)
		{
			this.dbProviderFactory = dbProviderFactory ?? throw new ArgumentNullException(nameof(dbProviderFactory));
			this.sqlConnectionString = sqlConnectionString ?? throw new ArgumentNullException(nameof(sqlConnectionString));
			this.connectionHolder = new ConnectionHolder(dbProviderFactory, () => UpdateConnectionString(sqlConnectionString, dbProviderFactory));
		}

		internal Transaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			if (transactionHolder?.IsActive == true)
				throw new InvalidOperationException("Transaction already active");

			var connection = connectionHolder.GetConnection();
			transactionHolder = new TransactionHolder(connection, isolationLevel);
			return new Transaction(transactionHolder, () => transactionHolder = null);
		}

		/// <summary>
		/// Creates a database command configured for executing a stored procedure.
		/// </summary>
		/// <param name="procedureName">The name of the stored procedure.</param>
		/// <returns>A DbCommand configured to execute the specified stored procedure.</returns>
		public virtual DbCommand GetStoredProcCommand(string procedureName)
		{
			DbCommand cmd = this.dbProviderFactory.GetStroredProcedure(procedureName);
			return cmd;
		}


		/// <summary>
		/// Creates a database command configured for executing SQL text.
		/// </summary>
		/// <param name="procedureName">The SQL command text to execute.</param>
		/// <returns>A DbCommand configured to execute the specified SQL text.</returns>
		public virtual DbCommand GetSqlCommand(string procedureName)
		{
			DbCommand cmd = this.dbProviderFactory.GetSqlCommand(procedureName);
			return cmd;
		}

		[Obsolete("Use DbCommand.WithParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual DbParameter AddParameter(DbCommand command,
																				string name,
																				DbType dbType,
																				int size,
																				ParameterDirection direction,
																				bool nullable,
																				byte precision,
																				byte scale,
																				string sourceColumn,
																				DataRowVersion sourceVersion,
																				object value)
		{
			if (command == null) throw new ArgumentNullException(nameof(command));

			DbParameter parameter = CreateParameter(name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
			command.Parameters.Add(parameter);
			return parameter;
		}

		[Obsolete("Use DbCommand.WithParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual void AddParameter(DbCommand command,
																string name,
																DbType dbType,
																ParameterDirection direction,
																string sourceColumn,
																DataRowVersion sourceVersion,
																object value)
		{
			AddParameter(command, name, dbType, 0, direction, false, 0, 0, sourceColumn, sourceVersion, value);
		}


		[Obsolete("Use DbCommand.WithInputParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual void AddInParameter(DbCommand command,
																	string name,
																	DbType dbType)
		{
			AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, null!);
		}

		[Obsolete("Use DbCommand.WithInputParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual void AddInParameter(DbCommand command,
																	 string name,
																	 DbType dbType,
																	 object value)
		{
			AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, value);
		}

		[Obsolete("Use DbCommand.WithInputParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual void AddInParameter(DbCommand command,
																	 string name,
																	 DbType dbType,
																	 string sourceColumn,
																	 DataRowVersion sourceVersion)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.Input, true, 0, 0, sourceColumn, sourceVersion, null!);
		}

		[Obsolete("Use DbCommand.WithOutputParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual DbParameter AddOutParameter(DbCommand command,
																	 string name,
																	 DbType dbType,
																	 int size)
		{
			return AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, DBNull.Value);
		}


		[Obsolete("Use DbCommand.WithInputOutputParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual void AddInOutParameter(DbCommand command, string name, DbType dbType, object value)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.InputOutput, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		[Obsolete("Use DbCommand.WithInputOutputParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual void AddInOutParameter(DbCommand command, string name, DbType dbType, int size, object value)
		{
			AddParameter(command, name, dbType, size, ParameterDirection.InputOutput, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		[Obsolete("Use DbCommand.WithOutputParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual void AddOutParameter(DbCommand command, string name, DbType dbType, object value)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		[Obsolete("Use DbCommand.WithOutputParameter() extension method instead. This method will be removed in version 5.0.")]
		public virtual void AddOutParameter(DbCommand command, string name, DbType dbType, int size, object value)
		{
			AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		[Obsolete("Use DbCommand.GetParameterValue() extension method instead. This method will be removed in version 5.0.")]
		public virtual object GetParameterValue(DbCommand command, string name)
		{
			if (command == null) throw new ArgumentNullException(nameof(command));

			return command.Parameters[BuildParameterName(name)].Value;
		}

		[Obsolete]
		protected DbParameter CreateParameter(string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			if (dbProviderFactory == null) throw new ArgumentNullException(nameof(dbProviderFactory));
			DbParameter param = dbProviderFactory.CreateParameter();
			param.ParameterName = BuildParameterName(name);
			ConfigureParameter(param, name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
			return param;
		}

		[Obsolete]
		protected virtual void ConfigureParameter(DbParameter param, string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			param.DbType = dbType;
			param.Size = size;
			param.Value = value ?? DBNull.Value;
			param.Direction = direction;
			param.IsNullable = nullable;
			param.SourceColumn = sourceColumn;
			param.SourceVersion = sourceVersion;
		}

		[Obsolete]
		protected virtual string GetParameterToken() => string.Empty;

		[Obsolete]
		string BuildParameterName(string name)
		{
			ParamNameRule paramNameRule = new ParamNameRule(GetParameterToken());
			return paramNameRule.GetParamName(name);
		}

		/// <summary>
		/// Releases all resources used by the Database.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and optionally managed resources.
		/// </summary>
		/// <param name="disposing">True to release both managed and unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// TransactionHolder.Dispose() is idempotent - safe to call even if 
					// Transaction already disposed it. Acts as fallback if user forgot 
					// to dispose Transaction.
					transactionHolder?.Dispose();
					connectionHolder?.Dispose();
				}
				disposed = true;
			}
		}

		/// <summary>
		/// Updates the connection string by preparing it with current user information.
		/// </summary>
		/// <param name="sqlConnectionString">The base SQL connection string</param>
		/// <param name="dbProviderFactory">The database provider factory</param>
		/// <returns>The prepared connection string with updated application name</returns>
		protected virtual string UpdateConnectionString(string sqlConnectionString, DbProviderFactory dbProviderFactory)
			=> ConnectionStringHelper.PrepareConnectionString(dbProviderFactory, sqlConnectionString);

		/// <summary>
		/// Obsolete: Use UpdateConnectionString instead.
		/// Kept for backward compatibility.
		/// </summary>
		[Obsolete("Use UpdateConnectionString instead", false)]
		protected virtual string UppDateConnectionString(string sqlConnectionString, DbProviderFactory dbProviderFactory)
			=> this.UpdateConnectionString(sqlConnectionString, dbProviderFactory);


		internal void OpenCmd(DbCommand cmd)
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(Database));

			cmd.Connection = connectionHolder.GetConnection();

			if (transactionHolder?.IsActive == true)
				cmd.Transaction = transactionHolder.Transaction;
		}
	}

}