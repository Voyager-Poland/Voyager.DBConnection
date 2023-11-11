using System.Data.Common;

namespace Voyager.DBConnection.MsSql
{
	public class SqlName
	{
		private Connection connection;

		public SqlName(Connection connection)
		{
			this.connection = connection;
		}

		public string Name()
		{
			SqlReadSqlName sqlReadSqlName = new SqlReadSqlName();
			connection.ExecuteNonQuery(sqlReadSqlName);
			return sqlReadSqlName.SqlName;
		}
	}

	class SqlReadSqlName : Interfaces.ICommandFactory
	{
		public SqlReadSqlName()
		{
			SqlName = "?";
		}
		public string SqlName { get; private set; }

		public DbCommand ConstructDbCommand(Database db)
		{
			var cmd = db.GetSqlCommand("SELECT @servname = @@SERVERNAME");
			db.AddOutParameter(cmd, "servname", System.Data.DbType.String, 255);
			return cmd;
		}

		public void ReadOutParameters(Database db, DbCommand command)
		{
			this.SqlName = (string)db.GetParameterValue(command, "servname");
		}
	}
}
