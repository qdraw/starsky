using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
	public TestContext TestContext { get; set; }

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
		var result =
			await sut.CheckHealthAsync(healthCheck, TestContext.CancellationTokenSource.Token);
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
		var result =
			await sut.CheckHealthAsync(healthCheck, TestContext.CancellationTokenSource.Token);
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
		var result =
			await sut.CheckHealthAsync(healthCheck, TestContext.CancellationTokenSource.Token);
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

	[TestMethod]
	public void GetWindowsDriveInfo_ExistingDrive_ReturnsExists()
	{
		var healthCheck =
			new DiskStorageHealthCheck(new DiskStorageOptions(), new FakeIWebLogger());
		// Use root drive for test (should exist on all systems)
		var drive = Path.GetPathRoot(Environment.SystemDirectory);
		Console.WriteLine("Testing drive: " + drive);
		var (exists, actualFreeMegabytes) = healthCheck.GetWindowsDriveInfo(drive!);
		Assert.IsTrue(exists);
		Assert.IsGreaterThan(0, actualFreeMegabytes);
	}

	[TestMethod]
	public void GetWindowsDriveInfo_NonExistingDrive_ReturnsNotExists()
	{
		var healthCheck =
			new DiskStorageHealthCheck(new DiskStorageOptions(), new FakeIWebLogger());
		var (exists, actualFreeMegabytes) = healthCheck.GetWindowsDriveInfo("Z:\\");
		Assert.IsFalse(exists);
		Assert.AreEqual(0, actualFreeMegabytes);
	}

	[TestMethod]
	public void GetWindowsDriveInfo_InvalidDrive_HandlesException()
	{
		var healthCheck =
			new DiskStorageHealthCheck(new DiskStorageOptions(), new FakeIWebLogger());
		// Illegal drive name
		var (exists, actualFreeMegabytes) = healthCheck.GetWindowsDriveInfo("?*invalid*?");
		Assert.IsFalse(exists);
		Assert.AreEqual(0, actualFreeMegabytes);
	}

	[TestMethod]
	public void GetUnixDriveInfo_ExistingDirectory_ReturnsExists()
	{
		var healthCheck =
			new DiskStorageHealthCheck(new DiskStorageOptions(), new FakeIWebLogger());
		var tempDir = Path.GetTempPath();
		var (exists, actualFreeMegabytes) = healthCheck.GetUnixDriveInfo(tempDir);
		Assert.IsTrue(exists);
		Assert.IsGreaterThan(0, actualFreeMegabytes);
	}

	[TestMethod]
	public void GetUnixDriveInfo_NonExistingDirectory_ReturnsNotExists()
	{
		var healthCheck =
			new DiskStorageHealthCheck(new DiskStorageOptions(), new FakeIWebLogger());
		var (exists, actualFreeMegabytes) = healthCheck.GetUnixDriveInfo("/notarealdir123456789");
		Assert.IsFalse(exists);
		Assert.AreEqual(0, actualFreeMegabytes);
	}

	[TestMethod]
	public void GetUnixDriveInfo_InvalidPath_HandlesException()
	{
		var healthCheck =
			new DiskStorageHealthCheck(new DiskStorageOptions(), new FakeIWebLogger());
		// Use a file path instead of directory
		var filePath = Path.GetTempFileName();
		try
		{
			var (exists, actualFreeMegabytes) = healthCheck.GetUnixDriveInfo(filePath);
			Assert.IsFalse(exists);
			Assert.AreEqual(0, actualFreeMegabytes);
		}
		finally
		{
			File.Delete(filePath);
		}
	}
}
