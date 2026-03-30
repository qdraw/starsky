using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class MountWatcherCliTest
{
	[TestMethod]
	public async Task MountWatcherCli_StartWatcher_ReturnsTrue()
	{
		// Arrange
		var fakeImport = new FakeIImport(new FakeSelectorStorage());
		var fakeConsole = new FakeConsoleWrapper(new List<string>());
		var fakeLogger = new FakeIWebLogger();
		var fakeExifToolDownload = new FakeExifToolDownload();
		var fakeGeoFileDownload = new FakeIGeoFileDownload();
		var fakeMountDetector = new FakeMountDetector();
		var fakeMountWatcherFactory = new FakeMountWatcherFactory();
		var fakeCameraStorageDetector = new FakeCameraStorageDetector(new List<string>());

		var sut = new MountWatcherCli(
			fakeImport,
			new AppSettings { TempFolder = "/temp" },
			fakeConsole,
			fakeLogger,
			fakeExifToolDownload,
			fakeGeoFileDownload,
			fakeMountDetector,
			fakeMountWatcherFactory,
			fakeCameraStorageDetector);

		// Act
		var result = await sut.StartWatcher([]);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task MountWatcherCli_StartWatcher_CallsExifToolDownload()
	{
		// Arrange
		var fakeExifToolDownload = new FakeExifToolDownload();
		var fakeImport = new FakeIImport(new FakeSelectorStorage());
		var fakeConsole = new FakeConsoleWrapper(new List<string>());
		var fakeLogger = new FakeIWebLogger();
		var fakeGeoFileDownload = new FakeIGeoFileDownload();
		var fakeMountDetector = new FakeMountDetector();
		var fakeMountWatcherFactory = new FakeMountWatcherFactory();
		var fakeCameraStorageDetector = new FakeCameraStorageDetector(new List<string>());

		var sut = new MountWatcherCli(
			fakeImport,
			new AppSettings { TempFolder = "/temp" },
			fakeConsole,
			fakeLogger,
			fakeExifToolDownload,
			fakeGeoFileDownload,
			fakeMountDetector,
			fakeMountWatcherFactory,
			fakeCameraStorageDetector);

		// Act
		_ = await sut.StartWatcher([]);

		// Assert
		Assert.AreEqual(1, fakeExifToolDownload.Called.Count);
	}

	[TestMethod]
	public async Task MountWatcherCli_StartWatcher_CallsGeoFileDownload()
	{
		// Arrange
		var fakeGeoFileDownload = new FakeIGeoFileDownload();
		var fakeImport = new FakeIImport(new FakeSelectorStorage());
		var fakeConsole = new FakeConsoleWrapper(new List<string>());
		var fakeLogger = new FakeIWebLogger();
		var fakeExifToolDownload = new FakeExifToolDownload();
		var fakeMountDetector = new FakeMountDetector();
		var fakeMountWatcherFactory = new FakeMountWatcherFactory();
		var fakeCameraStorageDetector = new FakeCameraStorageDetector(new List<string>());

		var sut = new MountWatcherCli(
			fakeImport,
			new AppSettings { TempFolder = "/temp" },
			fakeConsole,
			fakeLogger,
			fakeExifToolDownload,
			fakeGeoFileDownload,
			fakeMountDetector,
			fakeMountWatcherFactory,
			fakeCameraStorageDetector);

		// Act
		_ = await sut.StartWatcher([]);

		// Assert
		Assert.AreEqual(1, fakeGeoFileDownload.Count);
	}
}
