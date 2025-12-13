using System.Data.Common;
using Voyager.Common.Results;

namespace Voyager.DBConnection.Interfaces
{
	public interface IReadOutParameters
	{
		void ReadOutParameters(IDatabase db, DbCommand command);
	}

	public interface IReadParameters
	{
		Result<TValue> ReadValue<TValue>(DbCommand command);
	}
}
