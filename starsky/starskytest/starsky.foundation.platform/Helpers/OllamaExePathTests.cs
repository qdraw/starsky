using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class OllamaExePathTests
{
	private readonly AppSettings _appSettings;
	private readonly string _testDependenciesFolder;

	public OllamaExePathTests()
	{
		_testDependenciesFolder = Path.Combine(Path.GetTempPath(), "starsky_ollama_exe_path_tests",
			Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_testDependenciesFolder);
		_appSettings = new AppSettings { DependenciesFolder = _testDependenciesFolder };
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists(_testDependenciesFolder) )
		{
			Directory.Delete(_testDependenciesFolder, true);
		}
	}

	[TestMethod]
	[DataRow(null, "ollama")]
	[DataRow("", "ollama")]
	[DataRow("win-x64", "ollama-win-x64")]
	[DataRow("linux-x64", "ollama-linux-x64")]
	[DataRow("osx-arm64", "ollama-osx-arm64")]
	public void GetExeParentFolder_CurrentArchitecture(string? arch, string expectedFolder)
	{
		var sut = new OllamaExePath(_appSettings);
		var expectedPath = Path.Combine(_testDependenciesFolder, expectedFolder);
		var result = sut.GetExeParentFolder(arch ?? string.Empty);
		Assert.AreEqual(expectedPath, result);
	}

	[TestMethod]
	[DataRow("win-x64", "ollama.exe")]
	[DataRow("win-arm64", "ollama.exe")]
	[DataRow("linux-x64", "ollama")]
	[DataRow("osx-x64", "ollama")]
	public void GetExePath_ShouldReturnCorrectPath(string architecture, string expectedFileName)
	{
		var sut = new OllamaExePath(_appSettings);
		var expectedPath = Path.Combine(_testDependenciesFolder,
			$"ollama-{architecture}", expectedFileName);
		var result = sut.GetExePath(architecture);
		Assert.AreEqual(expectedPath, result);
	}

	[TestMethod]
	public void GetConfiguredOrDefaultPath_ShouldUseConfiguredPath_WhenFileExists()
	{
		var configuredFolder = Path.Combine(_testDependenciesFolder, "custom");
		Directory.CreateDirectory(configuredFolder);
		var configuredPath = Path.Combine(configuredFolder, "ollama");
		File.WriteAllText(configuredPath, "test");
		_appSettings.OllamaExecutablePath = configuredPath;

		var sut = new OllamaExePath(_appSettings);
		var result = sut.GetConfiguredOrDefaultPath("linux-x64");

		Assert.AreEqual(configuredPath, result);
	}

	[TestMethod]
	public void GetConfiguredOrDefaultPath_ShouldFallbackToAliasPath_WhenAliasExists()
	{
		var aliasFolder = Path.Combine(_testDependenciesFolder, "ollama-linux-amd64");
		Directory.CreateDirectory(aliasFolder);
		var aliasPath = Path.Combine(aliasFolder, "ollama");
		File.WriteAllText(aliasPath, "test");
		_appSettings.OllamaExecutablePath = Path.Combine(_testDependenciesFolder, "missing", "ollama");

		var sut = new OllamaExePath(_appSettings);
		var result = sut.GetConfiguredOrDefaultPath("linux-x64");

		Assert.AreEqual(aliasPath, result);
	}

	[TestMethod]
	public void GetConfiguredOrDefaultPath_ShouldReturnDefault_WhenNoFileExists()
	{
		_appSettings.OllamaExecutablePath = Path.Combine(_testDependenciesFolder, "missing", "ollama");
		var sut = new OllamaExePath(_appSettings);

		var result = sut.GetConfiguredOrDefaultPath("linux-x64");
		var expectedPath = Path.Combine(_testDependenciesFolder, "ollama-linux-x64", "ollama");

		Assert.AreEqual(expectedPath, result);
	}
}

