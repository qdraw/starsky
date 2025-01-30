using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class FfMpegDownloadBackgroundService(IServiceScopeFactory serviceScopeFactory)
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
		using var scope = serviceScopeFactory.CreateScope();
		var selectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
		var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
		var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>();
		var downloadIndex = scope.ServiceProvider.GetRequiredService<IFfMpegDownloadIndex>();
		var downloadBinaries =
			scope.ServiceProvider.GetRequiredService<IFfMpegDownloadBinaries>();
		var prepareBeforeRunning =
			scope.ServiceProvider.GetRequiredService<IFfMpegPrepareBeforeRunning>();
		var preflightBeforeRunning =
			scope.ServiceProvider.GetRequiredService<IFfMpegPreflightRunCheck>();

		var service = new FfMpegDownload(selectorStorage, appSettings, logger, downloadIndex,
			downloadBinaries, prepareBeforeRunning, preflightBeforeRunning);
		await service.DownloadFfMpeg();
	}
}
