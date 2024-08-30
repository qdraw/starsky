using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Services.ExifToolDownloader;

namespace starsky.foundation.writemeta.Services
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class ExifToolDownloadBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ExifToolDownloadBackgroundService(IServiceScopeFactory serviceScopeFactory)
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
			using ( var scope = _serviceScopeFactory.CreateScope() )
			{
				var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
				var httpClientHelper =
					scope.ServiceProvider.GetRequiredService<IHttpClientHelper>();
				var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>();
				await new ExifToolDownload(httpClientHelper, appSettings, logger).DownloadExifTool(
					appSettings.IsWindows);
			}
		}
	}
}
