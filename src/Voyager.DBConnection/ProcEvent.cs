using System;
using System.Data.Common;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	class ProcEvent<TResult>
	{
		private readonly IInvokeEvents invokeEvents;
		private SqlCallEvent callEvent;

		public ProcEvent(IInvokeEvents invokeEvent)
		{
			this.invokeEvents = invokeEvent;
			callEvent = new SqlCallEvent("connection", DateTime.Now);
		}

		public TResult Call(Func<TResult> function, DbCommand command)
		{
			callEvent = Voyager.DBConnection.Events.SqlCallEvent.Create(command);
			TResult result;
			try
			{
				result = function.Invoke();
			}
			finally
			{
				callEvent.Finish();
			}
			try
			{
				this.Invoke(callEvent);
			}
			catch { }
			return result;
		}
		protected void Invoke(SqlCallEvent sqlCallEvent)
		{
			this.invokeEvents.Invoke(sqlCallEvent);
		}

		public void ExceptionPublish(Exception ex)
		{
			var errorEvent = new Voyager.DBConnection.Events.ExceptionEvent(ex, callEvent);
			errorEvent.Finish();
			this.Invoke(errorEvent);
		}

		public void ErrorPublish(Common.Results.Error error)
		{
			var errorEvent = new Voyager.DBConnection.Events.CommonErrorEvent(error, callEvent);
			errorEvent.Finish();
			this.Invoke(errorEvent);
		}
	}
}
