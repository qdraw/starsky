using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services
{
	[TestClass]
	public class GeoFileDownloadBackgroundServiceTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IGeoFileDownload _geoFileDownload;

		public GeoFileDownloadBackgroundServiceTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<BackgroundService, GeoFileDownloadBackgroundService>();
			services.AddSingleton<IGeoFileDownload, FakeIGeoFileDownload>();

			var serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			_geoFileDownload = serviceProvider.GetRequiredService<IGeoFileDownload>();
		}
		
		[TestMethod]
		public async Task StartAsync()
		{
			await new GeoFileDownloadBackgroundService(_serviceScopeFactory).StartAsync(new CancellationToken());
			var value = _geoFileDownload as FakeIGeoFileDownload;
			Assert.AreEqual(1, value.Count);
		}
	}
}
