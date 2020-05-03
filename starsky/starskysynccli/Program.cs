using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.import.Interfaces;
using starsky.feature.import.Services;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Middleware;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Services;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskysynccli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
	        // Use args in application
	        new ArgsHelper().SetEnvironmentByArgs(args);

	        var services = new ServiceCollection();

	        // Setup AppSettings
	        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
	        var configurationRoot = SetupAppSettings.AppSettingsToBuilder();
	        services.ConfigurePoCo<AppSettings>(configurationRoot.GetSection("App"));

	        // Inject services
	        new RegisterDependencies().Configure(services);
	        var serviceProvider = services.BuildServiceProvider();
	        var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
	        appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

	        new SetupDatabaseTypes(appSettings,services).BuilderDb();
	        serviceProvider = services.BuildServiceProvider();

	        var syncService = serviceProvider.GetService<ISync>();
	        var console = serviceProvider.GetRequiredService<IConsole>();
			var thumbnailCleaner = serviceProvider.GetRequiredService<IThumbnailCleaner>();
			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

            new SyncServiceCli().Sync(args, syncService, appSettings,console, thumbnailCleaner, selectorStorage);
        }

    }
}
