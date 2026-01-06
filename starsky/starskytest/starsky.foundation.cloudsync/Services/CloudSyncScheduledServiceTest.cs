using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.cloudsync;
using starsky.foundation.cloudsync.Interfaces;
using starsky.foundation.cloudsync.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.cloudsync.Services;

[TestClass]
public class CloudSyncScheduledServiceTest
{
	[TestMethod]
	public async Task ExecuteAsync_WhenDisabled_ShouldNotRunSync()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudSync = new CloudSyncSettings
			{
				Providers = [new CloudSyncProviderSettings { Id = "test", Enabled = false }]
			}
		};
		var cloudSyncService = new FakeCloudSyncService();
		var logger = new FakeIWebLogger();
		var service = new CloudSyncScheduledService(cloudSyncService, logger, appSettings);

		using var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(1));

		// Act
		await service.StartAsync(cts.Token);
		await Task.Delay(500); // Give it some time
		await service.StopAsync(cts.Token);

		// Assert
		Assert.AreEqual(0, cloudSyncService.SyncCalls.Count);
		Assert.IsTrue(
			logger.TrackedInformation.Any(m =>
				m.Item2!.Contains("disabled") || m.Item2!.Contains("not run")));
	}

	[TestMethod]
	public async Task ExecuteAsync_WhenEnabled_ShouldScheduleSync()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudSync = new CloudSyncSettings
			{
				Providers = new List<CloudSyncProviderSettings>
				{
					new CloudSyncProviderSettings
					{
						Id = "test",
						Enabled = true,
						SyncFrequencyMinutes = 0,
						SyncFrequencyHours = 1
					}
				}
			}
		};
		var cloudSyncService = new FakeCloudSyncService();
		var logger = new FakeIWebLogger();
		var service = new CloudSyncScheduledService(cloudSyncService, logger, appSettings);

		using var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(1));

		// Act
		await service.RunAsync(cts.Token);
		await service.StopAsync(cts.Token);

		// Assert - Service should log that it's starting
		Assert.IsTrue(logger.TrackedInformation.Any(m => m.Item2!.Contains("started")));
	}

	[TestMethod]
	public async Task StopAsync_ShouldLogStopping()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudSync = new CloudSyncSettings
			{
				Providers =
				[
					new CloudSyncProviderSettings
					{
						Id = "test", Enabled = true, SyncFrequencyMinutes = 60
					}
				]
			}
		};
		var cloudSyncService = new FakeCloudSyncService();
		var logger = new FakeIWebLogger();
		var service = new CloudSyncScheduledService(cloudSyncService, logger, appSettings);

		using var cts = new CancellationTokenSource();

		// Act
		await service.RunAsync(cts.Token);
		await service.StopAsync(cts.Token);

		// Assert
		Assert.IsTrue(logger.TrackedInformation.Any(m => m.Item2!.Contains("stopping")));
	}
}
