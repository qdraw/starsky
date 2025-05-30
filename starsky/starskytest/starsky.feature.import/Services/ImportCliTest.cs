using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.import.Services;

[TestClass]
public sealed class ImportCliTest
{
	[TestMethod]
	public async Task ImporterCli_CheckIfExifToolIsCalled()
	{
		var fakeExifToolDownload = new FakeExifToolDownload();
		var fakeGeoFileDownload = new FakeIGeoFileDownload();

		var fakeConsole = new FakeConsoleWrapper(new List<string>());
		var sut = new ImportCli(
			new FakeIImport(new FakeSelectorStorage()),
			new AppSettings { TempFolder = "/___not___found_" },
			fakeConsole, new FakeIWebLogger(), fakeExifToolDownload, fakeGeoFileDownload);
		await sut.Importer([]);

		Assert.AreEqual(1, fakeExifToolDownload.Called.Count);
	}

	[TestMethod]
	public async Task ImporterCli_CheckIfGeoDownloadIsCalled()
	{
		var fakeExifToolDownload = new FakeExifToolDownload();
		var fakeGeoFileDownload = new FakeIGeoFileDownload();

		var fakeConsole = new FakeConsoleWrapper(new List<string>());
		var sut = new ImportCli(
			new FakeIImport(new FakeSelectorStorage()),
			new AppSettings { TempFolder = "/___not___found_" },
			fakeConsole, new FakeIWebLogger(), fakeExifToolDownload, fakeGeoFileDownload);
		await sut.Importer([]);

		Assert.AreEqual(1, fakeGeoFileDownload.Count);
	}

	[TestMethod]
	public async Task ImporterCli_NoArgs_DefaultHelp()
	{
		var fakeConsole = new FakeConsoleWrapper(new List<string>());
		var sut = new ImportCli(
			new FakeIImport(new FakeSelectorStorage()), new AppSettings(),
			fakeConsole, new FakeIWebLogger(), new FakeExifToolDownload(),
			new FakeIGeoFileDownload());
		await sut.Importer([]);

		Assert.IsTrue(fakeConsole.WrittenLines.FirstOrDefault()
			?.Contains("Starsky Importer Cli ~ Help"));
	}

	[TestMethod]
	public async Task ImporterCli_ArgPath()
	{
		var webLogger = new FakeIWebLogger();
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test" },
			new List<byte[]>([]));

		var sut = new ImportCli(new FakeIImport(new FakeSelectorStorage(storage)),
			new AppSettings(), new FakeConsoleWrapper(), webLogger,
			new FakeExifToolDownload(), new FakeIGeoFileDownload());
		await sut.Importer(
			["-p", "/test"]);
		Assert.IsTrue(
			webLogger.TrackedInformation.Exists(p => p.Item2?.Contains("Done Importing") == true));
	}


	[TestMethod]
	public async Task ImporterCli_ArgPath_1()
	{
		var fakeConsole = new FakeConsoleWrapper(new List<string>());
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test" },
			new List<byte[]>(Array.Empty<byte[]>()));

		await new ImportCli(new FakeIImport(new FakeSelectorStorage(storage)),
				new AppSettings(), fakeConsole, new FakeIWebLogger(),
				new FakeExifToolDownload(), new FakeIGeoFileDownload())
			.Importer(
				new List<string> { "-p", "/test", "--output", "csv" }.ToArray());

		Assert.IsFalse(fakeConsole.WrittenLines.FirstOrDefault()?.Contains("Done Importing"));
		Assert.AreEqual("Id;Status;SourceFullFilePath;SubPath;FileHash",
			fakeConsole.WrittenLines.FirstOrDefault());
		Assert.AreEqual("0;FileError;~/temp/test;;FAKE", fakeConsole.WrittenLines[1]);
		Assert.AreEqual("0;FileError;~/temp/test;;FAKE", fakeConsole.WrittenLines[2]);
		Assert.AreEqual("4;Ok;/test;/test;FAKE_OK", fakeConsole.WrittenLines[3]);
	}

	[TestMethod]
	public async Task ImporterCli_ArgPath_Verbose()
	{
		var webLogger = new FakeIWebLogger();

		var fakeConsole = new FakeConsoleWrapper(new List<string>());
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test" },
			new List<byte[]>(Array.Empty<byte[]>()));

		var cli = new ImportCli(new FakeIImport(new FakeSelectorStorage(storage)),
			new AppSettings { Verbose = true }, fakeConsole, webLogger,
			new FakeExifToolDownload(), new FakeIGeoFileDownload());

		// verbose is entered here 
		await cli.Importer(new List<string> { "-p", "/test", "-v", "true" }.ToArray());

		Assert.IsTrue(
			webLogger.TrackedInformation.Exists(p => p.Item2?.Contains("Failed: 2") == true));
	}

	[TestMethod]
	public async Task ImporterCli_ArgPath_Fail()
	{
		var fakeConsole = new FakeConsoleWrapper(new List<string>());
		var webLogger = new FakeIWebLogger();
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test" },
			new List<byte[]>([])); // instead of new byte[0][]

		await new ImportCli(new FakeIImport(new FakeSelectorStorage(storage)),
				new AppSettings { Verbose = false }, fakeConsole, webLogger,
				new FakeExifToolDownload(), new FakeIGeoFileDownload())
			.Importer(new List<string> { "-p", "/test" }.ToArray());

		Assert.IsTrue(
			webLogger.TrackedInformation.Exists(p => p.Item2?.Contains("Failed") == true));
	}
}
