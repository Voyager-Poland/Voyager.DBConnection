using System;
using System.Data.Common;
using System.Threading;

namespace Voyager.DBConnection.Tools
{
    /// <summary>
    /// Provides utilities for preparing database connection strings.
    /// </summary>
    public static class ConnectionStringHelper
    {
        /// <summary>
        /// Prepares a connection string by adding the current user's identity to the Application Name parameter.
        /// </summary>
        /// <param name="factory">The database provider factory used to create the connection string builder.</param>
        /// <param name="connectionString">The base connection string to prepare.</param>
        /// <param name="userName">Optional user name to use instead of Thread.CurrentPrincipal. Useful for testing.</param>
        /// <returns>The prepared connection string with Application Name set, or the original string if preparation fails.</returns>
        public static string PrepareConnectionString(
            DbProviderFactory factory,
            string connectionString,
            string userName = null)
        {
            if (string.IsNullOrEmpty(connectionString) || connectionString.Length <= 5)
                return connectionString;

            var builder = factory?.CreateConnectionStringBuilder();
            if (builder == null)
                return connectionString;

            builder.ConnectionString = connectionString;

            var appName = userName ?? Thread.CurrentPrincipal?.Identity?.Name;
            if (!string.IsNullOrEmpty(appName))
            {
                const string ApplicationNameKey = "Application Name";
                if (builder.ContainsKey(ApplicationNameKey))
                    builder[ApplicationNameKey] = $"{builder[ApplicationNameKey]} {appName}";
                else
                    builder.Add(ApplicationNameKey, appName);
            }

            return builder.ToString();
        }
    }
}
