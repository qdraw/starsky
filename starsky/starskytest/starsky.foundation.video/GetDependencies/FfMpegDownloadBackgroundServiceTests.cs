using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.GetDependencies.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public sealed class FfMpegDownloadBackgroundServiceTests
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public FfMpegDownloadBackgroundServiceTests()
	{
		var services = new ServiceCollection();
		services.AddSingleton(new AppSettings { FfmpegSkipDownloadOnStartup = true });
		services.AddSingleton<BackgroundService, FfMpegDownloadBackgroundService>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddSingleton<IFfMpegDownloadIndex, FakeIFfMpegDownloadIndex>();
		services.AddSingleton<IFfMpegDownloadBinaries, FakeIFfMpegDownloadBinaries>();
		services.AddSingleton<IFfMpegPrepareBeforeRunning, FakeIFfMpegPrepareBeforeRunning>();

		var serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task StartAsync()
	{
		var cancelToken = new CancellationToken();
		await new FfMpegDownloadBackgroundService(_serviceScopeFactory).StartAsync(cancelToken);
		Assert.IsNotNull(cancelToken);
	}
}
