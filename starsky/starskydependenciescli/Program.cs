using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.externaldependencies;
using starsky.feature.externaldependencies.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;

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
			services.AddScoped<IExternalDependenciesService, ExternalDependenciesService>();

			new SetupDatabaseTypes(appSettings, services).BuilderDb();
			serviceProvider = services.BuildServiceProvider();

			var dependenciesService =
				serviceProvider.GetRequiredService<IExternalDependenciesService>();

			await dependenciesService.SetupAsync(args);
		}
	}
}
