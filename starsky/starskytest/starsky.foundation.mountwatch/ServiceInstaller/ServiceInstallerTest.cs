using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.storage.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.ServiceInstaller;

[TestClass]
public sealed class ServiceInstallerTest
{
	public TestContext? TestContext { get; set; }

	private static global::starsky.foundation.mountwatch.ServiceInstaller.ServiceInstaller
		CreateSut()
	{
		return new global::starsky.foundation.mountwatch.ServiceInstaller.ServiceInstaller(
			new FakeSelectorStorage(),
			new FakeIWebLogger());
	}

	private static global::starsky.foundation.mountwatch.ServiceInstaller.ServiceInstaller
		CreateSut(ISelectorStorage storage, FakeIWebLogger logger,
			Func<OSPlatform> func)
	{
		return new global::starsky.foundation.mountwatch.ServiceInstaller.ServiceInstaller(
			storage,
			logger,
			func);
	}

	[TestMethod]
	public void GetMacOsPlistPath_ContainsLaunchAgents()
	{
		Assert.Contains("LaunchAgents", MacOsServiceInstaller.GetMacOsPlistPath());
	}

	[TestMethod]
	public void GetMacOsPlistPath_ContainsServiceName()
	{
		Assert.Contains("nl.qdraw.mountwatcher", MacOsServiceInstaller.GetMacOsPlistPath());
	}

	[TestMethod]
	public void GetMacOsPlistPath_EndsWithPlistExtension()
	{
		Assert.EndsWith(".plist", MacOsServiceInstaller.GetMacOsPlistPath());
	}

	[TestMethod]
	public void GenerateMacOsPlist_ContainsLabel()
	{
		Assert.Contains("nl.qdraw.mountwatcher",
			ServiceInstallerHelper.GenerateMacOsPlist(
				"/usr/local/bin/starskymountwatchercli", "nl.qdraw.mountwatcher"));
	}

	[TestMethod]
	public void GenerateMacOsPlist_ContainsExecutablePath()
	{
		const string execPath = "/usr/local/bin/starskymountwatchercli";
		Assert.Contains(execPath, ServiceInstallerHelper.GenerateMacOsPlist(execPath,
			"nl.qdraw.mountwatcher"));
	}

	[TestMethod]
	public void GenerateMacOsPlist_ContainsRunAtLoad()
	{
		Assert.Contains("RunAtLoad",
			ServiceInstallerHelper.GenerateMacOsPlist("/any/path", "nl.qdraw.mountwatcher"));
	}

	[TestMethod]
	public void GenerateMacOsPlist_ContainsKeepAlive()
	{
		Assert.Contains("KeepAlive",
			ServiceInstallerHelper.GenerateMacOsPlist("/any/path", "nl.qdraw.mountwatcher"));
	}

	[TestMethod]
	public void GenerateMacOsPlist_IsValidXml()
	{
		var plist = ServiceInstallerHelper.GenerateMacOsPlist(
			"/any/path", "nl.qdraw.mountwatcher");
		Assert.Contains("<?xml", plist);
		Assert.Contains("</plist>", plist);
	}

	[TestMethod]
	public void GenerateMacOsPlist_DifferentExecPath_ContainsCorrectPath()
	{
		const string path = "/opt/starsky/starskymountwatchercli";
		Assert.Contains(path,
			ServiceInstallerHelper.GenerateMacOsPlist(path, "nl.qdraw.mountwatcher"));
	}

	[TestMethod]
	public void GenerateLinuxSystemdUnit_ContainsExecStart()
	{
		const string execPath = "/usr/local/bin/starskymountwatchercli";
		Assert.Contains($"ExecStart={execPath}",
			ServiceInstallerHelper.GenerateLinuxSystemdUnit(execPath));
	}

	[TestMethod]
	public void GenerateLinuxSystemdUnit_ContainsRestartOnFailure()
	{
		Assert.Contains("Restart=on-failure",
			ServiceInstallerHelper.GenerateLinuxSystemdUnit("/any/path"));
	}

	[TestMethod]
	public void GenerateLinuxSystemdUnit_ContainsUnitSection()
	{
		var unit = ServiceInstallerHelper.GenerateLinuxSystemdUnit("/any/path");
		Assert.Contains("[Unit]", unit);
		Assert.Contains("[Service]", unit);
		Assert.Contains("[Install]", unit);
	}

	[TestMethod]
	public void GenerateLinuxSystemdUnit_ContainsWantedBy()
	{
		Assert.Contains("WantedBy=multi-user.target",
			ServiceInstallerHelper.GenerateLinuxSystemdUnit("/any/path"));
	}

	[TestMethod]
	public void GenerateLinuxSystemdUnit_ContainsVerboseFlag()
	{
		Assert.Contains("--verbose",
			ServiceInstallerHelper.GenerateLinuxSystemdUnit("/any/path"));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public async Task InstallAsync_MacOs_WritesPlistFile()
	{
		var ct = TestContext?.CancellationTokenSource.Token ?? default;
		var plistPath = MacOsServiceInstaller.GetMacOsPlistPath();

		try
		{
			var result = await CreateSut().InstallAsync("/usr/local/bin/test");
			Assert.IsTrue(result);
			Assert.IsTrue(File.Exists(plistPath));
			Assert.Contains("/usr/local/bin/test",
				await File.ReadAllTextAsync(plistPath, ct));
		}
		finally
		{
			if ( File.Exists(plistPath) )
			{
				File.Delete(plistPath);
			}
		}
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public async Task UninstallAsync_MacOs_RemovesPlistFile()
	{
		var ct = TestContext?.CancellationTokenSource.Token ?? CancellationToken.None;
		var plistPath = MacOsServiceInstaller.GetMacOsPlistPath();
		Directory.CreateDirectory(Path.GetDirectoryName(plistPath)!);
		await File.WriteAllTextAsync(plistPath, "<plist/>", ct);

		Assert.IsTrue(await CreateSut().UninstallAsync());
		Assert.IsFalse(File.Exists(plistPath));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public async Task UninstallAsync_MacOs_WhenNotInstalled_ReturnsTrue()
	{
		var plistPath = MacOsServiceInstaller.GetMacOsPlistPath();
		if ( File.Exists(plistPath) )
		{
			File.Delete(plistPath);
		}

		Assert.IsTrue(await CreateSut().UninstallAsync());
	}

	[TestMethod]
	public void WatchServiceName_GetReverseDnsName_IsNotEmpty()
	{
		Assert.IsFalse(string.IsNullOrWhiteSpace(WatchServiceName.GetReverseDnsName()));
	}

	[TestMethod]
	public void WatchServiceName_GetSystemDName_IsNotEmpty()
	{
		Assert.IsFalse(string.IsNullOrWhiteSpace(WatchServiceName.GetSystemDName()));
	}

	[TestMethod]
	public void WatchServiceName_GetDisplayName_IsNotEmpty()
	{
		Assert.IsFalse(string.IsNullOrWhiteSpace(WatchServiceName.GetDisplayName()));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Windows)]
	public async Task ServiceInstaller_StopAsync_Windows_CallsWindowsStopper()
	{
		var logger = new FakeIWebLogger();
		var installer = CreateSut(new FakeSelectorStorage(), logger,
			() => OSPlatform.Windows);

		var result = await installer.StopAsync();

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void ServiceInstaller_CreateInstaller_UnsupportedOs_Throws()
	{
		var logger = new FakeIWebLogger();
		var installer = CreateSut(new FakeSelectorStorage(), logger,
			() => OSPlatform.Create("Unknown"));
		AggregateException? exception = null;
		try
		{
			_ = installer.InstallAsync("/any").Result;
			Assert.Fail("Should have thrown PlatformNotSupportedException");
		}
		catch ( AggregateException ex )
		{
			exception = ex;
		}

		Assert.IsNotNull(exception);
		Assert.IsTrue(exception.InnerException is PlatformNotSupportedException,
			$"Expected PlatformNotSupportedException but got {exception.InnerException?.GetType().Name}");
	}
}
