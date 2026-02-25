using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starskyDependenciesDownloadCli;

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
		// need to set because it skips geo download background service
		appSettings.ApplicationType = AppSettings.StarskyAppType.DependenciesDownload;

		services.AddOpenTelemetryMonitoring(appSettings);
		services.AddTelemetryLogging(appSettings);

		new SetupDatabaseTypes(appSettings, services).BuilderDb();
		serviceProvider = services.BuildServiceProvider();

		var exifToolDownload = serviceProvider.GetRequiredService<IExifToolDownload>();
		var logger = serviceProvider.GetRequiredService<IWebLogger>();
		var geoFileDownload = serviceProvider.GetRequiredService<IGeoFileDownload>();
		var ffMpegDownload = serviceProvider.GetRequiredService<IFfMpegDownload>();
		var imageOptimisationToolDownload =
			serviceProvider.GetRequiredService<IImageOptimisationToolDownload>();

		// Migrations before geo-tools (not needed for this specific app, but helps the process)
		await RunMigrations.Run(serviceProvider.GetRequiredService<ApplicationDbContext>(),
			logger, appSettings);

		await geoFileDownload.DownloadAsync();

		var runtimes = ArgsHelper.GetRuntimes([.. args]);
		logger.LogInformation($"Runtimes: {string.Join(", ", runtimes)}");

		await ffMpegDownload.DownloadFfMpeg(runtimes);
		await exifToolDownload.DownloadExifTool(runtimes);

		await exifToolDownload.DownloadExifTool(runtimes);

		await imageOptimisationToolDownload.Download(MozJpegDownload.Options, runtimes);
	}
}
