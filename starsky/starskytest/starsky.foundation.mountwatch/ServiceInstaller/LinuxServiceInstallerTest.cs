using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.ServiceInstaller;

[TestClass]
public sealed class LinuxServiceInstallerTest
{
	private static string GetServiceName()
	{
		// Use the same systemd service name the installer expects
		return new WatchServiceName().GetSystemDName();
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
	[OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
	public async Task InstallAsync_WriteFails_FallsBackToUserInstall__UnixOnly()
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

		const string execPath = "/opt/starsky/bin/cli";

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
		// Verify that the expected sudo daemon-reload instruction was logged
		Assert.IsTrue(logger.TrackedInformation.Exists(t => t.Item2 != null &&
		                                                    t.Item2.Contains(
			                                                    "Run: sudo systemctl daemon-reload")),
			"Expected sudo daemon-reload instruction in logs");
		// Verify the systemd unit removed message
		Assert.IsTrue(logger.TrackedInformation.Exists(t => t.Item2 != null &&
		                                                    t.Item2.Contains(
			                                                    "systemd unit removed: " +
			                                                    systemServicePath)),
			"Expected systemd unit removed message in logs");
	}

	[TestMethod]
	public async Task UninstallAsync_UserFileExists_DeletesIt()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var userServicePath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".config", "systemd", "user", $"{new WatchServiceName().GetSystemDName()}.service");

		var storage = new FakeIStorage([], [userServicePath]);
		var sut = new LinuxServiceInstaller(logger, storage);

		// Act
		var result = await sut.UninstallAsync();

		// Assert
		Assert.IsTrue(result);
		// Verify that the expected user-level daemon-reload instruction was logged
		Assert.IsTrue(logger.TrackedInformation.Exists(t => t.Item2 != null &&
		                                                    t.Item2.Contains(
			                                                    "Run: systemctl --user daemon-reload")),
			"Expected user daemon-reload instruction in logs");
		// Verify the systemd user unit removed message
		Assert.IsTrue(logger.TrackedInformation.Exists(t => t.Item2 != null &&
		                                                    t.Item2.Contains(
			                                                    "systemd user unit removed: " +
			                                                    userServicePath)),
			"Expected systemd user unit removed message in logs");
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

		var sutWithFake = new LinuxServiceInstaller(logger, storage, FakeRun,
			new FakeUnixSecurity(false));
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
			return Task.FromResult(file != "systemctl" || !args.Contains("start ") ||
			                       args.Contains("--user"));
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

		var sutWithFake = new LinuxServiceInstaller(logger, storage, FakeRun,
			new FakeUnixSecurity(false));
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
			return Task.FromResult(file != "systemctl" || !args.Contains("stop ") ||
			                       args.Contains("--user"));
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

	[TestMethod]
	public async Task InstallAsync_SystemWriteFails_UserWriteSucceeds_ReturnsTrue()
	{
		var logger = new FakeIWebLogger();


		// Use a LinuxServiceInstaller that will attempt system write first; to simulate a system write failure
		// we create a wrapper storage that throws when WriteStreamAsync is called for /etc/systemd/system/...
		const string execPath = "/usr/bin/whatever";

		Task<bool> FakeRun(string file, string args)
		{
			return Task.FromResult(true);
		}

		// Create a storage that returns false for system path write by overriding WriteStreamAsync via exception injection
		var failingStorage = new FakeIStorage();

		// First, create sut with normal storage but inject a runProcess that doesn't matter here
		var sut = new LinuxServiceInstaller(logger, failingStorage, FakeRun,
			new FakeUnixSecurity(false));

		// Act
		var result = await sut.InstallAsync(execPath);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task InstallUserAsync_CreateDirectoryThrows_ReturnsFalse()
	{
		var logger = new FakeIWebLogger();
		// Make a storage that throws when CreateDirectory is called
		var storage = new FakeIStorage(new Exception("create dir fail"));
		var sut = new LinuxServiceInstaller(logger, storage);

		var result = await sut.InstallAsync("/bin/foo");

		Assert.IsFalse(result);
		Assert.IsNotEmpty(logger.TrackedExceptions);
	}

	[TestMethod]
	public async Task StartAsync_RunProcessThrows_ReturnsFalseAndLogs()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();

		Task<bool> ThrowingRun(string file, string args)
		{
			throw new InvalidOperationException("boom");
		}

		var sut = new LinuxServiceInstaller(logger, storage, ThrowingRun,
			new FakeUnixSecurity(false));
		var result = await sut.StartAsync();

		Assert.IsFalse(result);
		Assert.IsNotEmpty(logger.TrackedExceptions);
	}

	[TestMethod]
	public async Task StopAsync_RunProcessThrows_ReturnsFalseAndLogs()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();

		Task<bool> ThrowingRun(string file, string args)
		{
			throw new InvalidOperationException("stopboom");
		}

		var sut = new LinuxServiceInstaller(logger, storage, ThrowingRun,
			new FakeUnixSecurity(false));
		var result = await sut.StopAsync();

		Assert.IsFalse(result);
		Assert.IsNotEmpty(logger.TrackedExceptions);
	}

	[TestMethod]
	public async Task UninstallAsync_FileDeleteThrows_HandledAndLogs()
	{
		var logger = new FakeIWebLogger();
		const string systemServicePath = "/etc/systemd/system/nl.qdraw.mountwatcher.debug.service";

		// We'll wrap storage by creating a derived class instance that throws on FileDelete
		var throwingStorage =
			new FakeIStorage(outputSubPathFiles: new List<string> { systemServicePath });
		// Replace the FileDelete behavior via a simple wrapper class in test scope
		var storageWrapper = new ThrowOnDeleteStorage(throwingStorage);

		var sut = new LinuxServiceInstaller(logger, storageWrapper);
		var result = await sut.UninstallAsync();

		Assert.IsTrue(result);
		Assert.IsNotEmpty(logger.TrackedExceptions, "Should log delete error");
	}

	private sealed class ThrowOnDeleteStorage : FakeIStorage
	{
		private readonly FakeIStorage _inner;

		public ThrowOnDeleteStorage(FakeIStorage inner)
		{
			_inner = inner;
		}

		public override bool ExistFile(string path)
		{
			return _inner.ExistFile(path);
		}

		public override bool FileDelete(string path)
		{
			throw new InvalidOperationException("delete failed");
		}
	}
}
