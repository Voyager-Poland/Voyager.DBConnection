using System.Data;

namespace Voyager.DBConnection.Interfaces
{
	/// <summary>
	/// Defines a consumer that processes data from a database reader and produces a domain object.
	/// </summary>
	/// <typeparam name="TDomainObject">The type of domain object produced by this consumer.</typeparam>
	public interface IResultsConsumer<TDomainObject>
	{
		/// <summary>
		/// Processes the data reader and produces a domain object.
		/// </summary>
		/// <param name="dataReader">The data reader containing the result set to process.</param>
		/// <returns>A domain object of type <typeparamref name="TDomainObject"/> populated from the data reader.</returns>
		TDomainObject GetResults(IDataReader dataReader);
	}
}
