using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starskyGeoCli;

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

		var geoReverseLookup = serviceProvider.GetRequiredService<IGeoFolderReverseLookup>();
		var geoLocationWrite = serviceProvider.GetRequiredService<IGeoLocationWrite>();
		var geoFileDownload = serviceProvider.GetRequiredService<IGeoFileDownload>();

		var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

		var console = serviceProvider.GetRequiredService<IConsole>();
		var exifToolDownload = serviceProvider.GetRequiredService<IExifToolDownload>();
		var logger = serviceProvider.GetRequiredService<IWebLogger>();

		// Migrations before geo-tools (not needed for this specific app, but helps the process)
		await RunMigrations.Run(serviceProvider.GetRequiredService<ApplicationDbContext>(),
			logger, appSettings);

		// Help and other Command Line Tools args are included in the Geo tools 
		await new GeoCli(geoReverseLookup, geoLocationWrite, selectorStorage,
				appSettings, console, geoFileDownload, exifToolDownload, logger)
			.CommandLineAsync(args);
	}
}
