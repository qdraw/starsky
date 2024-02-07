using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.webtelemetry.Helpers;

namespace starskythumbnailmetacli
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

			services.AddTelemetryLogging(appSettings);

			new SetupDatabaseTypes(appSettings, services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();
			var console = serviceProvider.GetRequiredService<IConsole>();
			var metaExifThumbnailService =
				serviceProvider.GetRequiredService<IMetaExifThumbnailService>();
			var statusThumbnailService =
				serviceProvider.GetRequiredService<IMetaUpdateStatusThumbnailService>();
			var webLogger = serviceProvider.GetRequiredService<IWebLogger>();

			// Migrations before update db afterwards
			await RunMigrations.Run(serviceProvider.GetRequiredService<ApplicationDbContext>(),
				webLogger,
				appSettings);

			// Help and other Command Line Tools args are included in the MetaThumbnail tools 
			var cmdLineTool = new MetaThumbnailCommandLineHelper(
				selectorStorage, appSettings, console, metaExifThumbnailService,
				statusThumbnailService);
			await cmdLineTool.CommandLineAsync(args);
		}
	}
}
