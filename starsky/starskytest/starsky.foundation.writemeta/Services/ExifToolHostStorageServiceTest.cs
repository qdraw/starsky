using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Services;

[TestClass]
public sealed class ExifToolHostStorageServiceTest
{
	private readonly CreateAnImage _createAnImage;

	public ExifToolHostStorageServiceTest()
	{
		_createAnImage = new CreateAnImage();
	}

	[TestMethod]
	public async Task ExifToolHostStorageService_NotFound_Exception()
	{
		// Arrange
		var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist" };
		var fakeStorage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);

		var service = new ExifToolHostStorageService(
			new FakeSelectorStorage(fakeStorage),
			appSettings,
			new FakeIWebLogger()
		);

		// Act & Assert
		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
		{
			await service.WriteTagsAsync("/test.jpg", "-Software=\"Qdraw 2.0\"");
		});
	}

	/// <summary>
	///     WriteTagsAndRenameThumbnailAsyncTest
	/// </summary>
	[TestMethod]
	public async Task WriteTagsAndRenameThumbnailAsync_NotFound_Exception()
	{
		// Arrange
		var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist" };
		var fakeStorage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);

		var service = new ExifToolHostStorageService(
			new FakeSelectorStorage(fakeStorage),
			appSettings,
			new FakeIWebLogger()
		);

		// Act & Assert
		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
		{
			await service.WriteTagsAndRenameThumbnailAsync("/test.jpg", null,
				"-Software=\"Qdraw 2.0\"");
		});
	}

	[TestMethod]
	public async Task WriteTagsAndRenameThumbnailAsync_FakeExifToolBashTest_UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		var hostFileSystemStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		var memoryStream = new MemoryStream(CreateAnExifToolTarGz.Bytes.ToArray());
		var outputPath =
			Path.Combine(_createAnImage.BasePath, "tmp-3426782387");
		if ( hostFileSystemStorage.ExistFolder(outputPath) )
		{
			hostFileSystemStorage.FolderDelete(outputPath);
		}

		await new TarBal(hostFileSystemStorage, new FakeIWebLogger()).ExtractTarGz(memoryStream,
			outputPath,
			CancellationToken.None);
		var imageExifToolVersionFolder = hostFileSystemStorage.GetDirectories(outputPath)
			.FirstOrDefault(p => p.StartsWith(Path.Combine(outputPath, "Image-ExifTool-")))?
			.Replace("./", string.Empty);

		if ( imageExifToolVersionFolder == null )
		{
			throw new FileNotFoundException("imageExifToolVersionFolder: " + outputPath);
		}

		await Command.Run("chmod", "+x",
			Path.Combine(imageExifToolVersionFolder, "exiftool")).Task;

		var appSettings = new AppSettings
		{
			ExifToolPath = Path.Combine(imageExifToolVersionFolder, "exiftool")
		};

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var fakeLogger = new FakeIWebLogger();
		var service = new ExifToolHostStorageService(
			new FakeSelectorStorage(fakeStorage), appSettings, fakeLogger);

		var renameThumbnailAsync = await service
			.WriteTagsAndRenameThumbnailAsync("/test.jpg",
				null, "-Software=\"Qdraw 2.0\"");

		if ( hostFileSystemStorage.ExistFolder(outputPath) )
		{
			hostFileSystemStorage.FolderDelete(outputPath);
		}

		Assert.IsFalse(renameThumbnailAsync.IsSuccess);
		Assert.IsTrue(fakeLogger.TrackedExceptions.Exists(p =>
			p.Item2?.Contains("Fake Exiftool detected") == true));
	}

	[TestMethod]
	public async Task WriteTagsAndRenameThumbnailAsync_FakeExifToolTest_WindowsOnly()
	{
		if ( !new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Windows Only");
			return;
		}

		var hostFileSystemStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		var outputPath =
			Path.Combine(_createAnImage.BasePath, "tmp-979056548");

		Console.WriteLine(outputPath);
		if ( hostFileSystemStorage.ExistFolder(outputPath) )
		{
			hostFileSystemStorage.FolderDelete(outputPath);
		}

		hostFileSystemStorage.CreateDirectory(outputPath);

		var result = Zipper.ExtractZip(CreateAnExifToolWindows.Bytes.ToArray());
		var (_, item) = result.FirstOrDefault(p => p.Key.Contains("exiftool"));

		await hostFileSystemStorage.WriteStreamAsync(new MemoryStream(item),
			Path.Combine(outputPath, "exiftool.exe"));

		var appSettings = new AppSettings
		{
			ExifToolPath = Path.Combine(outputPath, "exiftool.exe"), Verbose = true
		};

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var fakeLogger = new FakeIWebLogger();
		var renameThumbnailAsync = await new ExifToolHostStorageService(
				new FakeSelectorStorage(fakeStorage),
				appSettings, fakeLogger)
			.WriteTagsAndRenameThumbnailAsync("/test.jpg", null,
				"-Software=\"Qdraw 2.0\"");

		if ( hostFileSystemStorage.ExistFolder(outputPath) )
		{
			try
			{
				hostFileSystemStorage.FileDelete(appSettings.ExifToolPath);
				hostFileSystemStorage.FolderDelete(outputPath);
			}
			catch ( UnauthorizedAccessException e )
			{
				Console.WriteLine("UnauthorizedAccessException");
				Console.WriteLine(outputPath);
				Console.WriteLine(e);
			}
		}

		Assert.IsFalse(renameThumbnailAsync.IsSuccess);
		Assert.IsTrue(fakeLogger.TrackedExceptions.Exists(p =>
			p.Item2?.Contains("Fake Exiftool detected") == true));
	}

	[TestMethod]
	public async Task ExifToolHostStorageService_WriteTagsThumbnailAsync_NotFound_Exception()
	{
		// Arrange
		var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist" };
		var fakeStorage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);

		var service = new ExifToolHostStorageService(
			new FakeSelectorStorage(fakeStorage),
			appSettings,
			new FakeIWebLogger()
		);

		// Act & Assert
		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
		{
			await service.WriteTagsThumbnailAsync("/test.jpg", "-Software=\"Qdraw 2.0\"");
		});
	}
}
