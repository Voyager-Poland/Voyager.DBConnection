using System;
using System.Data.Common;

namespace Voyager.DBConnection.Events
{
	public class SqlCallEvent
	{

		public SqlCallEvent(String sqlText, DateTime dateTime)
		{
			this.SqlText = sqlText;
			this.CallTime = dateTime;
		}

		public void Finish()
		{
			this.Duration = DateTime.Now - this.CallTime;
		}

		internal static SqlCallEvent Create(DbCommand command)
		{
			return new SqlCallEvent(command.CommandText, DateTime.Now);
		}

		public string SqlText { get; }
		public DateTime CallTime { get; }
		public TimeSpan Duration { get; protected set; }


		public override string ToString()
		{
			return $"{CallTime}; {SqlText}; Duration: {Duration}";
		}

		public bool IsError { get; protected set; }
	}
}
