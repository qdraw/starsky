using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Services;
using starsky.foundation.import.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.mountwatch.ServiceInstaller;
using starsky.foundation.mountwatch.Services;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;

namespace starskymountwatchercli;

public static class Program
{
	[SuppressMessage("Design",
		"ASP0000:Do not call \'IServiceCollection.BuildServiceProvider\' in \'ConfigureServices\'")]
	public static async Task Main(string[] args)
	{
		// Use args in the application
		new ArgsHelper().SetEnvironmentByArgs(args);

		var hostBuilder = Host.CreateDefaultBuilder(args);

		if ( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			hostBuilder.UseWindowsService();
		}

		hostBuilder.ConfigureServices((_, services) =>
		{
			// Setup AppSettings
			var configurationRoot = SetupAppSettings.AppSettingsToBuilder(args).Result;
			services.AddSingleton<IConfiguration>(configurationRoot);
			services.ConfigurePoCo<AppSettings>(configurationRoot.GetSection("App"));

			// Inject services
			RegisterDependencies.Configure(services);
			services.AddSingleton<IHttpClientHelper, HttpClientHelper>();

			services.AddMemoryCache();
			services.AddHealthChecks();
			services.AddScoped<ISelectorStorage, SelectorStorage>();

			var tempServiceProvider = services.BuildServiceProvider();
			var appSettings = tempServiceProvider.GetRequiredService<AppSettings>();

			services.AddOpenTelemetryMonitoring(appSettings);
			services.AddTelemetryLogging(appSettings);

			new SetupDatabaseTypes(appSettings, services).BuilderDb();
		});

		var host = hostBuilder.Build();
		var serviceProvider = host.Services;
		var appSettings = serviceProvider.GetRequiredService<AppSettings>();

		using var scope = serviceProvider.CreateScope();
		var scopedServiceProvider = scope.ServiceProvider;

		var import = scopedServiceProvider.GetRequiredService<IImport>();
		var console = scopedServiceProvider.GetRequiredService<IConsole>();
		var webLogger = scopedServiceProvider.GetRequiredService<IWebLogger>();
		var cameraStorageDetector = scopedServiceProvider.GetRequiredService<ICameraStorageDetector>();
		var mountWatcherFactory = scopedServiceProvider.GetRequiredService<IMountWatcherFactory>();
		var serviceInstaller = new ServiceInstaller(webLogger);

		// Migrations before starting
		await RunMigrations.Run(scopedServiceProvider.GetRequiredService<ApplicationDbContext>(),
			webLogger,
			appSettings);

		var service = new MountWatcherCli(
			import,
			appSettings,
			console,
			webLogger,
			cameraStorageDetector,
			mountWatcherFactory,
			serviceInstaller);

		if ( !await service.StartWatcher(args) )
		{
			// StartWatcher returns false when it fails, but not when it's an install/uninstall
			// Unless the install/uninstall operation itself failed.
			webLogger.LogError("Mount watcher failed to start or install. See logs for details.");
		}

		if ( !MountWatcherCli.NeedInstall(args) && !MountWatcherCli.NeedUninstall(args) &&
		     !ArgsHelper.NeedHelp(args) )
		{
			await host.RunAsync();
		}
	}
}
