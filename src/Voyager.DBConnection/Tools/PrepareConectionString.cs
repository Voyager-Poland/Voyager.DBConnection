using System;
using System.Data.Common;
using System.Threading;

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

	class ConBuilderEmpty
	{
		private readonly string sqlConnectionString;

		public ConBuilderEmpty(string sqlConnectionString)
		{
			this.sqlConnectionString = sqlConnectionString;
		}
		public virtual string Prepare() => sqlConnectionString;
	}

	class ConBuilderProvider : ConBuilderEmpty
	{
		private readonly DbConnectionStringBuilder conStringBuilder;

		public ConBuilderProvider(DbConnectionStringBuilder builder, string sqlConnectionString) : base(sqlConnectionString)
		{
			this.conStringBuilder = builder;
			if (sqlConnectionString.Length > 5)
				conStringBuilder.ConnectionString = sqlConnectionString;
		}

		const string ApplicationName = "Application Name";
		public override String Prepare()
		{
			if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null)
			{
				if (conStringBuilder.ContainsKey(ApplicationName))
					conStringBuilder[ApplicationName] += " " + Thread.CurrentPrincipal.Identity.Name;
				else
#pragma warning disable CS8604
#pragma warning disable CS8602
					conStringBuilder.Add(ApplicationName, Thread.CurrentPrincipal.Identity.Name);
#pragma warning restore CS8602
#pragma warning restore CS8604
			}
			return conStringBuilder.ToString();
		}
	}
}
