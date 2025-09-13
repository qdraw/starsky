using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.geo.GeoDownload;
using starsky.foundation.geo.GeoDownload.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services;

[TestClass]
public sealed class GeoFileDownloadBackgroundServiceTest
{
	private readonly IGeoFileDownload _geoFileDownload;
	private readonly FakeIWebLogger? _logger;
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public GeoFileDownloadBackgroundServiceTest()
	{
		var services = new ServiceCollection();
		services.AddSingleton<AppSettings>();
		services.AddSingleton<BackgroundService, GeoFileDownloadBackgroundService>();
		services.AddSingleton<IGeoFileDownload, FakeIGeoFileDownload>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();

		var serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		_geoFileDownload = serviceProvider.GetRequiredService<IGeoFileDownload>();

		var logger = serviceProvider.GetRequiredService<IWebLogger>();
		_logger = logger as FakeIWebLogger;
	}

	[TestMethod]
	public async Task StartAsync()
	{
		await new GeoFileDownloadBackgroundService(_serviceScopeFactory).StartAsync(
			new CancellationToken());
		var value = _geoFileDownload as FakeIGeoFileDownload;
		Assert.AreEqual(1, value?.Count);
	}

	[TestMethod]
	public async Task StartAsyncNotAllowedToWriteToDisk()
	{
		var value = _geoFileDownload as FakeIGeoFileDownload;
		value!.Count = int.MaxValue;
		await new GeoFileDownloadBackgroundService(_serviceScopeFactory).StartAsync(
			new CancellationToken());

		Assert.IsTrue(_logger?.TrackedExceptions.LastOrDefault().Item2
			?.Contains("Not allowed to write to disk"));
	}

	[TestMethod]
	public async Task StartAsync_Skip()
	{
		var appSettings = _serviceScopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<AppSettings>();
		var before = appSettings.GeoFilesSkipDownloadOnStartup;
		appSettings.GeoFilesSkipDownloadOnStartup = true;
		await new GeoFileDownloadBackgroundService(_serviceScopeFactory).StartAsync(
			new CancellationToken());
		var value = _geoFileDownload as FakeIGeoFileDownload;
		appSettings.GeoFilesSkipDownloadOnStartup = before;

		Assert.AreEqual(0, value?.Count);
	}

	[TestMethod]
	public async Task StartAsync_Skip2()
	{
		var appSettings = _serviceScopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<AppSettings>();
		var before = appSettings.ApplicationType;
		appSettings.GeoFilesSkipDownloadOnStartup = true;
		await new GeoFileDownloadBackgroundService(_serviceScopeFactory).StartAsync(
			CancellationToken.None);
		var value = _geoFileDownload as FakeIGeoFileDownload;
		appSettings.ApplicationType = before;

		Assert.AreEqual(0, value?.Count);
	}

	[TestMethod]
	[DataRow(AppSettings.StarskyAppType.Geo, DisplayName = "Skip Geo due direct deps")]
	[DataRow(AppSettings.StarskyAppType.Importer, DisplayName = "Skip Importer due direct deps")]
	[DataRow(AppSettings.StarskyAppType.DependenciesDownload,
		DisplayName = "Skip Downloader due direct deps")]
	public async Task StartAsync_Skip3_ApplicationTypeGeoImport(AppSettings.StarskyAppType appType)
	{
		var appSettings = _serviceScopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<AppSettings>();
		var before = appSettings.ApplicationType;
		appSettings.ApplicationType = appType;
		await new GeoFileDownloadBackgroundService(_serviceScopeFactory).StartAsync(
			CancellationToken.None);
		var value = _geoFileDownload as FakeIGeoFileDownload;
		appSettings.ApplicationType = before;

		Assert.AreEqual(0, value?.Count);
	}
}
