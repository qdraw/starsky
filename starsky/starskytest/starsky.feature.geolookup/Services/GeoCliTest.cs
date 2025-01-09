using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
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
		var geoCli = new GeoCli(new FakeIGeoReverseLookup(), new FakeIGeoLocationWrite(),
			new FakeSelectorStorage(new FakeIStorage(new List<string>())), new AppSettings(),
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(new List<string> { "-p" }.ToArray());

		Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("not found"));
	}

	[TestMethod]
	public async Task GeoCliInput_RelativeUrl_HappyFlow()
	{
		var relativeParentFolder = new AppSettings().DatabasePathToFilePath(
			new StructureService(new FakeIStorage(), new AppSettings().Structure)
				.ParseSubfolders(0)!);

		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var appSettings = new AppSettings();
		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoReverseLookup();
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), appSettings,
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(new List<string> { "-g", "0" }.ToArray());

		Assert.AreEqual(appSettings.StorageFolder,
			relativeParentFolder + Path.DirectorySeparatorChar);
		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile("/test.jpg"));
	}

	[TestMethod]
	public async Task GeoCliInput_AbsolutePath_HappyFlow()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var appSettings = new AppSettings { Verbose = true };
		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoReverseLookup();
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), appSettings,
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(new List<string> { "-p", "/test" }.ToArray());

		Assert.AreEqual(appSettings.StorageFolder, "/test" + Path.DirectorySeparatorChar);
		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile("/test.jpg"));
	}

	[TestMethod]
	public async Task GeoCliInput_Default_HappyFlow()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var hash = ( await new FileHash(storage).GetHashCodeAsync("/test.jpg") ).Key;
		storage.FileCopy("/test.jpg", $"/{hash}.jpg");

		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoReverseLookup();
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), new AppSettings(),
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(new List<string> { "-p" }.ToArray());

		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile($"/{hash}.jpg"));
		Assert.IsTrue(storage.ExistFile("/test.jpg"));
	}

	[TestMethod]
	public async Task GeoCliInput_Default_HappyFlow_ShouldMoveFile()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg", "1" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });
		var hash = ( await new FileHash(storage).GetHashCodeAsync("/test.jpg") ).Key;
		storage.FileCopy("/test.jpg", $"/{hash}.jpg");

		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoReverseLookup(new List<FileIndexItem>
		{
			new("/test.jpg") { Latitude = 50, Longitude = 4, FileHash = "1" }
		});
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), new AppSettings(),
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(new List<string> { "-p" }.ToArray());

		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile($"/{hash}.{new AppSettings().ThumbnailImageFormat}"));
	}


	[TestMethod]
	public async Task GeoCliInput_Default_HappyFlow_ShouldMoveFile_Verbose()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg", "1" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });
		var hash = ( await new FileHash(storage).GetHashCodeAsync("/test.jpg") ).Key;
		storage.FileCopy("/test.jpg", $"/{hash}.jpg");

		var geoWrite = new FakeIGeoLocationWrite();
		var geoLookup = new FakeIGeoReverseLookup(new List<FileIndexItem>
		{
			new("/test.jpg") { Latitude = 50, Longitude = 4, FileHash = "1" }
		});
		var console = new FakeConsoleWrapper();
		var geoCli = new GeoCli(geoLookup, geoWrite,
			new FakeSelectorStorage(storage), new AppSettings(),
			console, new FakeIGeoFileDownload(), new FakeExifToolDownload(), new FakeIWebLogger());
		await geoCli.CommandLineAsync(new List<string> { "-p", "-v" }.ToArray());

		Assert.AreEqual(1, geoLookup.Count);
		Assert.IsTrue(storage.ExistFile($"/{hash}.{new AppSettings().ThumbnailImageFormat}"));
	}
}
