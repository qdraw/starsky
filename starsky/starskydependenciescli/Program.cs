using System.Threading.Tasks;

namespace starskyDependenciesCli
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

			services.AddOpenTelemetryMonitoring(appSettings);
			services.AddTelemetryLogging(appSettings);

			new SetupDatabaseTypes(appSettings, services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var geoReverseLookup = serviceProvider.GetRequiredService<IGeoReverseLookup>();
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
}
