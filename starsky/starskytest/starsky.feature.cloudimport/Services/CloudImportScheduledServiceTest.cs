using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.cloudimport;
using starsky.feature.cloudimport.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.cloudimport.Services;

[TestClass]
public class CloudImportScheduledServiceTest
{
	[TestMethod]
	public async Task ExecuteAsync_WhenDisabled_ShouldNotRunSync()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
					[new CloudImportProviderSettings { Id = "test", Enabled = false }]
			}
		};
		var cloudImportService = new FakeCloudImportService();
		var logger = new FakeIWebLogger();
		var service = new CloudImportScheduledService(cloudImportService, logger, appSettings);

		using var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(1));

		// Act
		await service.RunAsync(cts.Token);
		await service.StopAsync(cts.Token);

		// Assert
		Assert.IsEmpty(cloudImportService.SyncCalls);
		Assert.Contains(
			m =>
				m.Item2!.Contains("disabled") || m.Item2!.Contains("not run"),
			logger.TrackedInformation);
	}

	[TestMethod]
	public async Task ExecuteAsync_WhenEnabled_ShouldScheduleSync()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "test",
						Enabled = true,
						SyncFrequencyMinutes = 0,
						SyncFrequencyHours = 1
					}
				]
			}
		};
		var cloudImportService = new FakeCloudImportService();
		var logger = new FakeIWebLogger();
		var service = new CloudImportScheduledService(cloudImportService, logger, appSettings);

		using var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(1));

		// Act
		await service.RunAsync(cts.Token);
		await service.StopAsync(cts.Token);

		// Assert - Service should log that it's starting
		Assert.Contains(m => m.Item2!.Contains("Starting scheduled sync"),
			logger.TrackedInformation);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task StopAsync_ShouldLogStopping()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "test", Enabled = true, SyncFrequencyMinutes = 60
					}
				]
			}
		};
		var cloudImportService = new FakeCloudImportService();
		var logger = new FakeIWebLogger();
		var service = new CloudImportScheduledService(cloudImportService, logger, appSettings);

		using var cts = new CancellationTokenSource();

		// Act
		await service.StartAsync(cts.Token);
		await service.StopAsync(cts.Token);

		// Assert
		Assert.Contains(m => m.Item2!.Contains("stopping"), logger.TrackedInformation);
	}

	[TestMethod]
	public async Task RunProviderSyncSingleAsync_ReturnsTrue_WhenDelayIsTooLong()
	{
		var appSettings = new AppSettings();
		var cloudImportService = new FakeCloudImportService();
		var logger = new FakeIWebLogger();
		var service = new CloudImportScheduledService(cloudImportService, logger, appSettings);

		var provider = new CloudImportProviderSettings
		{
			Id = "test", Enabled = true, SyncFrequencyMinutes = 0, SyncFrequencyHours = 0
		};
		using var cts = new CancellationTokenSource();
		var result = await service.RunProviderSyncSingleAsync(provider, cts.Token);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task RunProviderSyncSingleAsync_ReturnsTrue_WhenTokenIsCancelled()
	{
		var appSettings = new AppSettings();
		var cloudImportService = new FakeCloudImportService();
		var logger = new FakeIWebLogger();
		var service = new CloudImportScheduledService(cloudImportService, logger, appSettings);

		var provider = new CloudImportProviderSettings
		{
			Id = "test", Enabled = true, SyncFrequencyMinutes = 1
		};
		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();
		var result = await service.RunProviderSyncSingleAsync(provider, cts.Token);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task RunProviderSyncSingleAsync_ReturnsFalse_AndCallsSyncAsync_WhenDelayIsValid()
	{
		var appSettings = new AppSettings();
		var cloudImportService = new FakeCloudImportService();
		var logger = new FakeIWebLogger();
		var service = new CloudImportScheduledService(cloudImportService, logger, appSettings);

		var provider = new CloudImportProviderSettings
		{
			Id = "test", Enabled = true, SyncFrequencyMinutes = 0.001 // ~0.06 seconds
		};
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
		var result = await service.RunProviderSyncSingleAsync(provider, cts.Token);
		Assert.IsFalse(result);
		Assert.HasCount(1, cloudImportService.SyncCalls);
		Assert.AreEqual(CloudImportTriggerType.Scheduled, cloudImportService.SyncCalls[0]);
	}

	[TestMethod]
	public async Task RunProviderSyncSingleAsync_ThrowsException_WhenSyncAsyncThrows()
	{
		var appSettings = new AppSettings();
		var cloudImportService = new FakeCloudImportService { ThrowOnSync = true };
		var logger = new FakeIWebLogger();
		var service = new CloudImportScheduledService(cloudImportService, logger, appSettings);

		var provider = new CloudImportProviderSettings
		{
			Id = "test", Enabled = true, SyncFrequencyMinutes = 0.001
		};
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
		{
			await service.RunProviderSyncSingleAsync(provider, cts.Token);
		});
	}
}
