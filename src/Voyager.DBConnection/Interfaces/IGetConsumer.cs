using System.Data;

namespace Voyager.DBConnection.Interfaces
{
	public interface IGetConsumer<TDomainObject>
	{
		TDomainObject GetResults(IDataReader dataReader);
	}
}
