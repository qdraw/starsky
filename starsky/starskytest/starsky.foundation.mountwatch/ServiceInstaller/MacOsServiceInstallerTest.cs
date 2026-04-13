using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.ServiceInstaller;

[TestClass]
public sealed class MacOsServiceInstallerTest
{
	[TestMethod]
	public async Task InstallAsync_WritesToStorage()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new MacOsServiceInstaller(logger, storage,
			(_, _) => Task.FromResult(true));

		var execPath = "/usr/local/bin/starskymountwatchercli";

		// Act
		var result = await sut.InstallAsync(execPath);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task InstallAsync_CreateDirectoryBeforeWrite()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new MacOsServiceInstaller(logger, storage,
			(_, _) => Task.FromResult(true));

		// Act
		var result = await sut.InstallAsync("/opt/starsky/bin/cli");

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task InstallAsync_WriteFails_ReturnsFalse()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(new InvalidOperationException("Write failed"));
		var sut = new MacOsServiceInstaller(logger, storage,
			(_, _) => Task.FromResult(true));

		var result = await sut.InstallAsync("/usr/local/bin/cli");

		Assert.IsFalse(result);
		Assert.AreNotEqual(0, logger.TrackedExceptions.Count);
	}

	[TestMethod]
	public async Task StartAsync_CallsLaunchctlLoad()
	{
		// Arrange
		var calls = new List<(string fileName, string args)>();
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new MacOsServiceInstaller(logger, storage,
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				return Task.FromResult(true);
			});

		// Act
		var result = await sut.StartAsync();

		// Assert
		Assert.IsTrue(result);
		Assert.HasCount(1, calls);
		Assert.AreEqual("launchctl", calls[0].fileName);
		Assert.Contains("load", calls[0].args);
		Assert.Contains(".plist", calls[0].args);
	}

	[TestMethod]
	public async Task StartAsync_LaunchctlFails_ReturnsFalse()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new MacOsServiceInstaller(logger, storage,
			(_, _) => Task.FromResult(false));

		var result = await sut.StartAsync();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task StopAsync_CallsLaunchctlUnload()
	{
		// Arrange
		var calls = new List<(string fileName, string args)>();
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new MacOsServiceInstaller(logger, storage,
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				return Task.FromResult(true);
			});

		// Act
		var result = await sut.StopAsync();

		// Assert
		Assert.IsTrue(result);
		Assert.HasCount(1, calls);
		Assert.AreEqual("launchctl", calls[0].fileName);
		Assert.Contains("unload", calls[0].args);
		Assert.Contains(".plist", calls[0].args);
	}

	[TestMethod]
	public async Task StopAsync_LaunchctlFails_ReturnsFalse()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var sut = new MacOsServiceInstaller(logger, storage,
			(_, _) => Task.FromResult(false));

		var result = await sut.StopAsync();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task UninstallAsync_PlistExists_StopsThenDeletes()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(
			outputSubPathFiles: new List<string> { GetMacOsPlistPath() });
		var launchctlCalls = new List<string>();

		var sut = new MacOsServiceInstaller(logger, storage,
			(_, args) =>
			{
				launchctlCalls.Add(args);
				return Task.FromResult(true);
			});

		// Act
		var result = await sut.UninstallAsync();

		// Assert
		Assert.IsTrue(result);
		Assert.AreNotEqual(0, launchctlCalls.Count);
		Assert.Contains("unload", launchctlCalls[0]);
	}

	[TestMethod]
	public async Task UninstallAsync_PlistNotExists_ReturnsTrue()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(); // Empty storage - file doesn't exist
		var sut = new MacOsServiceInstaller(logger, storage,
			(_, _) => Task.FromResult(true));

		var result = await sut.UninstallAsync();

		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task StatusAsync_NotInstalledAndLaunchctlReportsInactive_ReturnsFalseFalse()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(); // no plist -> not installed
		var sut = new MacOsServiceInstaller(logger, storage,
			(_, _) => Task.FromResult(false));

		var (installed, running) = await sut.StatusAsync();

		Assert.IsFalse(installed);
		Assert.IsFalse(running);
	}

	[TestMethod]
	public async Task StatusAsync_PlistExistsAndLaunchctlReportsLoaded_ReturnsTrueTrue()
	{
		var logger = new FakeIWebLogger();
		var plistPath = MacOsServiceInstaller.GetMacOsPlistPath();
		var storage = new FakeIStorage(outputSubPathFiles: [plistPath]);
		var sut = new MacOsServiceInstaller(logger, storage,
			(_, args) => Task.FromResult(args.Contains("list")));

		var (installed, running) = await sut.StatusAsync();

		Assert.IsTrue(installed);
		Assert.IsTrue(running);
	}

	[TestMethod]
	public async Task StatusAsync_LaunchctlThrows_ReturnsInstalledTrueRunningFalse()
	{
		var logger = new FakeIWebLogger();
		var plistPath = MacOsServiceInstaller.GetMacOsPlistPath();
		var storage = new FakeIStorage(outputSubPathFiles: [plistPath]);
		var sut = new MacOsServiceInstaller(logger, storage,
			(_, _) => throw new InvalidOperationException("boom"));

		var (installed, running) = await sut.StatusAsync();

		Assert.IsTrue(installed);
		Assert.IsFalse(running);
	}

	[TestMethod]
	public void GetMacOsPlistPath_ContainsLaunchAgents()
	{
		var path = MacOsServiceInstaller.GetMacOsPlistPath();

		Assert.Contains("LaunchAgents", path);
	}

	[TestMethod]
	public void GetMacOsPlistPath_ContainsServiceName()
	{
		var path = MacOsServiceInstaller.GetMacOsPlistPath();

		Assert.Contains("nl.qdraw.mountwatcher", path);
	}

	[TestMethod]
	public void GetMacOsPlistPath_EndsWith_PlistExtension()
	{
		var path = MacOsServiceInstaller.GetMacOsPlistPath();

		Assert.IsTrue(path.EndsWith(".plist", StringComparison.OrdinalIgnoreCase));
	}

	[TestMethod]
	public void GetMacOsPlistPath_ContainsUserProfile()
	{
		var path = MacOsServiceInstaller.GetMacOsPlistPath();
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

		Assert.IsTrue(path.StartsWith(home, StringComparison.OrdinalIgnoreCase));
	}

	private static string GetMacOsPlistPath()
	{
		return MacOsServiceInstaller.GetMacOsPlistPath();
	}
}
