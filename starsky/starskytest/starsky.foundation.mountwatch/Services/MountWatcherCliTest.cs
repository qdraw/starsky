using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Interfaces;
using starsky.foundation.mountwatch.MountWatcher.Interfaces;
using starsky.foundation.mountwatch.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class MountWatcherCliTest
{
	private static MountWatcherCli CreateSut(
		FakeConsoleWrapper? console = null,
		FakeIWebLogger? logger = null,
		ICameraStorageDetector? mountDetector = null,
		IMountWatcherFactory? factory = null,
		FakeServiceInstaller? installer = null)
	{
		return new MountWatcherCli(
			new FakeIImport(new FakeSelectorStorage()),
			new AppSettings { TempFolder = "/temp" },
			console ?? new FakeConsoleWrapper([]),
			logger ?? new FakeIWebLogger(),
			mountDetector ?? new FakeCameraStorageDetector([]),
			factory ?? new FakeMountWatcherFactory(),
			installer ?? new FakeServiceInstaller());
	}

	private static MountWatcherCli CreateSut(
		Func<OSPlatform>? platformResolver,
		FakeConsoleWrapper? console = null,
		FakeIWebLogger? logger = null,
		ICameraStorageDetector? mountDetector = null,
		FakeMountWatcherFactory? factory = null,
		FakeServiceInstaller? installer = null)
	{
		return new MountWatcherCli(
			new FakeIImport(new FakeSelectorStorage()),
			new AppSettings { TempFolder = "/temp" },
			console ?? new FakeConsoleWrapper([]),
			logger ?? new FakeIWebLogger(),
			mountDetector ?? new FakeCameraStorageDetector([]),
			factory ?? new FakeMountWatcherFactory(),
			installer ?? new FakeServiceInstaller(),
			platformResolver);
	}

	[TestMethod]
	public async Task StartWatcher_NoArgs_ReturnsTrue()
	{
		var sut = CreateSut();
		var result = await sut.StartWatcher([]);
		Assert.IsTrue(result);
	}


	[TestMethod]
	public async Task StartWatcher_HelpArg_ShowsHelp_ReturnsTrue()
	{
		var console = new FakeConsoleWrapper([]);
		var sut = CreateSut(console);
		var result = await sut.StartWatcher(["--help"]);
		Assert.IsTrue(result);
		Assert.IsNotEmpty(console.WrittenLines);
	}

	[TestMethod]
	public async Task StartWatcher_Help_ShowsMacOSHelp_WhenPlatformIsOSX()
	{
		var console = new FakeConsoleWrapper([]);
		var sut = CreateSut(() => OSPlatform.OSX, console);
		var result = await sut.StartWatcher(["--help"]);
		Assert.IsTrue(result);
		// macOS specific help lines
		Assert.IsTrue(console.WrittenLines.Exists(l => l.Contains("macOS plist")));
		Assert.IsTrue(console.WrittenLines.Exists(l => l.Contains("Full Disk Access")));
	}

	[TestMethod]
	public async Task StartWatcher_Help_ShowsLinuxHelp_WhenPlatformIsLinux()
	{
		var console = new FakeConsoleWrapper([]);
		var sut = CreateSut(() => OSPlatform.Linux, console);
		var result = await sut.StartWatcher(["--help"]);
		Assert.IsTrue(result);
		// linux specific help lines
		Assert.IsTrue(console.WrittenLines.Exists(l => l.Contains("systemd:")));
	}

	[TestMethod]
	public async Task StartWatcher_Help_ShowsWindowsHelp_WhenPlatformIsWindows()
	{
		var console = new FakeConsoleWrapper([]);
		var sut = CreateSut(() => OSPlatform.Windows, console);
		var result = await sut.StartWatcher(["--help"]);
		Assert.IsTrue(result);
		// windows specific help lines
		Assert.IsTrue(console.WrittenLines.Exists(l => l.Contains("Windows Service")));
	}

	[TestMethod]
	public async Task StartWatcher_InstallArg_CallsInstaller()
	{
		var installer = new FakeServiceInstaller();
		var sut = CreateSut(installer: installer);
		var result = await sut.StartWatcher(["--install"]);
		Assert.IsTrue(result);
		Assert.HasCount(1, installer.InstalledPaths);
	}

	[TestMethod]
	public async Task StartWatcher_UninstallArg_CallsUninstaller()
	{
		var installer = new FakeServiceInstaller();
		var sut = CreateSut(installer: installer);
		var result = await sut.StartWatcher(["--uninstall"]);
		Assert.IsTrue(result);
		Assert.AreEqual(1, installer.UninstallCount);
	}

	[TestMethod]
	public async Task StartWatcher_InstallReturnsFalse_ReturnsFalse()
	{
		var installer = new FakeServiceInstaller { ReturnValue = false };
		var sut = CreateSut(installer: installer);
		var result = await sut.StartWatcher(["--install"]);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task StartWatcher_RunWatcherStartThrows_LogsErrorAndReturnsFalse()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var factory = new ThrowingMountWatcherFactory();
		var sut = CreateSut(new FakeConsoleWrapper([]), logger,
			new FakeCameraStorageDetector([]), factory);

		// Act
		var result = await sut.StartWatcher([]);

		// Assert
		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedExceptions.Exists(t =>
			t.Item2 != null && t.Item2.Contains("Mount watcher failed:")));
	}

	[TestMethod]
	public void NeedInstall_WithInstallArg_ReturnsTrue()
	{
		Assert.IsTrue(MountWatcherCli.NeedInstall(["--install"]));
	}

	[TestMethod]
	public void NeedInstall_WithoutInstallArg_ReturnsFalse()
	{
		Assert.IsFalse(MountWatcherCli.NeedInstall(["--verbose"]));
	}

	[TestMethod]
	public void NeedInstall_CaseInsensitive_ReturnsTrue()
	{
		Assert.IsTrue(MountWatcherCli.NeedInstall(["--INSTALL"]));
	}

	[TestMethod]
	public void NeedUninstall_WithUninstallArg_ReturnsTrue()
	{
		Assert.IsTrue(MountWatcherCli.NeedUninstall(["--uninstall"]));
	}

	[TestMethod]
	public void NeedUninstall_WithoutUninstallArg_ReturnsFalse()
	{
		Assert.IsFalse(MountWatcherCli.NeedUninstall(["--verbose"]));
	}

	[TestMethod]
	public void NormalizeMountPath_StripsTrailingSlash_ButKeepsRoot()
	{
		Assert.AreEqual("/Volumes/extreme2111",
			MountWatcherCli.NormalizeMountPath(" /Volumes/extreme2111/ "));
		Assert.AreEqual("/", MountWatcherCli.NormalizeMountPath("/"));
	}
}

internal class ThrowingMountWatcherFactory : IMountWatcherFactory
{
	public IMountWatcher CreateMountWatcher()
	{
		return new FakeMountWatcherFactory.FakeMountWatcher(new Exception("start failed"));
	}
}
