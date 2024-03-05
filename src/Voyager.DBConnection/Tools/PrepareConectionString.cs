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
		public virtual String Prepare() => stategy.Prepare();
	}


	public class PrepareConectionStringEmpty : PrepareConectionString
	{
		readonly ConBuilderEmpty stategy;
		public PrepareConectionStringEmpty(DbProviderFactory factory, string sqlConnectionString) : base(factory, sqlConnectionString)
		{
			var conStringBuilder = factory.CreateConnectionStringBuilder();
			stategy = new ConBuilderEmpty(sqlConnectionString);
		}
		public override String Prepare() => stategy.Prepare();
	}
}
