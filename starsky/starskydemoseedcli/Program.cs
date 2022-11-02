using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.demo.Services;
using starsky.foundation.consoletelemetry.Extensions;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.webtelemetry.Helpers;

namespace starskydemoseedcli
{
	public static class Program
	{
		public static async Task Main()
		{
			var services = new ServiceCollection();

			// Setup AppSettings
			services = await SetupAppSettings.FirstStepToAddSingleton(services);

			// Inject services
			RegisterDependencies.Configure(services);
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
			
			services.AddMonitoringWorkerService(appSettings, AppSettings.StarskyAppType.DemoSeed);
			services.AddApplicationInsightsLogging(appSettings);
			
			new SetupDatabaseTypes(appSettings,services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

			var webLogger = serviceProvider.GetRequiredService<IWebLogger>();
			var sync = serviceProvider.GetRequiredService<ISynchronize>();
			var httpClientHelper = serviceProvider.GetRequiredService<IHttpClientHelper>();
			
			// Migrations before seeding data
			await RunMigrations.Run(serviceProvider.GetRequiredService<ApplicationDbContext>(), webLogger,appSettings);

			// Help and other Command Line Tools args are NOT included in the tools 
			await CleanDemoDataService.SeedCli(appSettings, sync, 
				selectorStorage.Get(SelectorStorage.StorageServices.SubPath), 
				selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem), 
				webLogger, httpClientHelper );

			await new FlushApplicationInsights(serviceProvider).FlushAsync();
		}
	}
}
