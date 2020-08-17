using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.feature.geolookup.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;

namespace starsky.feature.geolookup.Services
{

	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class GeoFileDownloadBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public GeoFileDownloadBackgroundService(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}

		/// <summary>
		/// Running scoped services
		/// @see: https://thinkrethink.net/2018/07/12/injecting-a-scoped-service-into-ihostedservice/
		/// </summary>
		/// <param name="stoppingToken">Cancellation Token, but it ignored</param>
		/// <returns>CompletedTask</returns>
#pragma warning disable 1998
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
#pragma warning restore 1998
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
				// Geo Helper has a direct need of this, other are downloaded when needed
				// This Background service is for running offline 
				if ( appSettings.ApplicationType == AppSettings.StarskyAppType.Geo ) return;
				new GeoFileDownload(appSettings).Download();
			}
		}
	}
}
