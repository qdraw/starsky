using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.Middleware;
using starsky.Models;

namespace starsky.Helpers
{
    public class ConfigCliAppsStartupHelper
    {

        // Only for CLI apps
        
        public ConfigCliAppsStartupHelper()
        {
            // Depencency Injection for configuration
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            var builder = new ConfigurationBuilder()
                .AddJsonFile(
                    AppSettings().BaseDirectoryProject  + 
                    Path.DirectorySeparatorChar + "appsettings.json", optional: false)
                .AddEnvironmentVariables();
            
            var configuration = builder.Build();
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            _serviceProvider = services.BuildServiceProvider();
            // End of Depencency Injection for configuration
        }
        
        private readonly IServiceProvider _serviceProvider;

        public AppSettings AppSettings()
        {
            return _serviceProvider.GetRequiredService<AppSettings>();
        }
            
    }
}