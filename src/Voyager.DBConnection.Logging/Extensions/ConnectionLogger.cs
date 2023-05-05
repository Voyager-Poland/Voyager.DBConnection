using Microsoft.Extensions.Logging;

namespace Voyager.DBConnection
{
	public static class ConnectionLogger
	{
		public static void AddLogger(this Connection connection, ILogger logger)
		{
			connection.AddFeature(new LogFeature(logger, connection));
		}
	}
}
