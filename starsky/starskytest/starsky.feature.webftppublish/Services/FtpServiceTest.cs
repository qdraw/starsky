using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Models;
using starsky.feature.webftppublish.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webftppublish.Services;

[TestClass]
public sealed class FtpServiceTest
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _storage;

	public FtpServiceTest()
	{
		_appSettings = new AppSettings { WebFtp = "ftp://test:test@testmedia.be" };
		_storage = new FakeIStorage(["/", "//large", "/large"]);
	}

	[TestMethod]
	public void CreateListOfRemoteDirectories_default()
	{
		var item = new FtpService(_appSettings, _storage, new FakeConsoleWrapper(),
				new FakeIFtpWebRequestFactory(), new FakeIWebLogger())
			.CreateListOfRemoteDirectories("/", "item-name",
				new Dictionary<string, bool>()).ToList();

		Assert.AreEqual("ftp://testmedia.be/", item[0]);
		Assert.AreEqual("ftp://testmedia.be//item-name", item[1]);
	}

	[TestMethod]
	public void CreateListOfRemoteDirectories_default_useCopyContent()
	{
		var item = new FtpService(_appSettings, _storage, new FakeConsoleWrapper(),
				new FakeIFtpWebRequestFactory(), new FakeIWebLogger())
			.CreateListOfRemoteDirectories("/", "item-name",
				new Dictionary<string, bool> { { "large/test.jpg", true } }).ToList();

		// start with index 2
		Assert.AreEqual("ftp://testmedia.be//item-name//large", item[2]);
	}

	[TestMethod]
	public void CreateListOfRemoteFilesTest()
	{
		var copyContent = new Dictionary<string, bool> { { "/test.jpg", true } };
		var item = FtpService.CreateListOfRemoteFiles(copyContent).ToList();

		Assert.AreEqual("//test.jpg", item.FirstOrDefault());
	}

	[TestMethod]
	public void DoesFtpDirectoryExist()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var item = new FtpService(_appSettings, _storage, new FakeConsoleWrapper(), factory,
			new FakeIWebLogger());

		var result = item.DoesFtpDirectoryExist("/");

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void DoesFtpDirectoryExist_NonExist()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var item = new FtpService(_appSettings, _storage, new FakeConsoleWrapper(), factory,
			new FakeIWebLogger());

		var result = item.DoesFtpDirectoryExist("/web-exception");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CreateFtpDirectory()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var item = new FtpService(_appSettings, _storage, new FakeConsoleWrapper(), factory,
			new FakeIWebLogger());

		var result = item.CreateFtpDirectory("/new-folder");

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void CreateFtpDirectory_Fail()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var ftpService =
			new FtpService(_appSettings, _storage, new FakeConsoleWrapper(), factory,
				new FakeIWebLogger());

		var result = ftpService.CreateFtpDirectory("/web-exception");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void MakeUpload_Fail_FileNotFound()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var ftpService =
			new FtpService(_appSettings, _storage, new FakeConsoleWrapper(), factory,
				new FakeIWebLogger());
		// And Fail
		var makeUpload = ftpService.MakeUpload("/", "test", new List<string> { "/test" });
		Assert.IsFalse(makeUpload);
	}

	[TestMethod]
	public void MakeUpload_AndFile_Is_Found()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var fakeStorage = new FakeIStorage(["/"],
			["//test.jpg"], new List<byte[]> { Array.Empty<byte>() });
		var ftpService = new FtpService(_appSettings, fakeStorage, new FakeConsoleWrapper(),
			factory, new FakeIWebLogger());
		var makeUpload = ftpService.MakeUpload("/", "test", new List<string> { "/test.jpg" });
		Assert.IsTrue(makeUpload);
	}

	[TestMethod]
	public void Run_UploadDone()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var fakeStorage = new FakeIStorage(["/"],
			["//test.jpg"], new List<byte[]> { Array.Empty<byte>() });
		var ftpService = new FtpService(_appSettings, fakeStorage, new FakeConsoleWrapper(),
			factory, new FakeIWebLogger());
		var makeUpload = ftpService.Run("/", "test",
			new Dictionary<string, bool> { { "test.jpg", true } });
		Assert.IsTrue(makeUpload);
	}

	[TestMethod]
	public void Run_UploadFail()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var ftpService =
			new FtpService(_appSettings, _storage, new FakeConsoleWrapper(), factory,
				new FakeIWebLogger());
		var makeUpload = ftpService.Run("/", "test",
			new Dictionary<string, bool> { { "non-existing-file.jpg", true } });
		Assert.IsFalse(makeUpload);
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_NullOrEmpty_ReturnsNull()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var ftpService = new FtpService(_appSettings, _storage, new FakeConsoleWrapper(), factory,
			new FakeIWebLogger());
		Assert.IsNull(await ftpService.IsValidZipOrFolder(null!));
		Assert.IsNull(await ftpService.IsValidZipOrFolder(""));
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_Deleted_ReturnsNull()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var fakeStorage = new FakeIStorage([], [], []);
		var ftpService = new FtpService(_appSettings, fakeStorage, new FakeConsoleWrapper(),
			factory, new FakeIWebLogger());
		Assert.IsNull(await ftpService.IsValidZipOrFolder("/deleted"));
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_FolderWithSettings_ReturnsManifest()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var manifest = new FtpPublishManifestModel { Slug = "test" };
		var manifestJson = JsonSerializer.Serialize(manifest);
		var fakeStorage = new FakeIStorage([
				$"{Path.DirectorySeparatorChar}folder"
			],
			[$"{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}_settings.json"],
			[System.Text.Encoding.UTF8.GetBytes(manifestJson)]);
		var ftpService = new FtpService(_appSettings, fakeStorage, new FakeConsoleWrapper(),
			factory, new FakeIWebLogger());
		var result = await ftpService.IsValidZipOrFolder($"{Path.DirectorySeparatorChar}folder");
		Assert.IsNotNull(result);
		Assert.AreEqual("test", result.Slug);
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_FolderWithoutSettings_ReturnsNull()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var fakeStorage = new FakeIStorage(["/folder"], [], []);
		var ftpService = new FtpService(_appSettings, fakeStorage, new FakeConsoleWrapper(),
			factory, new FakeIWebLogger());
		Assert.IsNull(await ftpService.IsValidZipOrFolder("/folder"));
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_FileNotZip_ReturnsNull()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var fakeStorage = new FakeIStorage(["/"], ["/file.txt"],
			["not a zip"u8.ToArray()]);
		var ftpService = new FtpService(_appSettings, fakeStorage, new FakeConsoleWrapper(),
			factory, new FakeIWebLogger());
		Assert.IsNull(await ftpService.IsValidZipOrFolder("/file.txt"));
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_FileInvalidZip_ReturnsNull()
	{
		var factory = new FakeIFtpWebRequestFactory();
		var fakeStorage =
			new FakeIStorage(["/"],
				["/file.zip"], [[0x00, 0x01, 0x02, 0x03]]);
		var ftpService = new FtpService(_appSettings, fakeStorage, new FakeConsoleWrapper(),
			factory, new FakeIWebLogger());
		Assert.IsNull(await ftpService.IsValidZipOrFolder("/file.zip"));
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_FileZipWithSettings_ReturnsManifest()
	{
		var logger = new FakeIWebLogger();
		var factory = new FakeIFtpWebRequestFactory();
		var manifest = new FtpPublishManifestModel { Slug = "test" };
		var manifestJson = JsonSerializer.Serialize(manifest);
		// Create zip in memory
		using var memoryStreamZip = new MemoryStream();
		using ( var zip =
		       new ZipArchive(memoryStreamZip, ZipArchiveMode.Create,
			       true) )
		{
			var entry = zip.CreateEntry("_settings.json");
			await using var writer = new StreamWriter(entry.Open());
			await writer.WriteAsync(manifestJson);
		}

		var tempFilePath = Path.Join(Path.GetTempPath(),
			"IsValidZipOrFolder_FileZipWithSettings_ReturnsManifest.zip");
		try
		{
			var hostFullPathFilesystem = new StorageHostFullPathFilesystem(logger);
			await hostFullPathFilesystem.WriteStreamAsync(memoryStreamZip, tempFilePath);

			var ftpService = new FtpService(_appSettings, hostFullPathFilesystem,
				new FakeConsoleWrapper(),
				factory, new FakeIWebLogger());
			var result = await ftpService.IsValidZipOrFolder(tempFilePath);
			Assert.IsNotNull(result);
			Assert.AreEqual("test", result.Slug);
		}
		finally
		{
			File.Delete(tempFilePath);
		}
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_FileZipWithoutSettings_ReturnsNull()
	{
		var logger = new FakeIWebLogger();
		var factory = new FakeIFtpWebRequestFactory();
		using var memoryStreamZip = new MemoryStream();
		using ( var zip =
		       new ZipArchive(memoryStreamZip, ZipArchiveMode.Create,
			       true) )
		{
			zip.CreateEntry("not_settings.txt");
		}

		var tempFilePath = Path.Join(Path.GetTempPath(),
			"IsValidZipOrFolder_FileZipWithoutSettings_ReturnsNull.zip");

		try
		{
			var hostFullPathFilesystem = new StorageHostFullPathFilesystem(logger);
			await hostFullPathFilesystem.WriteStreamAsync(memoryStreamZip, tempFilePath);

			var ftpService = new FtpService(_appSettings, hostFullPathFilesystem,
				new FakeConsoleWrapper(),
				factory, new FakeIWebLogger());
			var result = await ftpService.IsValidZipOrFolder(tempFilePath);
			Assert.IsNull(result);
		}
		finally
		{
			File.Delete(tempFilePath);
		}
	}
}
