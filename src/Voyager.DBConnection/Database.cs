using System;
using System.Data;
using System.Data.Common;
using Voyager.DBConnection.Interfaces;
using Voyager.DBConnection.MockServcie;
using Voyager.DBConnection.Tools;

namespace Voyager.DBConnection
{

	public class Database : ITransactionOwner
	{
		private readonly DbProviderFactory dbProviderFactory;
		private readonly string sqlConnectionString;
		private DbConnection dbConnection;
		private Transaction transaction;

		/// <summary>
		/// Default constructor uses mock provider and a mock connection string
		/// </summary>
		public Database() : this("Data Source=mockSql; Initial Catalog=FanyDB; Integrated Security = true;", new DbProviderFactoryMock())
		{

		}


		public Database(string sqlConnectionString, DbProviderFactory dbProviderFactory)
		{
			this.dbProviderFactory = dbProviderFactory;
			this.sqlConnectionString = sqlConnectionString;
			dbConnection = null;
			transaction = null;
		}

		internal Transaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			PrepareConnection();
			var tran = dbConnection!.BeginTransaction(isolationLevel);

			transaction = new Transaction(tran, this);
			return transaction;
		}

		public virtual DbCommand GetStoredProcCommand(string procedureName)
		{
			DbCommand cmd = this.dbProviderFactory.GetStroredProcedure(procedureName);
			return cmd;
		}


		public virtual DbCommand GetSqlCommand(string procedureName)
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


		public virtual void AddInParameter(DbCommand command,
																	string name,
																	DbType dbType)
		{
			AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, null!);
		}

		public virtual void AddInParameter(DbCommand command,
																	 string name,
																	 DbType dbType,
																	 object value)
		{
			AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, value);
		}

		public virtual void AddInParameter(DbCommand command,
																	 string name,
																	 DbType dbType,
																	 string sourceColumn,
																	 DataRowVersion sourceVersion)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.Input, true, 0, 0, sourceColumn, sourceVersion, null!);
		}

		public virtual DbParameter AddOutParameter(DbCommand command,
																	 string name,
																	 DbType dbType,
																	 int size)
		{
			return AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, DBNull.Value);
		}


		public virtual void AddInOutParameter(DbCommand command, string name, DbType dbType, object value)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.InputOutput, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		public virtual void AddInOutParameter(DbCommand command, string name, DbType dbType, int size, object value)
		{
			AddParameter(command, name, dbType, size, ParameterDirection.InputOutput, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		public virtual void AddOutParameter(DbCommand command, string name, DbType dbType, object value)
		{
			AddParameter(command, name, dbType, 0, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		public virtual void AddOutParameter(DbCommand command, string name, DbType dbType, int size, object value)
		{
			AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, value);
		}

		public virtual object GetParameterValue(DbCommand command, string name)
		{
			if (command == null) throw new ArgumentNullException(nameof(command));

			return command.Parameters[BuildParameterName(name)].Value;
		}


		protected DbParameter CreateParameter(string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			if (dbProviderFactory == null) throw new ArgumentNullException(nameof(transaction));
			DbParameter param = dbProviderFactory.CreateParameter();
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

		[Obsolete]
		protected char ParameterToken = '0';

		protected virtual string GetParameterToken() => string.Empty;


		string BuildParameterName(string name)
		{
			ParamNameRule paramNameRule = new ParamNameRule(GetParameterToken());
			return paramNameRule.GetParamName(name);
		}

		private void DoConnection()
		{
			dbConnection = this.dbProviderFactory.CreateConnection();
			dbConnection.ConnectionString = UppDateConnectionString(sqlConnectionString, dbProviderFactory);
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
		protected virtual string UppDateConnectionString(string sqlConnectionString, DbProviderFactory dbProviderFactory) => this.GetPrepare(sqlConnectionString, dbProviderFactory).Prepare();

		protected virtual PrepareConectionString GetPrepare(string sqlConnectionString, DbProviderFactory dbProviderFactory) => new PrepareConectionString(this.dbProviderFactory, sqlConnectionString);

		void ITransactionOwner.ResetTransaction()
		{
			transaction = null;
		}

		void PrepareConnection()
		{
			if (!ConnectionIsReady)
			{
				if (dbConnection != null)
					RealseConnection();
				DoConnection();
			}
			if (dbConnection!.State != ConnectionState.Open)
				dbConnection.Open();
		}


		internal void OpenCmd(DbCommand cmd)
		{
			PrepareConnection();
			cmd.Connection = this.dbConnection;

			if (this.transaction != null)
				cmd.Transaction = this.transaction.GetTransaction();
		}
	}

}