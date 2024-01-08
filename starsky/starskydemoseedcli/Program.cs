using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.demo.Helpers;
using starsky.foundation.consoletelemetry.Extensions;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.webtelemetry.Helpers;

namespace starskydemoseedcli
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
			
			services.AddMonitoringWorkerService(appSettings, AppSettings.StarskyAppType.DemoSeed);
			services.AddTelemetryLogging(appSettings);
			
			new SetupDatabaseTypes(appSettings,services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

			var webLogger = serviceProvider.GetRequiredService<IWebLogger>();
			var sync = serviceProvider.GetRequiredService<ISynchronize>();
			var httpClientHelper = serviceProvider.GetRequiredService<IHttpClientHelper>();
			var console = serviceProvider.GetRequiredService<IConsole>();

			// Migrations before seeding data
			await RunMigrations.Run(serviceProvider.GetRequiredService<ApplicationDbContext>(), webLogger,appSettings);

			// Help and Command Line Tools args are included in the tools 
			var cleanDemoDataServiceCli = new CleanDemoDataServiceCli(
				appSettings,
				httpClientHelper,
				selectorStorage,
				webLogger,
				console,
				sync);
			
			await cleanDemoDataServiceCli.SeedCli(args);

			await new FlushApplicationInsights(serviceProvider).FlushAsync();
		}
	}
}
