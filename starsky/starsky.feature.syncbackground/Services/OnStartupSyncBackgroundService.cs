using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.sync.SyncInterfaces;

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
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>();
				await synchronize.Sync("/");

			}
		}
	}
}
