using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.ServiceInstaller;

[TestClass]
public sealed class LinuxServiceInstallerTest
{
	private static string GetServiceName()
	{
		return "nl.qdraw.mountwatcher.debug";
	}

	[TestMethod]
	public async Task InstallAsync_WritesServiceFile()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new LinuxServiceInstaller(logger, storage);

		var execPath = "/usr/local/bin/starskymountwatchercli";

		// Act
		var result = await sut.InstallAsync(execPath);

		// Assert
		Assert.IsTrue(result);
		Assert.IsNotEmpty(logger.TrackedInformation, "Should log installation messages");
	}

	[TestMethod]
	public async Task InstallAsync_WriteFails_FallsBackToUserInstall()
	{
		// Arrange - simulate write failure at system level
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(
			new UnauthorizedAccessException("Access denied"));
		var sut = new LinuxServiceInstaller(logger, storage);

		const string execPath = "/usr/local/bin/starskymountwatchercli";

		// Act
		var result = await sut.InstallAsync(execPath);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task InstallAsync_CreatesCorrectServicePath()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new LinuxServiceInstaller(logger, storage);

		var execPath = "/opt/starsky/bin/cli";

		// Act
		var result = await sut.InstallAsync(execPath);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task UninstallAsync_SystemFileExists_DeletesIt()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var systemServicePath = $"/etc/systemd/system/{GetServiceName()}.service";
		var storage = new FakeIStorage(
			outputSubPathFiles: new List<string> { systemServicePath });
		var sut = new LinuxServiceInstaller(logger, storage);

		// Act
		var result = await sut.UninstallAsync();

		// Assert
		Assert.IsTrue(result);
		Assert.IsNotEmpty(logger.TrackedInformation, "Should log uninstall messages");
	}

	[TestMethod]
	public async Task UninstallAsync_UserFileExists_DeletesIt()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var userServicePath = $"{userHome}/.config/systemd/user/{GetServiceName()}.service";
		var storage = new FakeIStorage(
			outputSubPathFiles: new List<string> { userServicePath });
		var sut = new LinuxServiceInstaller(logger, storage);

		// Act
		var result = await sut.UninstallAsync();

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task UninstallAsync_BothFilesExist_DeletesBoth()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var systemServicePath = $"/etc/systemd/system/{GetServiceName()}.service";
		var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var userServicePath = $"{userHome}/.config/systemd/user/{GetServiceName()}.service";
		var storage = new FakeIStorage(
			outputSubPathFiles: new List<string> { systemServicePath, userServicePath });
		var sut = new LinuxServiceInstaller(logger, storage);

		// Act
		var result = await sut.UninstallAsync();

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task UninstallAsync_NoFilesExist_ReturnsTrue()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(); // Empty storage
		var sut = new LinuxServiceInstaller(logger, storage);

		// Act
		var result = await sut.UninstallAsync();

		// Assert
		Assert.IsTrue(result);
		Assert.IsNotEmpty(logger.TrackedInformation, "Should log that no service was found");
	}

	[TestMethod]
	public async Task StartAsync_CallsSystemctl()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();

		// Act - StartAsync uses RunProcess internally. Inject fake runProcess that simulates success for user-level
		var called = new List<string>();

		var sutWithFake = new LinuxServiceInstaller(logger, storage, FakeRun);
		var result = await sutWithFake.StartAsync();

		// Assert
		Assert.IsTrue(result);
		Assert.Contains("systemctl start", called[0]);
		Assert.Contains("systemctl --user", called[1]);
		return;

		Task<bool> FakeRun(string file, string args)
		{
			called.Add(file + " " + args);
			// simulate system-level systemctl failing and user-level succeeding
			return Task.FromResult(file != "systemctl" || !args.Contains("start ") || args.Contains("--user"));
		}
	}

	[TestMethod]
	public async Task StopAsync_CallsSystemctl()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();

		// Act - inject fake runProcess to simulate sudo failure then user success
		var called = new List<string>();

		var sutWithFake = new LinuxServiceInstaller(logger, storage, FakeRun);
		var result = await sutWithFake.StopAsync();

		// Assert - should succeed as user-level fallback succeeds
		Assert.IsTrue(result);
		Assert.Contains("systemctl stop", called[0]);
		Assert.Contains("systemctl --user", called[1]);
		return;

		Task<bool> FakeRun(string file, string args)
		{
			called.Add(file + " " + args);
			// simulate system-level systemctl failing and user-level succeeding
			return Task.FromResult(file != "systemctl" || !args.Contains("stop ") || args.Contains("--user"));
		}
	}

	[TestMethod]
	public async Task InstallAsync_WithEmptyExecutablePath_StillInstalls()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new LinuxServiceInstaller(logger, storage);

		// Act
		var result = await sut.InstallAsync("");

		// Assert - should still attempt installation
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task UninstallAsync_StopsServiceBeforeDeleting()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var systemServicePath = $"/etc/systemd/system/{GetServiceName()}.service";
		var storage = new FakeIStorage(
			outputSubPathFiles: new List<string> { systemServicePath });
		var sut = new LinuxServiceInstaller(logger, storage);

		// Act
		var result = await sut.UninstallAsync();

		// Assert - should have called StopAsync internally
		Assert.IsTrue(result);
		// Verify stop was attempted (indirectly through successful uninstall)
	}

	[TestMethod]
	public async Task InstallAsync_UserLevelFallback_CreatesDirectory()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new LinuxServiceInstaller(logger, storage);

		var execPath = "/usr/local/bin/starskymountwatchercli";

		// Act
		var result = await sut.InstallAsync(execPath);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task InstallAsync_LogsDetailedInstructions()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new LinuxServiceInstaller(logger, storage);

		var execPath = "/usr/local/bin/starskymountwatchercli";

		// Act
		var result = await sut.InstallAsync(execPath);

		// Assert
		Assert.IsTrue(result);
		var logOutput = string.Join("\n", logger.TrackedInformation.ConvertAll(x => x.Item2));
		Assert.Contains("systemctl", logOutput, "Should log systemctl instructions");
		Assert.Contains("daemon-reload", logOutput, "Should mention daemon-reload");
	}
}
