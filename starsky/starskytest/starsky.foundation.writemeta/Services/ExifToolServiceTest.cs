using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Services;

[TestClass]
public class ExifToolServiceTest
{
	private static readonly string ExifToolPath =
		Path.Join(new CreateAnImage().BasePath, "exiftool-service-test-tmp");
	private static readonly string RetryExifToolPath =
		Path.Join(new CreateAnImage().BasePath, "exiftool-service-retry-test-tmp");

	public ExifToolServiceTest()
	{
		if ( new AppSettings().IsWindows )
		{
			return;
		}

		CreateFile();
	}

	private static void CreateFile(string? path = null)
	{
		path ??= ExifToolPath;
		var stream = StringToStreamHelper.StringToStream("#!/bin/bash\necho Fake ExifTool");
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStream(stream, path);

		var result = Command.Run("chmod", "+x",
			path).Task.Result;
		if ( !result.Success )
		{
			throw new FileNotFoundException(result.StandardError);
		}
	}

	private static void CreatePassingFile(string? path = null)
	{
		path ??= ExifToolPath;
		var stream = StringToStreamHelper.StringToStream("#!/bin/bash\ncat");
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStream(stream, path);

		var result = Command.Run("chmod", "+x",
			path).Task.Result;
		if ( !result.Success )
		{
			throw new FileNotFoundException(result.StandardError);
		}
	}

	[ClassCleanup]
	public static void CleanExifToolServiceTest()
	{
		if ( File.Exists(ExifToolPath) )
		{
			File.Delete(ExifToolPath);
		}

		if ( File.Exists(RetryExifToolPath) )
		{
			File.Delete(RetryExifToolPath);
		}
	}

	private static async Task WriteTagsAndRenameThumbnailAsyncUnixPrivateTest()
	{
		var storage = new FakeIStorage(["/"],
			["/image.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		CreateFile();

		var service = new ExifToolService(new FakeSelectorStorage(storage),
			new AppSettings { ExifToolPath = ExifToolPath }, new FakeIWebLogger(),
			new FakeExifToolDownload());
		var result = await service.WriteTagsAndRenameThumbnailAsync(
			"/image.jpg",
			null, "");
		Assert.IsFalse(result.IsSuccess);

		CleanExifToolServiceTest();
	}

	[TestMethod]
	public async Task WriteTagsAndRenameThumbnailAsync__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		try
		{
			await WriteTagsAndRenameThumbnailAsyncUnixPrivateTest();
		}
		catch ( ObjectDisposedException )
		{
			Console.WriteLine("Retry due ObjectDisposedException");
			await WriteTagsAndRenameThumbnailAsyncUnixPrivateTest();
		}
	}

	[TestMethod]
	public async Task WriteTagsAndRenameThumbnailAsync_TaskCanceledException__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is for Unix Only");
			return;
		}

		// Arrange
		var storage = new FakeIStorage(
			["/"],
			["/image.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);

		var service = new ExifToolService(
			new FakeSelectorStorage(storage),
			new AppSettings { ExifToolPath = ExifToolPath },
			new FakeIWebLogger(),
			new FakeExifToolDownload()
		);

		using var cancelSource = new CancellationTokenSource();
		var token = cancelSource.Token;
		await cancelSource.CancelAsync(); // Trigger cancellation

		// Act & Assert
		await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () =>
		{
			await service.WriteTagsAndRenameThumbnailAsync("/image.jpg", null, "", token);
		});
	}

	[TestMethod]
	public async Task WriteTagsAndRenameThumbnailAsync_RetriesAfterArgumentException__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is for Unix Only");
			return;
		}

		var storage = new FakeIStorage(["/"],
			["/image.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var download = new CreateExifToolOnDownload(RetryExifToolPath);
		var service = new ExifToolService(new FakeSelectorStorage(storage),
			new AppSettings { ExifToolPath = RetryExifToolPath }, new FakeIWebLogger(),
			download);

		var result = await service.WriteTagsAndRenameThumbnailAsync("/image.jpg",
			null, "");

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(1, download.Called);
	}

	private sealed class CreateExifToolOnDownload : IExifToolDownload
	{
		private readonly string _exifToolPath;

		public CreateExifToolOnDownload(string exifToolPath)
		{
			_exifToolPath = exifToolPath;
		}

		public int Called { get; private set; }

		public Task<List<bool>> DownloadExifTool(List<string> architectures)
		{
			throw new NotImplementedException();
		}

		public async Task<bool> DownloadExifTool(bool isWindows, int minimumSize = 30)
		{
			Called++;
			CreatePassingFile(_exifToolPath);
			return await Task.FromResult(true);
		}
	}
}
