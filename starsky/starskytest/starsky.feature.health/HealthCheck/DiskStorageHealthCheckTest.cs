using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.HealthCheck;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.health.HealthCheck;

[TestClass]
public sealed class DiskStorageHealthCheckTest
{
	[TestMethod]
	[SuppressMessage("Performance",
		"CA1806:Do not ignore method results",
		Justification = "Should fail when null in constructor")]
	[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
	public void Constructor_NullOptions_ThrowsArgumentNullException()
	{
		// Arrange
		DiskStorageOptions? options = null;
		var logger = new FakeIWebLogger();

		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() =>
			new DiskStorageHealthCheck(options, logger));
	}

	[TestMethod]
	public void Constructor_ValidParameters_DoesNotThrow()
	{
		// Arrange
		var options = new DiskStorageOptions();
		var logger = new FakeIWebLogger();

		// Act & Assert
		var healthCheck = new DiskStorageHealthCheck(options, logger);
		Assert.IsNotNull(healthCheck);
	}

	[TestMethod]
	public async Task RunSuccessful()
	{
		var appSettings = new AppSettings();
		var diskOptions = new DiskStorageOptions();
		DiskOptionsPercentageSetup.Setup(appSettings.TempFolder, diskOptions, 0.000001f);

		var healthCheck = new HealthCheckContext();
		var sut = new DiskStorageHealthCheck(diskOptions, new FakeIWebLogger());
		var result = await sut.CheckHealthAsync(healthCheck, TestContext.CancellationTokenSource.Token);
		Assert.AreEqual(HealthStatus.Healthy, result.Status);
	}

	[TestMethod]
	public async Task RunFailNotEnoughSpace()
	{
		var appSettings = new AppSettings();
		var diskOptions = new DiskStorageOptions();

		DiskOptionsPercentageSetup.Setup(appSettings.TempFolder, diskOptions, 1.01f);

		var sut = new DiskStorageHealthCheck(diskOptions, new FakeIWebLogger());

		var healthCheck = new HealthCheckContext
		{
			Registration = new HealthCheckRegistration("te",
				sut, null, null)
		};
		var result = await sut.CheckHealthAsync(healthCheck, TestContext.CancellationTokenSource.Token);
		Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		Assert.IsTrue(result.Description?.Contains("Minimum configured megabytes for disk"));
	}

	[TestMethod]
	public async Task RunFailNonExistDisk()
	{
		var diskOptions = new DiskStorageOptions();
		diskOptions.AddDrive("NonExistDisk:", 10);

		var sut = new DiskStorageHealthCheck(diskOptions, new FakeIWebLogger());

		var healthCheck = new HealthCheckContext
		{
			Registration = new HealthCheckRegistration("te",
				sut, null, null)
		};
		var result = await sut.CheckHealthAsync(healthCheck, TestContext.CancellationTokenSource.Token);
		Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		Assert.IsTrue(result.Description?.Contains("is not present on system"));
	}

	[TestMethod]
	public async Task RunFail_NullReferenceException()
	{
		var appSettings = new AppSettings();
		var diskOptions = new DiskStorageOptions();
		DiskOptionsPercentageSetup.Setup(appSettings.TempFolder, diskOptions, 99.9f);

		var healthCheck = new HealthCheckContext();
		var sut = new DiskStorageHealthCheck(diskOptions, new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<NullReferenceException>(async () =>
			await sut.CheckHealthAsync(healthCheck, TestContext.CancellationTokenSource.Token));
	}

	public TestContext TestContext { get; set; }
}
