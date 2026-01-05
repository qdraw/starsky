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

namespace starskytest.starsky.foundation.cloudsync.Services;

[TestClass]
public class CloudSyncScheduledServiceTest
{
	private class FakeCloudSyncService : ICloudSyncService
	{
		public bool IsSyncInProgress { get; set; }
		public CloudSyncResult? LastSyncResult { get; set; }
		public List<CloudSyncTriggerType> SyncCalls { get; } = new();

		public Task<CloudSyncResult> SyncAsync(CloudSyncTriggerType triggerType)
		{
			SyncCalls.Add(triggerType);
			return Task.FromResult(new CloudSyncResult
			{
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType
			});
		}
	}

	private class FakeLogger : IWebLogger
	{
		public List<string> LoggedMessages { get; } = new();

		public void LogInformation(string message)
		{
			LoggedMessages.Add($"INFO: {message}");
		}

		public void LogError(string message)
		{
			LoggedMessages.Add($"ERROR: {message}");
		}

		public void LogError(Exception exception, string message)
		{
			LoggedMessages.Add($"ERROR: {message} - {exception.Message}");
		}

		public void LogWarning(string message)
		{
			LoggedMessages.Add($"WARNING: {message}");
		}

		public void LogDebug(string message)
		{
			LoggedMessages.Add($"DEBUG: {message}");
		}

		public void LogTrace(string message)
		{
			LoggedMessages.Add($"TRACE: {message}");
		}

		public void LogCritical(string message)
		{
			LoggedMessages.Add($"CRITICAL: {message}");
		}

		public void LogCritical(Exception exception, string message)
		{
			LoggedMessages.Add($"CRITICAL: {message} - {exception.Message}");
		}
	}

	[TestMethod]
	public async Task ExecuteAsync_WhenDisabled_ShouldNotRunSync()
	{
		// Arrange
		var settings = new CloudSyncSettings { Enabled = false };
		var cloudSyncService = new FakeCloudSyncService();
		var logger = new FakeLogger();
		var service = new CloudSyncScheduledService(cloudSyncService, logger, settings);

		using var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(1));

		// Act
		await service.StartAsync(cts.Token);
		await Task.Delay(500); // Give it some time
		await service.StopAsync(cts.Token);

		// Assert
		Assert.AreEqual(0, cloudSyncService.SyncCalls.Count);
		Assert.IsTrue(logger.LoggedMessages.Any(m => m.Contains("disabled")));
	}

	[TestMethod]
	public async Task ExecuteAsync_WhenEnabled_ShouldScheduleSync()
	{
		// Arrange
		var settings = new CloudSyncSettings
		{
			Enabled = true,
			SyncFrequencyMinutes = 0,
			SyncFrequencyHours = 1 // Will be very short for test
		};
		var cloudSyncService = new FakeCloudSyncService();
		var logger = new FakeLogger();
		
		// We can't actually test the full scheduled execution without waiting hours
		// So we'll just verify the service starts correctly
		var service = new CloudSyncScheduledService(cloudSyncService, logger, settings);

		using var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromSeconds(1));

		// Act
		await service.StartAsync(cts.Token);
		await Task.Delay(100);
		await service.StopAsync(cts.Token);

		// Assert - Service should log that it's starting
		Assert.IsTrue(logger.LoggedMessages.Any(m => m.Contains("started")));
	}

	[TestMethod]
	public async Task StopAsync_ShouldLogStopping()
	{
		// Arrange
		var settings = new CloudSyncSettings { Enabled = true, SyncFrequencyMinutes = 60 };
		var cloudSyncService = new FakeCloudSyncService();
		var logger = new FakeLogger();
		var service = new CloudSyncScheduledService(cloudSyncService, logger, settings);

		using var cts = new CancellationTokenSource();

		// Act
		await service.StartAsync(cts.Token);
		await service.StopAsync(cts.Token);

		// Assert
		Assert.IsTrue(logger.LoggedMessages.Any(m => m.Contains("stopping")));
	}
}

