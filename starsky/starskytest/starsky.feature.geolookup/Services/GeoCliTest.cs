using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Structure;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services;

[TestClass]
public sealed class GeoCliTest
{
	[TestMethod]
	public async Task GeoCliInput_Notfound()
	{
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(new FakeIGeoFolderReverseLookup(), new FakeIGeoLocationWrite(),
			new FakeSelectorStorage(new FakeIStorage([])), new AppSettings(),
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(["-p"]);

		Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("not found"));
	}

	[TestMethod]
	public async Task GeoCliInput_RelativeUrl_HappyFlow()
	{
		var relativeParentFolder = new AppSettings().DatabasePathToFilePath(
			new StructureService(new FakeSelectorStorage(), new AppSettings(), new FakeIWebLogger())
				.ParseSubfolders(0)!);

		var storage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var appSettings = new AppSettings();
		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoFolderReverseLookup();
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), appSettings,
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(["-g", "0"]);

		Assert.AreEqual(appSettings.StorageFolder,
			relativeParentFolder + Path.DirectorySeparatorChar);
		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile("/test.jpg"));
	}

	[TestMethod]
	public async Task GeoCliInput_AbsolutePath_HappyFlow()
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var appSettings = new AppSettings { Verbose = true };
		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoFolderReverseLookup();
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), appSettings,
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(["-p", "/test"]);

		Assert.AreEqual(appSettings.StorageFolder, "/test" + Path.DirectorySeparatorChar);
		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile("/test.jpg"));
	}

	[TestMethod]
	public async Task GeoCliInput_Default_HappyFlow()
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var hash =
			( await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync("/test.jpg",
				ExtensionRolesHelper.ImageFormat.jpg) ).Key;
		storage.FileCopy("/test.jpg", $"/{hash}.jpg");

		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoFolderReverseLookup();
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), new AppSettings(),
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(["-p"]);

		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile($"/{hash}.jpg"));
		Assert.IsTrue(storage.ExistFile("/test.jpg"));
	}

	[TestMethod]
	public async Task GeoCliInput_Default_HappyFlow_ShouldMoveFile()
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg", "1"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });
		var hash =
			( await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync(
				"/test.jpg",
				ExtensionRolesHelper.ImageFormat.jpg) ).Key;
		storage.FileCopy("/test.jpg", $"/{hash}.jpg");

		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoFolderReverseLookup([
			new("/test.jpg") { Latitude = 50, Longitude = 4, FileHash = "1" }
		]);
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), new AppSettings(),
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(["-p"]);

		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile($"/{hash}.jpg"));
	}


	[TestMethod]
	public async Task GeoCliInput_Default_HappyFlow_ShouldMoveFile_Verbose()
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg", "1"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });
		var hash =
			( await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync(
				"/test.jpg",
				ExtensionRolesHelper.ImageFormat.jpg) ).Key;
		storage.FileCopy("/test.jpg", $"/{hash}.jpg");

		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoFolderReverseLookup([
			new("/test.jpg") { Latitude = 50, Longitude = 4, FileHash = "1" }
		]);
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), new AppSettings(),
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(["-p", "-v"]);

		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile($"/{hash}.jpg"));
	}
}
