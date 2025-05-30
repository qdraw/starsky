using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.import.Interfaces;
using starsky.feature.import.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Exceptions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starskyimportercli;

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

		// Migrations before importing
		await RunMigrations.Run(serviceProvider.GetRequiredService<ApplicationDbContext>(),
			webLogger,
			appSettings);


		// Help and other Command Line Tools args are included in the ImporterCli 
		var service = new ImportCli(import, appSettings, console, webLogger, exifToolDownload,
			geoFileDownload);
		if ( !await service.Importer(args) )
		{
			throw new WebApplicationException("Import failed");
		}
	}
}
