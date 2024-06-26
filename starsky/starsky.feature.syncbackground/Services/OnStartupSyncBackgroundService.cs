using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.feature.syncbackground.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherBackgroundService;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.syncbackground.Services
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class OnStartupSyncBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public OnStartupSyncBackgroundService(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}

		/// <summary>
		/// Running scoped services
		/// </summary>
		/// <param name="stoppingToken">Cancellation Token, but it ignored</param>
		/// <returns>CompletedTask</returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
			var diskWatcherBackgroundTaskQueue = scope.ServiceProvider
				.GetRequiredService<IDiskWatcherBackgroundTaskQueue>();

			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>();
			var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
			var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>();

			await new OnStartupSync(_serviceScopeFactory, diskWatcherBackgroundTaskQueue,
				appSettings, synchronize, settingsService, logger).StartUpSync();
		}
	}
}
