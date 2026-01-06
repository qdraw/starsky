using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.cloudimport.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.cloudimport.Services;

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
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new()
					{
						Id = "test",
						Enabled = true,
						SyncFrequencyMinutes = 0,
						SyncFrequencyHours = 1
					}
				}
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
		Assert.IsTrue(logger.TrackedInformation.Any(m => m.Item2!.Contains("started")));
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
		Assert.IsTrue(logger.TrackedInformation.Any(m => m.Item2!.Contains("stopping")));
	}
}
