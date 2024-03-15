using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;

namespace starskywebhtmlcli
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			// Use args in application
			new ArgsHelper().SetEnvironmentByArgs(args);
			var services = new ServiceCollection();
			services = await SetupAppSettings.FirstStepToAddSingleton(services);

			// Inject services
			RegisterDependencies.Configure(services);
			var serviceProvider = services.BuildServiceProvider();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();
			
			services.AddOpenTelemetryMonitoring(appSettings);
			services.AddTelemetryLogging(appSettings);
			
			new SetupDatabaseTypes(appSettings, services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var publishPreflight = serviceProvider.GetRequiredService<IPublishPreflight>();
			var publishService = serviceProvider.GetRequiredService<IWebHtmlPublishService>();
			var storageSelector = serviceProvider.GetRequiredService<ISelectorStorage>();
			var console = serviceProvider.GetRequiredService<IConsole>();
			var logger = serviceProvider.GetRequiredService<IWebLogger>();

			// Help and args selectors are defined in the PublishCli
			await new PublishCli(storageSelector, publishPreflight, publishService, appSettings,
				console, logger).Publisher(args);
		}
	}
}
