using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.consoletelemetry.Extensions;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.Helpers;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.webtelemetry.Helpers;

namespace starskythumbnailcli
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);

			var services = new ServiceCollection();

			// Setup AppSettings
			services = await SetupAppSettings.FirstStepToAddSingleton(services);

			// Inject services
			RegisterDependencies.Configure(services);
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            			
			services.AddMonitoringWorkerService(appSettings, AppSettings.StarskyAppType.Thumbnail);
			services.AddApplicationInsightsLogging(appSettings);
			
			new SetupDatabaseTypes(appSettings,services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var thumbnailService = serviceProvider.GetService<IThumbnailService>();
			var thumbnailCleaner = serviceProvider.GetService<IThumbnailCleaner>();

			var console = serviceProvider.GetRequiredService<IConsole>();
			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

			// Help and other Command Line Tools args are included in the ThumbnailCLI
			await new ThumbnailCli( appSettings, console, thumbnailService, thumbnailCleaner, selectorStorage).Thumbnail(args); 
			
			await new FlushApplicationInsights(serviceProvider).FlushAsync();
		}
	}
}
