

using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.consoletelemetry.Extensions;
using starsky.foundation.georealtime.Services;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.webtelemetry.Helpers;

namespace starskykmlimport
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
			
			services.AddMonitoringWorkerService(appSettings, AppSettings.StarskyAppType.Sync);
			services.AddApplicationInsightsLogging(appSettings);

			serviceProvider = services.BuildServiceProvider();
		
			var console = serviceProvider.GetRequiredService<IConsole>();
			var selectorStorage = serviceProvider.GetRequiredService<ISelectorStorage>();

			// Help and other Command Line Tools args are included in the SyncCLI 
			await new KmlImportCli(appSettings, console, selectorStorage).ImportKml(args);
			
			await new FlushApplicationInsights(serviceProvider).FlushAsync();
		}
	}
}
