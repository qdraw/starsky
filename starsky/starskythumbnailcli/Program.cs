using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.Helpers;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starskythumbnailcli
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);

			var services = new ServiceCollection();

			// Setup AppSettings
			services = SetupAppSettings.FirstStepToAddSingleton(services);

			// Inject services
			new RegisterDependencies().Configure(services);
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
			// new SetupDatabaseTypes(appSettings,services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var thumbnailService = serviceProvider.GetService<IThumbnailService>();
			var thumbnailCleaner = serviceProvider.GetService<IThumbnailCleaner>();

			var console = serviceProvider.GetRequiredService<IConsole>();
			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

			// Help and other Command Line Tools args are included in the ThumbnailCLI
			new ThumbnailCli( appSettings, console, thumbnailService, thumbnailCleaner, selectorStorage).Thumbnail(args);
		}
	}
}
