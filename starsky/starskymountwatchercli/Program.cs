using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.import.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.mountwatch.Services;
using starsky.foundation.platform.Exceptions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starskymountwatchercli;

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

		services.AddOpenTelemetryMonitoring(appSettings);
		services.AddTelemetryLogging(appSettings);

		new SetupDatabaseTypes(appSettings, services).BuilderDb();
		serviceProvider = services.BuildServiceProvider();

		var import = serviceProvider.GetRequiredService<IImport>();
		var console = serviceProvider.GetRequiredService<IConsole>();
		var exifToolDownload = serviceProvider.GetRequiredService<IExifToolDownload>();
		var webLogger = serviceProvider.GetRequiredService<IWebLogger>();
		var geoFileDownload = serviceProvider.GetRequiredService<IGeoFileDownload>();
		var mountDetector = serviceProvider.GetRequiredService<IMountDetector>();
		var mountWatcherFactory = serviceProvider.GetRequiredService<IMountWatcherFactory>();
		var cameraStorageDetector = serviceProvider.GetRequiredService<ICameraStorageDetector>();

		// Migrations before starting
		await RunMigrations.Run(serviceProvider.GetRequiredService<ApplicationDbContext>(),
			webLogger,
			appSettings);

		var service = new MountWatcherCli(
			import,
			appSettings,
			console,
			webLogger,
			exifToolDownload,
			geoFileDownload,
			mountDetector,
			mountWatcherFactory,
			cameraStorageDetector);

		if ( !await service.StartWatcher(args) )
		{
			throw new WebApplicationException("Mount watcher failed to start");
		}
	}
}

