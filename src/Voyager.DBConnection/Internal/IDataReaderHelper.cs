using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Voyager.Common.Results;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection.Internal
{
	/// <summary>
	/// Interface for helper class responsible for DataReader operations.
	/// </summary>
	internal interface IDataReaderHelper
	{
		/// <summary>
		/// Executes a command and returns a DataReader.
		/// </summary>
		Result<IDataReader> GetDataReader(DbCommand command);

		/// <summary>
		/// Asynchronously executes a command and returns a DataReader.
		/// </summary>
		Task<Result<IDataReader>> GetDataReaderAsync(DbCommand command, CancellationToken cancellationToken);

		/// <summary>
		/// Processes a DataReader using the provided consumer.
		/// </summary>
		TDomain HandleReader<TDomain>(IGetConsumer<TDomain> consumer, IDataReader reader);
	}
}
