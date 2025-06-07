using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Structure;
using starsky.foundation.thumbnailgeneration.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Helpers;

[TestClass]
public sealed class ThumbnailCliTest
{
	[TestMethod]
	public async Task Thumbnail_NoArgs_Assume_T_True()
	{
		var fakeConsole = new FakeConsoleWrapper();
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var thumbnailService = new ThumbnailCli(new AppSettings(), fakeConsole,
			new FakeIThumbnailService(new FakeSelectorStorage(storage)),
			new FakeIThumbnailCleaner(),
			new FakeSelectorStorage(storage));

		await thumbnailService.Thumbnail(Array.Empty<string>());

		Assert.AreEqual(1, fakeConsole.WrittenLines.Count);
	}

	[TestMethod]
	public async Task Thumbnail_Enable_T_Param_AssumeHome()
	{
		var fakeConsole = new FakeConsoleWrapper();
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var fakeIThumbnailService = new FakeIThumbnailService(new FakeSelectorStorage(storage));
		var thumbnailService = new ThumbnailCli(new AppSettings(), fakeConsole,
			fakeIThumbnailService, new FakeIThumbnailCleaner(),
			new FakeSelectorStorage(storage));

		await thumbnailService.Thumbnail(["-t", "true"]);

		Assert.AreEqual("/", fakeIThumbnailService.Inputs[0].Item1);
	}

	[TestMethod]
	public async Task Thumbnail_Help()
	{
		var fakeConsole = new FakeConsoleWrapper();
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var thumbnailService = new ThumbnailCli(new AppSettings(), fakeConsole,
			new FakeIThumbnailService(), new FakeIThumbnailCleaner(),
			new FakeSelectorStorage(storage));

		await thumbnailService.Thumbnail(["-h"]);

		Assert.IsTrue(fakeConsole.WrittenLines[0].Contains("Help"));
	}

	[TestMethod]
	public async Task Thumbnail_Disable_T_Param()
	{
		var fakeThumbnail = new FakeIThumbnailService();
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });

		var thumbnailService = new ThumbnailCli(new AppSettings(), new ConsoleWrapper(),
			fakeThumbnail, new FakeIThumbnailCleaner(),
			new FakeSelectorStorage(storage));

		await thumbnailService.Thumbnail(["-t", "false"]);

		Assert.AreEqual(0, fakeThumbnail.Inputs.Count);
	}

	[TestMethod]
	public async Task Thumbnail_MinusP_FullPath()
	{
		var fakeConsole = new FakeConsoleWrapper();
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var fakeIThumbnailService = new FakeIThumbnailService(new FakeSelectorStorage(storage));
		var appSettings = new AppSettings();
		var thumbnailService = new ThumbnailCli(appSettings, fakeConsole,
			fakeIThumbnailService, new FakeIThumbnailCleaner(),
			new FakeSelectorStorage(storage));

		await thumbnailService.Thumbnail([
			"-v", "true", "-t", "true", "-p", Path.Combine(appSettings.StorageFolder, "test")
		]);

		Assert.AreEqual("/test", fakeIThumbnailService.Inputs[0].Item1);
	}

	[TestMethod]
	public async Task Thumbnail_MinusS_SubPath()
	{
		var fakeConsole = new FakeConsoleWrapper();
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var fakeIThumbnailService = new FakeIThumbnailService(new FakeSelectorStorage(storage));
		var appSettings = new AppSettings();
		var thumbnailService = new ThumbnailCli(appSettings, fakeConsole,
			fakeIThumbnailService, new FakeIThumbnailCleaner(),
			new FakeSelectorStorage(storage));

		await thumbnailService.Thumbnail(["-t", "true", "-s", "/test"]);

		Assert.AreEqual("/test", fakeIThumbnailService.Inputs[0].Item1);
	}

	[TestMethod]
	public async Task Thumbnail_MinusS_SubPath_Direct()
	{
		var fakeConsole = new FakeConsoleWrapper();
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var fakeIThumbnailService = new FakeIThumbnailService(new FakeSelectorStorage(storage));
		var appSettings = new AppSettings();
		var thumbnailService = new ThumbnailCli(appSettings, fakeConsole,
			fakeIThumbnailService, new FakeIThumbnailCleaner(),
			new FakeSelectorStorage(storage));

		await thumbnailService.Thumbnail(["-t", "true", "-s", "/test.jpg"]);

		Assert.AreEqual("/test.jpg", fakeIThumbnailService.Inputs[0].Item1);

		Assert.AreEqual("/test.jpg", fakeIThumbnailService.Inputs[0].Item1);
		Assert.IsNull(fakeIThumbnailService.Inputs[0].Item2);
	}

	[TestMethod]
	public async Task Thumbnail_MinusG_Relative()
	{
		var fakeConsole = new FakeConsoleWrapper();
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var fakeIThumbnailService = new FakeIThumbnailService(new FakeSelectorStorage(storage));
		var appSettings = new AppSettings();
		var thumbnailService = new ThumbnailCli(appSettings, fakeConsole,
			fakeIThumbnailService, new FakeIThumbnailCleaner(),
			new FakeSelectorStorage(storage));

		await thumbnailService.Thumbnail(["-t", "true", "-g", "0"]);

		var subPathRelative = new StructureService(new FakeSelectorStorage(), appSettings.Structure)
			.ParseSubfolders(0);

		Assert.AreEqual(subPathRelative, fakeIThumbnailService.Inputs[0].Item1);
	}

	[TestMethod]
	public async Task Thumbnail_MinusX_CleanAllUnusedFiles()
	{
		var fakeConsole = new FakeConsoleWrapper();
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var fakeIThumbnailCleaner = new FakeIThumbnailCleaner();
		var thumbnailService = new ThumbnailCli(new AppSettings(), fakeConsole,
			new FakeIThumbnailService(new FakeSelectorStorage(storage)), fakeIThumbnailCleaner,
			new FakeSelectorStorage(storage));

		await thumbnailService.Thumbnail(["--clean", "true"]);

		Assert.IsTrue(fakeIThumbnailCleaner.Inputs[0]);
	}
}
