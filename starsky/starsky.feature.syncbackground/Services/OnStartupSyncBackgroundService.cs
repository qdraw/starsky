using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherBackgroundService;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.syncbackground.Services;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public class OnStartupSyncBackgroundService(
	IServiceScopeFactory serviceScopeFactory,
	ITenantRegistry? tenantRegistry = null)
	: BackgroundService
{
	/// <summary>
	///     Running scoped services
	/// </summary>
	/// <param name="stoppingToken">Cancellation Token, but it ignored</param>
	/// <returns>CompletedTask</returns>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await StartAsync();
	}

	internal async Task StartAsync()
	{
		using var scope = serviceScopeFactory.CreateScope();
		var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();

		if ( appSettings.ApplicationType != AppSettings.StarskyAppType.WebController )
		{
			return;
		}

		// one sync job per tenant: each has its own storage folder, index and settings
		await TenantJobs.ForEachActiveTenantAsync(serviceScopeFactory,
			tenantRegistry ?? new TenantRegistry(),
			async (serviceProvider, _) =>
			{
				var diskWatcherBackgroundTaskQueue = serviceProvider
					.GetRequiredService<IDiskWatcherBackgroundTaskQueue>();

				var synchronize = serviceProvider.GetRequiredService<ISynchronize>();
				var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
				var logger = serviceProvider.GetRequiredService<IWebLogger>();

				await new OnStartupSync(serviceScopeFactory, diskWatcherBackgroundTaskQueue,
					appSettings, synchronize, settingsService, logger).CreateJobAsync();
			});
	}
}
