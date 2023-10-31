using System;
using System.Data.Common;
using System.Threading;

namespace Voyager.DBConnection.Tools
{
	public class PrepareConectionString
	{
		DbConnectionStringBuilder conStringBuilder;
		public PrepareConectionString(DbProviderFactory factory, string sqlConnectionString)
		{
			conStringBuilder = factory.CreateConnectionStringBuilder();
			// INFO pomiń przypadki testowe
			if (sqlConnectionString.Length > 5)
				conStringBuilder.ConnectionString = sqlConnectionString;
		}

		const string ApplicationName = "Application Name";
		public String Prepare()
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
