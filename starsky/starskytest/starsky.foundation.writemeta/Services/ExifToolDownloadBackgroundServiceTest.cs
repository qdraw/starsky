using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.writemeta.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Services
{
	[TestClass]
	public class ExifToolDownloadBackgroundServiceTest
	{
		private  readonly IServiceScopeFactory _serviceScopeFactory;

		public ExifToolDownloadBackgroundServiceTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<BackgroundService, ExifToolDownloadBackgroundService>();
			services.AddSingleton<IHttpClientHelper, HttpClientHelper>();
			services.AddSingleton<IHttpProvider, FakeIHttpProvider>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();

			var serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task StartAsync()
		{
			await new ExifToolDownloadBackgroundService(_serviceScopeFactory).StartAsync(new CancellationToken());
		}
	}
}
