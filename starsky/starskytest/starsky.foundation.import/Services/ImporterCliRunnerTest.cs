using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public class ImporterCliRunnerTest
{
	[TestMethod]
	public void ResolveImporterPath_UsesOverride_WhenExists()
	{
		var baseDir = new AppSettings().BaseDirectoryProject;
		var overridePath = Path.Combine(baseDir, "custom", "starskyimportercli");
		var storage = new FakeIStorage([], [overridePath]);
		var appSettings = new AppSettings { MountWatcherImporterPath = overridePath };
		var sut = new ImporterCliRunner(appSettings, new FakeSelectorStorage(storage),
			new FakeIWebLogger());

		var result = sut.ResolveImporterPath();

		Assert.AreEqual(overridePath, result);
	}

	[TestMethod]
	public void ResolveImporterPath_FallsBackToBinCandidate()
	{
		var baseDir = new AppSettings().BaseDirectoryProject;
		var candidate = Path.Combine(baseDir, "bin", "starskyimportercli");
		var storage = new FakeIStorage([], [candidate]);
		var sut = new ImporterCliRunner(new AppSettings(), new FakeSelectorStorage(storage),
			new FakeIWebLogger());

		var result = sut.ResolveImporterPath();

		Assert.AreEqual(candidate, result);
	}

	[TestMethod]
	public void BuildCameraImportArguments_UsesDefault_WhenUnsafe()
	{
		var appSettings = new AppSettings { MountWatcherImporterArguments = "--camera; rm -rf /" };

		var result = ImporterCliRunner.BuildCameraImportArguments(appSettings);

		Assert.AreEqual(ImporterCliRunner.DefaultCameraArguments, result);
	}

	[TestMethod]
	public void BuildCameraImportArguments_UsesConfigured_WhenAllowed()
	{
		var appSettings = new AppSettings { MountWatcherImporterArguments = "--camera --recursive --move" };

		var result = ImporterCliRunner.BuildCameraImportArguments(appSettings);

		Assert.AreEqual("--camera --recursive --move", result);
	}
}

