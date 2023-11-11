using Microsoft.Extensions.Logging;
using System;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	[Obsolete("Use AddLogger")]
	public class ConnectionLogged : Connection
	{
		public ConnectionLogged(Database db, IExceptionPolicy exceptionPolicy, ILogger logger) : base(db, exceptionPolicy)
		{
			this.AddLogger(logger);
		}

		public ConnectionLogged(Database db, ILogger logger) : base(db)
		{
			this.AddLogger(logger);
		}
	}
}
