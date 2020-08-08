using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Middleware;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starskycore.Interfaces;
using starskycore.Services;

namespace starskysynccli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
	        // Use args in application
	        new ArgsHelper().SetEnvironmentByArgs(args);

	        // Setup AppSettings
	        var services = SetupAppSettings.FirstStepToAddSingleton(new ServiceCollection());

	        // Inject services
	        new RegisterDependencies().Configure(services);
	        var serviceProvider = services.BuildServiceProvider();
	        var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
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
