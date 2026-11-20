using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.feature.realtime.Interface;
using starsky.foundation.injection;
using starsky.foundation.platform.Tenancy;

namespace starsky.feature.realtime.Services;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public class CleanUpConnectionBackgroundService(
	IServiceScopeFactory serviceScopeFactory,
	ITenantRegistry? tenantRegistry = null)
	: BackgroundService
{
	/// <summary>
	///     Running scoped services
	///     @see: https://thinkrethink.net/2018/07/12/injecting-a-scoped-service-into-ihostedservice/
	/// </summary>
	/// <param name="stoppingToken">Cancellation Token, but it ignored</param>
	/// <returns>CompletedTask</returns>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// the notifications table has rows of every tenant
		await TenantJobs.ForEachActiveTenantAsync(serviceScopeFactory,
			tenantRegistry ?? new TenantRegistry(),
			async (serviceProvider, _) =>
			{
				var connectionsService =
					serviceProvider.GetRequiredService<IRealtimeConnectionsService>();

				//exception is already catch-ed in the service
				await connectionsService.CleanOldMessagesAsync();
			}, cancellationToken: CancellationToken.None);
	}
}
