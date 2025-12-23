using Microsoft.Extensions.Logging;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{

	class LogFeature : IFeature
	{
		private readonly ILogger logger;
		private readonly IRegisterEvents registerEvents;

		public LogFeature(ILogger logger, IRegisterEvents registerEvents)
		{
			this.logger = logger;
			this.registerEvents = registerEvents;
			this.registerEvents.AddEvent(LogEvent);
		}

		public void Dispose()
		{
			this.registerEvents.RemoveEvent(LogEvent);
		}

		private void LogEvent(SqlCallEvent obj)
		{
			if (obj.IsError)
				logger?.LogError(obj.ToString());
			else
				logger?.LogInformation(obj.ToString());
		}
	}
}
