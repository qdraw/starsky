using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Services;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.Helpers;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;

namespace starskythumbnailcli;

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
		services.AddSingleton<IHttpClientHelper, HttpClientHelper>();

		var serviceProvider = services.BuildServiceProvider();
		var appSettings = serviceProvider.GetRequiredService<AppSettings>();

		services.AddOpenTelemetryMonitoring(appSettings);
		services.AddTelemetryLogging(appSettings);

		new SetupDatabaseTypes(appSettings, services).BuilderDb();
		serviceProvider = services.BuildServiceProvider();

		var thumbnailService = serviceProvider.GetRequiredService<IThumbnailService>();
		var thumbnailCleaner = serviceProvider.GetRequiredService<IThumbnailCleaner>();

		var console = serviceProvider.GetRequiredService<IConsole>();
		var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();
		var logger = serviceProvider.GetRequiredService<IWebLogger>();

		// Help and other Command Line Tools args are included in the ThumbnailCLI
		var thumbnailCli = new ThumbnailCli(appSettings, console,
			thumbnailService, thumbnailCleaner, selectorStorage, logger);
		await thumbnailCli.Thumbnail(args);
	}
}
