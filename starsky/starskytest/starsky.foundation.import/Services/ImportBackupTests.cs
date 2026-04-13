using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.import.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public sealed class ImportBackupTests
{
	[TestMethod]
	public async Task CopyStreamFromHostToBackup_ReturnsNull_WhenDisabled()
	{
		var storage = new FakeIStorage(["/"], ["/test.jpg"],
			new List<byte[]> { new byte[] { 1, 2, 3 } });
		var selector = new FakeSelectorStorage(storage);
		var logger = new FakeIWebLogger();

		var sut = new ImportBackup(selector, logger);

		var importIndexItem = new ImportIndexItem
		{
			SourceFullFilePath = "/test.jpg",
			FileIndexItem = new FileIndexItem
			{
				FileName = "test.jpg",
				DateTime = new DateTime(2020, 1, 2,
					3, 4, 5, DateTimeKind.Local),
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		};

		var importBackup = new AppSettingsImportBackupModel
		{
			Enabled = false, StorageFolder = "/backup"
		};

		var result = await sut.CopyStreamFromHostToBackup(importIndexItem, importBackup);
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task CopyStreamFromHostToBackup_WritesBackupAndReturnsTrue_WhenSizesMatch()
	{
		var sourceBytes = new byte[] { 1, 2, 3, 4, 5 };
		var storage = new FakeIStorage(["/", "/backup"],
			["/test.jpg"], new List<byte[]> { sourceBytes });
		var selector = new FakeSelectorStorage(storage);
		var logger = new FakeIWebLogger();

		var sut = new ImportBackup(selector, logger);

		var importIndexItem = new ImportIndexItem
		{
			SourceFullFilePath = "/test.jpg",
			FileIndexItem = new FileIndexItem
			{
				FileName = "test.jpg",
				DateTime = new DateTime(2020, 1, 2,
					3, 4, 5, DateTimeKind.Local),
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		};

		var importBackup = new AppSettingsImportBackupModel
		{
			Enabled = true, StorageFolder = "/backup"
		};

		var result = await sut.CopyStreamFromHostToBackup(importIndexItem, importBackup);
		Assert.IsTrue(result);

		// Expect a file created under /backup with size same as source
		var files = storage.GetAllFilesInDirectory("/backup").ToList();
		Assert.IsNotNull(files);
		// check that at least one file exists in backup folder
		Assert.IsNotEmpty(files);
		var backupPath = files[0];
		Assert.IsTrue(storage.ExistFile(backupPath));
		var info = storage.Info(backupPath);
		Assert.AreEqual(sourceBytes.Length, info.Size);
	}

	[TestMethod]
	public async Task CopyStreamFromHostToBackup_ReturnsFalse_WhenWriteFails()
	{
		var sourceBytes = "\t\t\t"u8.ToArray();
		var storageForSelector = new FakeIStorage(["/"], ["/test.jpg"],
			new List<byte[]> { sourceBytes }, null,
			null, new AggregateException(new IOException("Simulated write failure")));
		var selector = new FakeSelectorStorage(storageForSelector);
		var logger = new FakeIWebLogger();
		var sut = new ImportBackup(selector, logger);

		var importIndexItem = new ImportIndexItem
		{
			SourceFullFilePath = "/test.jpg",
			FileIndexItem = new FileIndexItem
			{
				FileName = "test.jpg",
				DateTime = DateTime.UtcNow,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
			}
		};

		var importBackup = new AppSettingsImportBackupModel
		{
			Enabled = true, StorageFolder = "/backup"
		};

		var result = await sut.CopyStreamFromHostToBackup(importIndexItem, importBackup);
		Assert.IsFalse(result);
		// error has been logged
		Assert.IsNotEmpty(logger.TrackedExceptions);
	}
}
