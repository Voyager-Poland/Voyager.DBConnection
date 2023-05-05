using System.Data.Common;

namespace Voyager.DBConnection.Test
{
	internal class WaitForCommand : Voyager.DBConnection.Interfaces.ICommandFactory
	{
		public DbCommand ConstructDbCommand(Database db)
		{
			return db.GetSqlCommand("WAITFOR delay '00:00:01'");
		}

		public void ReadOutParameters(Database db, DbCommand command)
		{

		}
	}
}
