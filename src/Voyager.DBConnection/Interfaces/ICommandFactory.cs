using System.Data.Common;

namespace Voyager.DBConnection.Interfaces
{
	public interface ICommandFactory : IReadOutParameters
	{
		DbCommand ConstructDbCommand(Database db);
	}

}
