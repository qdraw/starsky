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

namespace starskysynclegacycli
{
	static class Program
	{
		static async Task Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);

			var services = new ServiceCollection();

			// Setup AppSettings
			services = SetupAppSettings.FirstStepToAddSingleton(services);

			// Inject services
			new RegisterDependencies().Configure(services);
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            
			new SetupDatabaseTypes(appSettings,services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var synchronize = serviceProvider.GetService<ISynchronize>();
			var console = serviceProvider.GetRequiredService<IConsole>();
			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

			// Help and other Command Line Tools args are included in the SyncCLI 
			await new SyncCli(synchronize, appSettings, console, selectorStorage).Sync(args);
		}
	}
}
