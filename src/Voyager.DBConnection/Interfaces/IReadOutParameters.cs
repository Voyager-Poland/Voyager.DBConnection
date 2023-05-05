using System.Data.Common;

namespace Voyager.DBConnection.Interfaces
{
	public interface IReadOutParameters
	{
		void ReadOutParameters(Database db, DbCommand command);
	}
}
