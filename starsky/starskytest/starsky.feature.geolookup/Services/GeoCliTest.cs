using starsky.foundation.platform.Tenancy;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
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

	private static async Task<(FakeConsoleWrapper Console, FakeIGeoFolderReverseLookup Lookup,
		AppSettings Settings, FakeIStorage Storage)> RunTenant(string[] args)
	{
		var storage = new FakeIStorage(["/", "/album"],
			["/album/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var settings = new AppSettings();
		var lookup = new FakeIGeoFolderReverseLookup();
		var console = new FakeConsoleWrapper();
		TenantAmbient.Set("acme");
		try
		{
			await new GeoCli(lookup, new FakeIGeoLocationWrite(), new FakeSelectorStorage(storage),
				settings, console, new FakeIGeoFileDownload(), new FakeExifToolDownload(),
				new FakeIWebLogger()).CommandLineAsync(args);
		}
		finally
		{
			TenantAmbient.Reset();
		}

		return ( console, lookup, settings, storage );
	}

	[TestMethod]
	public async Task GeoCli_WithTenant_SubPath_WorksInTheStorageOfTheTenant()
	{
		var originalStorageFolder = new AppSettings().StorageFolder;

		var (console, lookup, settings, _) = await RunTenant(["-s", "/album"]);

		Assert.AreEqual(1, lookup.Count);
		Assert.IsFalse(console.WrittenLines.Exists(p => p.Contains("not found")));
		// the storage folder of the session is the one of the default tenant: left alone
		Assert.AreEqual(originalStorageFolder, settings.StorageFolder);
	}

	[TestMethod]
	public async Task GeoCli_WithTenant_RelativeDays_UsesTheStorageOfTheTenant()
	{
		var originalStorageFolder = new AppSettings().StorageFolder;

		// the folder of today does not exist in the storage of the tenant
		var (console, lookup, settings, _) = await RunTenant(["-g", "0"]);

		Assert.AreEqual(0, lookup.Count);
		Assert.IsTrue(console.WrittenLines.Exists(p => p.Contains("not found")));
		Assert.AreEqual(originalStorageFolder, settings.StorageFolder);
	}

	[TestMethod]
	public async Task GeoCli_WithTenant_SubPathNotFound()
	{
		var (console, lookup, _, _) = await RunTenant(["-s", "/missing"]);

		Assert.AreEqual(0, lookup.Count);
		Assert.IsTrue(console.WrittenLines.Exists(p =>
			p.Contains("not found") && p.Contains("acme:/missing")));
	}

	[TestMethod]
	[DataRow("-p")]
	[DataRow("--path")]
	public async Task GeoCli_WithTenant_FullPath_IsRefused(string option)
	{
		var (console, lookup, _, _) = await RunTenant([option, "/album"]);

		Assert.AreEqual(0, lookup.Count);
		Assert.Contains("can not be used with --tenant", console.WrittenLines.Last());
	}

	[TestMethod]
	public async Task GeoCli_WithTenant_NoFolder_ShowsHelp()
	{
		var (console, lookup, _, _) = await RunTenant([]);

		Assert.AreEqual(0, lookup.Count);
		Assert.IsFalse(console.WrittenLines.Exists(p => p.Contains("not found")));
	}
}
