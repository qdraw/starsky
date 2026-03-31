using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class ServiceInstallerTest
{
	public TestContext? TestContext { get; set; }

	private static ServiceInstaller CreateSut()
	{
		return new ServiceInstaller(
			new FakeConsoleWrapper(new List<string>()),
			new FakeIWebLogger());
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
		var ct = TestContext?.CancellationTokenSource.Token ?? default;
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
	[OSCondition(OperatingSystems.Linux)]
	public async Task InstallAsync_Linux_WritesFallbackUserUnit()
	{
		Assert.IsTrue(
			await CreateSut().InstallAsync("/usr/local/bin/starskymountwatchercli"));
	}
}
