using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.packagetelemetry.Services;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class PackageTelemetryBackgroundService(IServiceScopeFactory serviceScopeFactory)
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
		using ( var scope = serviceScopeFactory.CreateScope() )
		{
			var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
			var httpClientHelper = scope.ServiceProvider.GetRequiredService<IHttpClientHelper>();
			var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>();
			var query = scope.ServiceProvider.GetRequiredService<IQuery>();
			var deviceIdService = scope.ServiceProvider.GetRequiredService<IDeviceIdService>();
			var lifetimeDiagnosticsService =
				scope.ServiceProvider.GetRequiredService<ILifetimeDiagnosticsService>();

			if ( appSettings.ApplicationType == AppSettings.StarskyAppType.WebController )
			{
				var service = new PackageTelemetry(httpClientHelper, appSettings, logger, query,
					deviceIdService, lifetimeDiagnosticsService);
				await service.PackageTelemetrySend();
			}
		}
	}
}
