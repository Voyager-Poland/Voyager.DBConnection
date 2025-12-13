using System;

namespace Voyager.DBConnection.Events
{
	public class CommonErrorEvent : SqlCallEvent
	{
		public CommonErrorEvent(Common.Results.Error error, SqlCallEvent callEvent) : base(callEvent.SqlText, callEvent.CallTime)
		{
			Error = error;
			this.Duration = callEvent.Duration;
			this.IsError = true;
		}

		public Common.Results.Error Error { get; }


		public override string ToString()
		{
			return $"{base.ToString()}{Environment.NewLine}Error:{Error.Message}";
		}
	}
	public class ExceptionEvent : SqlCallEvent
	{
		public ExceptionEvent(Exception exception, SqlCallEvent callEvent) : base(callEvent.SqlText, callEvent.CallTime)
		{
			Exception = exception;
			this.Duration = callEvent.Duration;
			this.IsError = true;
		}

		public Exception Exception { get; }


		public override string ToString()
		{
			return $"{base.ToString()}{Environment.NewLine}Error:{Exception.ToString()}";
		}
	}
}
