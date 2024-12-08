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
using starsky.foundation.writemeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Services;

[TestClass]
public class ExifToolServiceTest
{
	private static readonly string ExifToolPath =
		Path.Join(new CreateAnImage().BasePath, "exiftool-service-test-tmp");

	public ExifToolServiceTest()
	{
		if ( new AppSettings().IsWindows )
		{
			return;
		}

		CreateFile();
	}

	private static void CreateFile()
	{
		var stream = StringToStreamHelper.StringToStream("#!/bin/bash\necho Fake ExifTool");
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStream(stream,
			ExifToolPath);

		var result = Command.Run("chmod", "+x",
			ExifToolPath).Task.Result;
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
	}

	private async Task WriteTagsAndRenameThumbnailAsyncUnixPrivateTest()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/image.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		CreateFile();

		var service = new ExifToolService(new FakeSelectorStorage(storage),
			new AppSettings { ExifToolPath = ExifToolPath }, new FakeIWebLogger());
		var result = await service.WriteTagsAndRenameThumbnailAsync(
			"/image.jpg",
			null, "");
		Assert.IsFalse(result.Key);

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
			new List<string> { "/" },
			new List<string> { "/image.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }
		);

		var service = new ExifToolService(
			new FakeSelectorStorage(storage),
			new AppSettings { ExifToolPath = ExifToolPath },
			new FakeIWebLogger()
		);

		using var cancelSource = new CancellationTokenSource();
		var token = cancelSource.Token;
		await cancelSource.CancelAsync(); // Trigger cancellation

		// Act & Assert
		await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
		{
			await service.WriteTagsAndRenameThumbnailAsync("/image.jpg", null, "", token);
		});
	}
}
