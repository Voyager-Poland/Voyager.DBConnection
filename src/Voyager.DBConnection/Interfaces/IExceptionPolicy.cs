namespace Voyager.DBConnection.Interfaces
{
	public interface IExceptionPolicy
	{
		Exception GetException(Exception ex);
	}
}
