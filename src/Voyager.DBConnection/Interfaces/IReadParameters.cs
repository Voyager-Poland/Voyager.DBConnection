using System.Data.Common;
using Voyager.Common.Results;

namespace Voyager.DBConnection.Interfaces
{
    public interface IReadParameters
    {
        Result<TValue> ReadValue<TValue>(DbCommand command);
    }
}
