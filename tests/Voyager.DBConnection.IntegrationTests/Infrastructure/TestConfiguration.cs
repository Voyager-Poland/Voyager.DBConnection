using Microsoft.Extensions.Configuration;

namespace Voyager.DBConnection.IntegrationTests.Infrastructure;

public static class TestConfiguration
{
    private static IConfiguration? _configuration;

    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddEnvironmentVariables()
                    .Build();
            }
            return _configuration;
        }
    }

    public static string GetConnectionString(DatabaseProvider provider)
    {
        var connectionString = Configuration.GetConnectionString(provider.ToString());
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string for {provider} not found in configuration");
        }
        return connectionString;
    }

    public static string GetProviderName(DatabaseProvider provider)
    {
        var providerName = Configuration[$"DbProviderFactories:{provider}"];
        if (string.IsNullOrEmpty(providerName))
        {
            throw new InvalidOperationException($"Provider name for {provider} not found in configuration");
        }
        return providerName;
    }
}

public enum DatabaseProvider
{
    SqlServer,
    PostgreSQL,
    MySQL,
    Oracle,
    SQLite
}
