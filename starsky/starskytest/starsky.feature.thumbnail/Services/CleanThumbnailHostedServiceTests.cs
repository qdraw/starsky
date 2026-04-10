using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Formats;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class CleanThumbnailHostedServiceTest
{
	public TestContext TestContext { get; set; }

	private static (CleanThumbnailHostedService HostedService,
		FakeIThumbnailCleaner ThumbnailCleaner,
		FakeTriggerableIHostApplicationLifetime HostApplicationLifetime) CreateServiceScope(
		bool thumbnailCleanupSkipOnStartup, DateTime? lastRun = null)
	{
		var serviceProvider = new ServiceCollection()
			.AddSingleton<IThumbnailCleaner, FakeIThumbnailCleaner>()
			.AddSingleton<AppSettings>()
			.AddSingleton<ISettingsService, FakeISettingsService>()
			.BuildServiceProvider();

		var hostApplicationLifetime = new FakeTriggerableIHostApplicationLifetime();

		var service =
			serviceProvider.GetRequiredService<IThumbnailCleaner>() as FakeIThumbnailCleaner;
		service!.Files = ["test.jpg"];

		if ( lastRun != null )
		{
			var settingService =
				serviceProvider.GetRequiredService<ISettingsService>();
			var lastRunDateTime = lastRun.Value.ToDefaultSettingsFormat();
			settingService.AddOrUpdateSetting(SettingsType.CleanUpThumbnailDatabaseLastRun,
				lastRunDateTime);
		}

		var appSettings =
			serviceProvider.GetRequiredService<AppSettings>();
		appSettings.ThumbnailCleanupSkipOnStartup = thumbnailCleanupSkipOnStartup;

		var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var hostedService = new CleanThumbnailHostedService(serviceScopeFactory,
			hostApplicationLifetime);
		return (hostedService, service, hostApplicationLifetime);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task StartAsync_ShouldNotBlock_AndShouldRunAfterApplicationStarted()
	{
		var (hostedService, thumbnailCleaner, hostApplicationLifetime) =
			CreateServiceScope(false);
		hostedService.StartupDelay = TimeSpan.Zero;

		// StartAsync should return immediately and not run cleanup yet.
		await hostedService.StartAsync(CancellationToken.None)
			.WaitAsync(TimeSpan.FromMilliseconds(250), TestContext.CancellationToken);
		Assert.IsEmpty(thumbnailCleaner.Inputs);

		// Trigger app started callback so the background cleanup task can begin.
		hostApplicationLifetime.TriggerApplicationStarted();

		var sw = System.Diagnostics.Stopwatch.StartNew();
		while ( thumbnailCleaner.Inputs.Count == 0 && sw.ElapsedMilliseconds < 2000 )
		{
			await Task.Delay(25, TestContext.CancellationToken);
		}

		Assert.HasCount(1, thumbnailCleaner.Inputs);
		await hostedService.StopAsync(CancellationToken.None);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	[DataRow(-1, 0)] // one day ago skips
	[DataRow(-7, 1)] // 7 days ago runs
	public async Task StartBackgroundAsync_RelativeDays(int relativeDays, int expectCount)
	{
		// Arrange
		var (hostedService, _, _) = CreateServiceScope(false,
			DateTime.UtcNow.AddDays(relativeDays));
		using var cancellationTokenSource = new CancellationTokenSource();
		var stoppingToken = cancellationTokenSource.Token;

		// Act
		var result = await hostedService.StartBackgroundAsync(new TimeSpan(0), stoppingToken);

		// mock always return one item, except when disabled
		Assert.HasCount(expectCount, result);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	[DataRow(true, 0)]
	[DataRow(false, 1)]
	public async Task StartBackgroundAsync_TrueAndFalse(bool thumbnailCleanupSkipOnStartup,
		int expectCount)
	{
		// Arrange
		var (hostedService, _, _) = CreateServiceScope(thumbnailCleanupSkipOnStartup);
		using var cancellationTokenSource = new CancellationTokenSource();
		var stoppingToken = cancellationTokenSource.Token;

		// Act
		var result = await hostedService.StartBackgroundAsync(new TimeSpan(0), stoppingToken);

		// mock always return one item, except when disabled
		Assert.HasCount(expectCount, result);
	}

	[TestMethod]
	[DataRow(-1000, true)]
	[DataRow(-7, true)]
	[DataRow(-1, false)]
	[DataRow(null, true)]
	public async Task ContinueDueSettings_ShouldReturnExpectedResult(int? lastRunRelative,
		bool expectedResult)
	{
		// Arrange
		var appSettings = new AppSettings { ThumbnailCleanupSkipOnStartup = false };
		var settingService = new FakeISettingsService();

		if ( lastRunRelative.HasValue )
		{
			var lastRun = DateTime.UtcNow.AddDays(lastRunRelative.Value);
			await settingService.AddOrUpdateSetting(SettingsType.CleanUpThumbnailDatabaseLastRun,
				lastRun.ToDefaultSettingsFormat());
		}
		else
		{
			await settingService.RemoveSetting(SettingsType.CleanUpThumbnailDatabaseLastRun);
		}

		// Act
		var result =
			await CleanThumbnailHostedService.ContinueDueSettings(appSettings, settingService);

		// Assert
		Assert.AreEqual(expectedResult, result);
	}
}
