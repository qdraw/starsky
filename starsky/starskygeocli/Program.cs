using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Services;
using starsky.foundation.consoletelemetry.Extensions;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starskyGeoCli
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
			
			services.AddMonitoringWorkerService(appSettings, AppSettings.StarskyAppType.Geo);
			services.AddApplicationInsightsLogging(appSettings);
			
			new SetupDatabaseTypes(appSettings,services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var geoReverseLookup = serviceProvider.GetService<IGeoReverseLookup>();
			var geoLocationWrite = serviceProvider.GetRequiredService<IGeoLocationWrite>();
			var geoFileDownload = serviceProvider.GetRequiredService<IGeoFileDownload>();

			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

			var console = serviceProvider.GetRequiredService<IConsole>();
			var exifToolDownload = serviceProvider.GetRequiredService<IExifToolDownload>();

			// Help and other Command Line Tools args are included in the Geo tools 
			await new GeoCli(geoReverseLookup, geoLocationWrite, selectorStorage,
				appSettings, console, geoFileDownload, exifToolDownload).CommandLineAsync(args);

			await new FlushApplicationInsights(serviceProvider).FlushAsync();
		}
	}
}
