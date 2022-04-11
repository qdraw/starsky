using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.feature.realtime.Interface;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starsky.feature.realtime.Services
{

	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class CleanUpConnectionBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public CleanUpConnectionBackgroundService(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}

		/// <summary>
		/// Running scoped services
		/// @see: https://thinkrethink.net/2018/07/12/injecting-a-scoped-service-into-ihostedservice/
		/// </summary>
		/// <param name="stoppingToken">Cancellation Token, but it ignored</param>
		/// <returns>CompletedTask</returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var connectionsService = scope.ServiceProvider.GetRequiredService<IRealtimeConnectionsService>();
				
				//exception is already catch-ed in the service
				await connectionsService.CleanOldMessagesAsync();
			}
		}
	}
}
