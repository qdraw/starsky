using System;
using System.Reflection;
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
using starskytest.ExtensionMethods;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class CleanThumbnailHostedServiceTest
{
	private static CleanThumbnailHostedService CreateServiceScope(
		bool thumbnailCleanupSkipOnStartup, DateTime? lastRun = null)
	{
		var serviceProvider = new ServiceCollection()
			.AddSingleton<IThumbnailCleaner, FakeIThumbnailCleaner>()
			.AddSingleton<AppSettings>()
			.AddSingleton<ISettingsService, FakeISettingsService>()
			.BuildServiceProvider();

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

		var serviceScopeFactory = new FakeServiceScopeFactory(serviceProvider);
		var hostedService = new CleanThumbnailHostedService(serviceScopeFactory);
		return hostedService;
	}

	[TestMethod]
	[Timeout(10000)]
	public async Task ExecuteAsync_ShouldInvokeCleanAllUnusedFilesAsync()
	{
		// Arrange
		var hostedService = CreateServiceScope(false);
		using var cancellationTokenSource = new CancellationTokenSource();
		var stoppingToken = cancellationTokenSource.Token;

		// Act
		var dynMethod = hostedService.GetType().GetMethod("ExecuteAsync",
			                BindingFlags.NonPublic | BindingFlags.Instance) ??
		                throw new Exception("missing ExecuteAsync");

		// Assert
		// The method should throw a TimeoutException if it takes longer than 1 second to execute
		await Assert.ThrowsExceptionAsync<TimeoutException>(async () =>
		{
			await dynMethod.InvokeAsync(hostedService, stoppingToken)
				.WaitAsync(TimeSpan.FromSeconds(1), new CancellationToken());
		});
	}

	[TestMethod]
	[Timeout(5000)]
	[DataRow(-1, 0)] // one day ago skips
	[DataRow(-7, 1)] // 7 days ago runs
	public async Task StartBackgroundAsync_RelativeDays(int relativeDays, int expectCount)
	{
		// Arrange
		var hostedService = CreateServiceScope(false,
			DateTime.UtcNow.AddDays(relativeDays));
		using var cancellationTokenSource = new CancellationTokenSource();
		var stoppingToken = cancellationTokenSource.Token;

		// Act
		var result = await hostedService.StartBackgroundAsync(new TimeSpan(0), stoppingToken);

		// mock always return one item, except when disabled
		Assert.AreEqual(expectCount, result.Count);
	}

	[TestMethod]
	[Timeout(5000)]
	[DataRow(true, 0)]
	[DataRow(false, 1)]
	public async Task StartBackgroundAsync_TrueAndFalse(bool thumbnailCleanupSkipOnStartup,
		int expectCount)
	{
		// Arrange
		var hostedService = CreateServiceScope(thumbnailCleanupSkipOnStartup);
		using var cancellationTokenSource = new CancellationTokenSource();
		var stoppingToken = cancellationTokenSource.Token;

		// Act
		var result = await hostedService.StartBackgroundAsync(new TimeSpan(0), stoppingToken);

		// mock always return one item, except when disabled
		Assert.AreEqual(expectCount, result.Count);
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

	private class FakeServiceScope : IServiceScope
	{
		public FakeServiceScope(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		public IServiceProvider ServiceProvider { get; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			// Cleanup
		}
	}

	private class FakeServiceScopeFactory : IServiceScopeFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public FakeServiceScopeFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public IServiceScope CreateScope()
		{
			return new FakeServiceScope(_serviceProvider);
		}
	}
}
