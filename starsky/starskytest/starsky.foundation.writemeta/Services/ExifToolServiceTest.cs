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
	private readonly string _exifToolPath = string.Empty;

	public ExifToolServiceTest()
	{
		if ( new AppSettings().IsWindows )
		{
			return;
		}
		
		var stream = StringToStreamHelper.StringToStream("#!/bin/bash\necho Fake ExifTool");
		_exifToolPath = Path.Join(new CreateAnImage().BasePath, "exiftool-tmp");
		new StorageHostFullPathFilesystem().WriteStream(stream,
			_exifToolPath);

		var result = Command.Run("chmod", "+x",
			_exifToolPath).Task.Result;
		if ( !result.Success )
		{
			throw new FileNotFoundException(result.StandardError);
		}
	}

	private async Task WriteTagsAndRenameThumbnailAsyncUnixPrivateTest()
	{
		var storage = new FakeIStorage(new List<string>{"/"}, 
			new List<string>{"/image.jpg"}, new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray()
			});
		
		var service = new ExifToolService(new FakeSelectorStorage(storage), new AppSettings
		{
			ExifToolPath = _exifToolPath
		}, new FakeIWebLogger());
		var result = await service.WriteTagsAndRenameThumbnailAsync("/image.jpg", 
			null, "");
		
		Assert.AreEqual(false,result.Key);
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
	[ExpectedException(typeof(TaskCanceledException))]
	public async Task WriteTagsAndRenameThumbnailAsync_TaskCanceledException__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}
		
		var storage = new FakeIStorage(new List<string>{"/"}, 
			new List<string>{"/image.jpg"}, new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray()
			});
		
		var service = new ExifToolService(new FakeSelectorStorage(storage), new AppSettings
		{
			ExifToolPath = _exifToolPath
		}, new FakeIWebLogger());

		using var cancelSource = new CancellationTokenSource();
		var token = cancelSource.Token;
		await cancelSource.CancelAsync();
			
		await service.WriteTagsAndRenameThumbnailAsync("/image.jpg", 
			null, "", token);
		// Cancel token
	}
}
