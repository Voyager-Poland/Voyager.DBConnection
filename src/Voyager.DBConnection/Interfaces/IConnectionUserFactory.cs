namespace Voyager.DBConnection.Interfaces
{
	[System.Obsolete("It is a mistake")]
	public interface IConnectionUserFactory<TType>
	{
		TType GetUser(Connection connection);
	}
}
