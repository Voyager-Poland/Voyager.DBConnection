using System.Data;
using System.Data.Common;
using Voyager.DBConnection.Interfaces;
using Voyager.DBConnection.Tools;

namespace Voyager.DBConnection
{



	public class Database : ITransactionOwner
	{


		private readonly DbProviderFactory dbProviderFactory;

		private readonly string sqlConnectionString;

		DbConnection dbConnection;

		Transaction transaction;
		public Database(string sqlConnectionString, DbProviderFactory dbProviderFactory)
		{
			this.dbProviderFactory = dbProviderFactory;
			this.sqlConnectionString = UppDateConnectionString(sqlConnectionString);
			dbConnection = null!;
			transaction = null!;
		}

		public Transaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			var connection = GetConnection();
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}
			var tran = connection.BeginTransaction(isolationLevel);

			transaction = new Transaction(tran, this);
			return transaction;
		}

		[Obsolete("Wywołać GetStoredProcCommand")]
		public Task<DbCommand> GetStoredProcCommandAsync(string procedureName)
		{
			DbCommand cmd = this.dbProviderFactory.GetStroredProcedure(procedureName);
			//	await OpenCmdAsync(cmd);
			return Task.FromResult(cmd);
		}

		public DbCommand GetStoredProcCommand(string procedureName)
		{
			DbCommand cmd = this.dbProviderFactory.GetStroredProcedure(procedureName);
			return cmd;
		}


		public DbCommand GetSqlCommand(string procedureName)
		{
			DbCommand cmd = this.dbProviderFactory.GetSqlCommand(procedureName);
			return cmd;
		}

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

		public void AddParameter(DbCommand command,
																string name,
																DbType dbType,
																ParameterDirection direction,
																string sourceColumn,
																DataRowVersion sourceVersion,
																object value)
		{
			AddParameter(command, name, dbType, 0, direction, false, 0, 0, sourceColumn, sourceVersion, value);
		}


		public void AddInParameter(DbCommand command,
																	string name,
																	DbType dbType)
		{
			AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, null!);
		}

		public void AddInParameter(DbCommand command,
																	 string name,
																	 DbType dbType,
																	 object value)
		{
			AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, value);
		}

		public void AddInParameter(DbCommand command,
																	 string name,
																	 DbType dbType,
																	 string sourceColumn,
																	 DataRowVersion sourceVersion)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.Input, true, 0, 0, sourceColumn, sourceVersion, null!);
		}

		public DbParameter AddOutParameter(DbCommand command,
																	 string name,
																	 DbType dbType,
																	 int size)
		{
			return AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, DBNull.Value);
		}


		public void AddInOutParameter(DbCommand command, string name, DbType dbType, object value)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.InputOutput, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		public void AddInOutParameter(DbCommand command, string name, DbType dbType, int size, object value)
		{
			AddParameter(command, name, dbType, size, ParameterDirection.InputOutput, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		public void AddOutParameter(DbCommand command, string name, DbType dbType, object value)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		public void AddOutParameter(DbCommand command, string name, DbType dbType, int size, object value)
		{
			AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		public virtual object GetParameterValue(DbCommand command, string name)
		{
			if (command == null) throw new ArgumentNullException(nameof(command));

			return command.Parameters[BuildParameterName(name)].Value!;
		}


		protected DbParameter CreateParameter(string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			DbParameter param = dbProviderFactory.CreateParameter()!;
			param.ParameterName = BuildParameterName(name);
			ConfigureParameter(param, name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
			return param;
		}

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

		protected char ParameterToken = '0';

		string BuildParameterName(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			if (ParameterToken != '0')
				if (name[0] != ParameterToken)
				{
					return name.Insert(0, new string(ParameterToken, 1));
				}
			return name;
		}

		private void DoConnection()
		{
			dbConnection = this.dbProviderFactory.CreateConnection()!;
			dbConnection.ConnectionString = this.sqlConnectionString;
		}

		bool ConnectionIsReady
		{
			get
			{
				return !(dbConnection == null || dbConnection.State == ConnectionState.Broken);
			}
		}

		internal void RealseConnection()
		{
			try
			{
				if (dbConnection != null)
					dbConnection.Dispose();
			}
			catch { }
		}
		protected virtual string UppDateConnectionString(string sqlConnectionString)
		{
			return (new PrepareConectionString(this.dbProviderFactory, sqlConnectionString)).Prepare();
		}

		void ITransactionOwner.ResetTransaction()
		{
			transaction = null!;
		}

		DbConnection GetConnection()
		{
			if (!ConnectionIsReady)
			{
				if (dbConnection != null)
					RealseConnection();
				DoConnection();
			}
			return dbConnection!;
		}

		/*
		internal async Task OpenCmdAsync(DbCommand cmd)
		{
			cmd.Connection = GetConnection();
			if (cmd.Connection.State != ConnectionState.Open)
				await cmd.Connection.OpenAsync();

			if (this.transaction != null)
				cmd.Transaction = this.transaction.GetTransaction();

		}
		*/

		internal void OpenCmd(DbCommand cmd)
		{
			cmd.Connection = GetConnection();
			if (cmd.Connection.State != ConnectionState.Open)
				cmd.Connection.Open();

			if (this.transaction != null)
				cmd.Transaction = this.transaction.GetTransaction();

		}
	}
}