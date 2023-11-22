using System;
using System.Data.Common;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
	class ProcEvent<TResult>
	{
		private readonly IInvokeEvents invokeEnents;
		private SqlCallEvent callEvent;

		public ProcEvent(IInvokeEvents invokeEvent)
		{
			this.invokeEnents = invokeEvent;
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
				this.invokeEnents.Invoke(callEvent);
			}
			catch { }
			return result;
		}

		public void ErrorPublish(Exception ex)
		{
			var errorEvent = new Voyager.DBConnection.Events.ErrorEvent(ex, callEvent);
			errorEvent.Finish();
			this.invokeEnents.Invoke(errorEvent);
		}
	}
}
