using System;
using System.Data.Common;

namespace Voyager.DBConnection.Tools
{
	public class PrepareConectionString
	{
		readonly ConBuilderEmpty stategy;
		public PrepareConectionString(DbProviderFactory factory, string sqlConnectionString)
		{
			var conStringBuilder = factory.CreateConnectionStringBuilder();
			stategy = conStringBuilder != null ? new ConBuilderProvider(conStringBuilder, sqlConnectionString) : new ConBuilderEmpty(sqlConnectionString);
		}
		public String Prepare() => stategy.Prepare();
	}
}
