using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class PeriodicThumbnailScanHostedServiceTest
{
	[TestMethod]
	[Timeout(5000)]
	public async Task StartBackgroundAsync_Cancel()
	{
		var services = new ServiceCollection();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(new AppSettings(),
			new FakeIWebLogger(),
			scopeFactory);
		var cancelToken = new CancellationTokenSource();
		cancelToken.Cancel();
		
		await periodicThumbnailScanHostedService.StartBackgroundAsync(
			cancelToken.Token);
	}
}
