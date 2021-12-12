using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.sync.Helpers;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;

namespace starskysynchronizecli
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
			new RegisterDependencies().Configure(services);
			
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();

			services.AddMonitoring(appSettings);
			services.AddApplicationInsightsLogging(appSettings);

			new SetupDatabaseTypes(appSettings,services).BuilderDb();
				
			serviceProvider = services.BuildServiceProvider();
			
			var synchronize = serviceProvider.GetService<ISynchronize>();
			var console = serviceProvider.GetRequiredService<IConsole>();
			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

			var logger = serviceProvider.GetRequiredService<IWebLogger>();
			logger.LogInformation("Logger is working...");

			// Help and other Command Line Tools args are included in the SyncCLI 
			await new SyncCli(synchronize, appSettings, console, selectorStorage).Sync(args);
		}
	}
}
