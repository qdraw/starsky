using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.export.Services;
using starsky.foundation.http.Streaming;
using starsky.foundation.import.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.Tenancy;

/// <summary>
///     Staging, zips and previews are in the temp folder of the tenant: a name that one tenant
///     chooses (a publish name, an upload) can not touch the files of another tenant
/// </summary>
[TestClass]
public sealed class TenantTempFolderTest
{
	private readonly string _root =
		Path.Combine(Path.GetTempPath(), "starsky-temp-" + Guid.NewGuid().ToString("N"));

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists(_root) )
		{
			Directory.Delete(_root, true);
		}
	}

	[TestMethod]
	[DataRow("/data/temp/stream_ABC", "/data/temp/", true)]
	[DataRow("/data/temp/stream_ABC", "/data/temp", true)]
	[DataRow("/data/temp/a/b", "/data/temp/", true)]
	// the temp folder itself, with or without a trailing separator
	[DataRow("/data/temp", "/data/temp/", false)]
	[DataRow("/data/temp/", "/data/temp", false)]
	// a folder that only starts with the same name
	[DataRow("/data/temp2/stream_ABC", "/data/temp/", false)]
	[DataRow("/data/tempx", "/data/temp", false)]
	// the parent, and a folder next to it
	[DataRow("/data", "/data/temp/", false)]
	[DataRow("/other/stream_ABC", "/data/temp/", false)]
	public void IsSubFolderOf(string path, string root, bool expected)
	{
		Assert.AreEqual(expected, RemoveTempAndParentStreamFolderHelper.IsSubFolderOf(path, root));
	}

	[TestMethod]
	public void RemoveTemp_FileDirectlyInTheTempFolder_NeverRemovesTheTempFolder()
	{
		var temp = Path.Combine(_root, "tenant-temp") + Path.DirectorySeparatorChar;
		Directory.CreateDirectory(temp);
		var other = Path.Combine(temp, "wedding.zip");
		File.WriteAllText(other, "zip of somebody");
		var tenant = TenantSettings.ForFolders("acme", new AppSettings(),
			Path.Combine(_root, "s"), Path.Combine(_root, "t"), temp);

		new RemoveTempAndParentStreamFolderHelper(new StorageHostFullPathFilesystem(
			new FakeIWebLogger()), new AppSettings(), tenant).RemoveTempAndParentStreamFolder(other);

		Assert.IsFalse(File.Exists(other));
		Assert.IsTrue(Directory.Exists(temp), "the temp folder itself stays");
	}

	[TestMethod]
	public void RemoveTemp_StreamFolder_IsRemoved()
	{
		var temp = Path.Combine(_root, "tenant-temp") + Path.DirectorySeparatorChar;
		var stream = Path.Combine(temp, "stream_ABC");
		Directory.CreateDirectory(stream);
		var file = Path.Combine(stream, "a.jpg");
		File.WriteAllText(file, "x");
		var tenant = TenantSettings.ForFolders("acme", new AppSettings(),
			Path.Combine(_root, "s"), Path.Combine(_root, "t"), temp);

		new RemoveTempAndParentStreamFolderHelper(new StorageHostFullPathFilesystem(
			new FakeIWebLogger()), new AppSettings(), tenant).RemoveTempAndParentStreamFolder(file);

		Assert.IsFalse(Directory.Exists(stream));
		Assert.IsTrue(Directory.Exists(temp));
	}

	[TestMethod]
	public async Task StreamFile_UsesTheGivenTempFolder()
	{
		var tenantTemp = Path.Combine(_root, "tenant-temp") + Path.DirectorySeparatorChar;
		var defaultTemp = Path.Combine(_root, "default-temp") + Path.DirectorySeparatorChar;
		var storage = new FakeIStorage();

		var paths = await FileStreamingHelper.StreamFile("image/jpeg",
			new MemoryStream(Encoding.UTF8.GetBytes("x")),
			new AppSettings { TempFolder = defaultTemp }, new FakeSelectorStorage(storage),
			"a.jpg", tenantTemp);

		Assert.AreEqual(1, paths.Count);
		Assert.IsTrue(paths[0].StartsWith(tenantTemp));
		Assert.IsFalse(paths[0].StartsWith(defaultTemp));
	}

	[TestMethod]
	public async Task StreamFile_WithoutTempFolder_UsesTheAppSettings()
	{
		var defaultTemp = Path.Combine(_root, "default-temp") + Path.DirectorySeparatorChar;

		var paths = await FileStreamingHelper.StreamFile("image/jpeg",
			new MemoryStream(Encoding.UTF8.GetBytes("x")),
			new AppSettings { TempFolder = defaultTemp }, new FakeSelectorStorage(new FakeIStorage()),
			"a.jpg");

		Assert.IsTrue(paths[0].StartsWith(defaultTemp));
	}

	[TestMethod]
	public void Export_TheZipOfAnotherTenantIsNotFound()
	{
		// the same zip name exists in the temp folder of the default tenant only
		var storage = new FakeIStorage(["/tmp", "/tmp/default-temp", "/tmp/acme-temp"],
			[Path.Combine("/tmp/default-temp", "wedding") + ".zip"]);
		var appSettings = new AppSettings { TempFolder = "/tmp/default-temp" };
		var acme = TenantSettings.ForFolders("acme", appSettings, "/tmp/acme-storage",
			"/tmp/acme-thumbs", "/tmp/acme-temp");

		var defaultExport = new ExportService(new FakeIQuery(), appSettings,
			new FakeSelectorStorage(storage), new FakeIWebLogger(), new FakeIThumbnailService());
		var acmeExport = new ExportService(new FakeIQuery(), appSettings,
			new FakeSelectorStorage(storage), new FakeIWebLogger(), new FakeIThumbnailService(),
			acme);

		var (defaultStatus, defaultPath) = defaultExport.StatusIsReady("wedding");
		var (acmeStatus, acmePath) = acmeExport.StatusIsReady("wedding");

		Assert.IsNotNull(defaultPath, "the owner finds its zip");
		Assert.AreEqual(false, defaultStatus, "no .done file yet, but the zip is known");
		Assert.IsNull(acmeStatus);
		Assert.IsNull(acmePath, "another tenant does not find that zip");
	}
}
