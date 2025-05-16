using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.writemeta.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Services;

[TestClass]
public sealed class ExifToolDownloadBackgroundServiceTest
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public ExifToolDownloadBackgroundServiceTest()
	{
		var services = new ServiceCollection();
		services.AddSingleton<AppSettings>();
		services.AddSingleton<BackgroundService, ExifToolDownloadBackgroundService>();
		services.AddSingleton<IHttpClientHelper, HttpClientHelper>();
		services.AddSingleton<IHttpProvider, FakeIHttpProvider>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();

		var serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task StartAsync()
	{
		var cancelToken = CancellationToken.None;
		await new ExifToolDownloadBackgroundService(_serviceScopeFactory).StartAsync(cancelToken);

		var logger = _serviceScopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<IWebLogger>();
		Assert.IsNotNull(logger);
	}
}
