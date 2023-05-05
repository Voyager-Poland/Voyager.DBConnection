namespace Voyager.DBConnection.Interfaces
{

	public interface IConnectionUserFactory<TType>
	{
		TType GetUser(Connection connection);
	}
}
