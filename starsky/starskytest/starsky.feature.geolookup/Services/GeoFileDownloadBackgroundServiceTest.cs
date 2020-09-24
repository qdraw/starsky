using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services
{
	[TestClass]
	public class GeoFileDownloadBackgroundServiceTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IGeoFileDownload _geoFileDownload;
		private readonly FakeConsoleWrapper _console;

		public GeoFileDownloadBackgroundServiceTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<BackgroundService, GeoFileDownloadBackgroundService>();
			services.AddSingleton<IGeoFileDownload, FakeIGeoFileDownload>();
			services.AddSingleton<IConsole, FakeConsoleWrapper>();

			var serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			_geoFileDownload = serviceProvider.GetRequiredService<IGeoFileDownload>();
			
			var console = serviceProvider.GetRequiredService<IConsole>();
			_console = console as FakeConsoleWrapper;
		}
		
		[TestMethod]
		public async Task StartAsync()
		{
			await new GeoFileDownloadBackgroundService(_serviceScopeFactory).StartAsync(new CancellationToken());
			var value = _geoFileDownload as FakeIGeoFileDownload;
			Assert.AreEqual(1, value.Count);
		}
		
		[TestMethod]
		public async Task StartAsyncNotAllowedToWriteToDisk()
		{
			var value = _geoFileDownload as FakeIGeoFileDownload;
			value.Count = int.MaxValue;
			await new GeoFileDownloadBackgroundService(_serviceScopeFactory).StartAsync(new CancellationToken());
		
			Assert.IsTrue(_console.WrittenLines.LastOrDefault().Contains("Not allowed to write to disk"));
		}
	}
}
