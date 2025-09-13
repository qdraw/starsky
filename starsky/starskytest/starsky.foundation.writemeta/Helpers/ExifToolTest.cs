using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateFakeExifToolWindows;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Helpers;

[TestClass]
public sealed class ExifToolTest
{
	private readonly AppSettings _appSettingsWithExifTool;
	private readonly StorageHostFullPathFilesystem _hostFullPathFilesystem;

	public ExifToolTest()
	{
		_hostFullPathFilesystem = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		var exifToolExeWindows = new CreateFakeExifToolWindows().ExifToolPath;
		_hostFullPathFilesystem.CreateDirectory(Path.Combine(new CreateAnImage().BasePath,
			"ExifToolTest"));
		var exifToolExePosix = Path.Combine(new CreateAnImage().BasePath,
			"ExifToolTest", "exiftool");

		CreateStubFile(exifToolExePosix,
			"#!/bin/bash\necho Fake Executable");

		new FfMpegChmod(new FakeSelectorStorage(_hostFullPathFilesystem),
				new FakeIWebLogger())
			.Chmod(exifToolExePosix).ConfigureAwait(false);

		var exifToolExe = new AppSettings().IsWindows ? exifToolExeWindows : exifToolExePosix;
		_appSettingsWithExifTool = new AppSettings { ExifToolPath = exifToolExe };
	}

	[ClassCleanup(ClassCleanupBehavior.EndOfClass)]
	public static void CleanUp()
	{
		var folder = Path.Combine(new CreateAnImage().BasePath,
			"ExifToolTest");
		if ( Directory.Exists(folder) )
		{
			Directory.Delete(folder, true);
		}
	}

	private void CreateStubFile(string path, string content)
	{
		var stream = StringToStreamHelper.StringToStream(content);
		_hostFullPathFilesystem.WriteStream(stream, path);
	}

	[TestMethod]
	public async Task ExifTool_ArgumentException()
	{
		var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist" };

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var sut = new ExifToolService(new FakeSelectorStorage(fakeStorage), appSettings,
			new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await sut.WriteTagsAsync("/test.jpg", "-Software=\"Qdraw 2.0\""));
	}

	[TestMethod]
	public async Task ExifTool_WriteTagsThumbnailAsync_NotFound_Exception()
	{
		var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist" };

		var fakeStorage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var sut = new ExifToolService(new FakeSelectorStorage(fakeStorage), appSettings,
			new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await sut.WriteTagsAsync("/test.jpg", "-Software=\"Qdraw 2.0\""));
	}

	[TestMethod]
	public async Task ExifTool_RenameThumbnailByStream_Length26()
	{
		var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist" };

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var result =
			await new ExifTool(fakeStorage, fakeStorage, appSettings, new FakeIWebLogger())
				.RenameThumbnailByStream("OLDHASH", new MemoryStream(), true, "test");

		Assert.AreEqual(26, result.newHashCode.Length);
	}

	[TestMethod]
	public async Task ExifTool_RenameThumbnailByStream_Fail()
	{
		var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist" };

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var result =
			await new ExifTool(fakeStorage, fakeStorage, appSettings, new FakeIWebLogger())
				.RenameThumbnailByStream("OLDHASH", new MemoryStream(), false, "test");

		Assert.AreEqual(0, result.newHashCode.Length);
	}

	[TestMethod]
	public async Task ExifTool_RenameThumbnailByStream_NotDisposed_CanWrite()
	{
		var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist" };

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var stream = new MemoryStream();
		await new ExifTool(fakeStorage, fakeStorage, appSettings, new FakeIWebLogger())
			.RenameThumbnailByStream("OLDHASH", stream, true, "test");

		Assert.IsTrue(stream.CanWrite);
	}

	[TestMethod]
	public async Task WriteTagsAndRenameThumbnailAsync_Disposed()
	{
		var storage =
			new FakeIStorage(new ObjectDisposedException("disposed"));

		var exifTool = new ExifTool(storage,
			storage,
			new AppSettings(), new FakeIWebLogger());

		var exceptionMessage = string.Empty;
		try
		{
			await exifTool.WriteTagsAndRenameThumbnailAsync("test.jpg", null, "");
		}
		catch ( ObjectDisposedException e )
		{
			// Expected
			exceptionMessage = e.Message;
		}

		Assert.StartsWith("Cannot access a disposed object.", exceptionMessage);
		Assert.EndsWith("Object name: 'disposed'.", exceptionMessage);

		Assert.AreEqual(1, storage.ExceptionCount);
	}

	[TestMethod]
	public async Task ExifTool_WriteTagsAsync_HappyFlow__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test does not work under windows");
		}

		// unfortunately, this test does not work under windows
		// if you are reading this and are interested in fixing this test please do

		var storage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var (beforeHash, _) =
			await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync("/test.jpg");

		var sut = new ExifTool(storage, new FakeIStorage(),
			_appSettingsWithExifTool, new FakeIWebLogger());

		// Act
		var result = await sut.WriteTagsAsync("/test.jpg", "-Software=\"Qdraw 2.0\"");

		// Assert
		var (afterHash, _) =
			await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync("/test.jpg");

		Assert.IsTrue(result);
		// Does change after update
		Assert.AreNotEqual(beforeHash, afterHash);
	}

	[TestMethod]
	public async Task ExifTool_WriteTagsThumbnailAsync_HappyFlow__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test does not work under windows");
		}

		// unfortunately, this test does not work under windows
		// if you are reading this and are interested in fixing this test please do

		var storage = new FakeIStorage(["/"],
			["/hash.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var (beforeHash, _) =
			await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync("/hash.jpg");

		var sut = new ExifTool(new FakeIStorage(), storage,
			_appSettingsWithExifTool, new FakeIWebLogger());

		// Act
		var result = await sut.WriteTagsThumbnailAsync("/hash.jpg", "-Software=\"Qdraw 2.0\"");

		// Assert
		var (afterHash, _) =
			await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync("/hash.jpg");

		Assert.IsTrue(result);
		// Does change after update
		Assert.AreNotEqual(beforeHash, afterHash);
	}
}
