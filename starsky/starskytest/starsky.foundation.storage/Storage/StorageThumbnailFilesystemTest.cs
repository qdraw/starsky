using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Storage;

[TestClass]
public sealed class StorageThumbnailFilesystemTest
{
	private readonly string _fileName;
	private readonly StorageThumbnailFilesystem _thumbnailStorage;

	public StorageThumbnailFilesystemTest()
	{
		var createNewImage = new CreateAnImage();
		var appSettings = new AppSettings { ThumbnailTempFolder = createNewImage.BasePath };
		_thumbnailStorage = new StorageThumbnailFilesystem(appSettings, new FakeIWebLogger());
		_fileName = createNewImage.FileName;
	}

	[TestMethod]
	public void Thumbnail_CombinePathShouldEndWithTestJpg()
	{
		var result = _thumbnailStorage.CombinePath("test.jpg");
		Assert.EndsWith("test.jpg", result);
	}

	[TestMethod]
	public void Thumbnail_FileMove_Test()
	{
		var createNewImage = new CreateAnImage();

		// first copy for parallel test
		RetryHelper.Do(CreateStartMoveFileAsync, TimeSpan.FromSeconds(1));

		_thumbnailStorage.FileMove("start_move_file",
			"StorageThumbnailFilesystemTest_FileMove.jpg");

		var path = Path.Combine(createNewImage.BasePath, "start_move_file" + ".jpg");
		Assert.IsFalse(File.Exists(path));
		var path2 = Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileMove.jpg");
		Assert.IsTrue(File.Exists(path2));

		File.Delete(Path.Combine(createNewImage.BasePath, "start_move_file.jpg"));
		File.Delete(Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileMove.jpg"));

		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);
		return;

		bool CreateStartMoveFileAsync()
		{
			// first copy for parallel test
			createNewImage = new CreateAnImage();
			_thumbnailStorage.FileCopy(_fileName, "start_move_file");
			return true;
		}
	}

	[TestMethod]
	public void Thumbnail_FileMove_NotFound()
	{
		Assert.IsFalse(_thumbnailStorage.FileMove("not-found",
			"StorageThumbnailFilesystemTest_FileMove.jpg"));
	}

	[TestMethod]
	public async Task Thumbnail_FileMove_SkipIfAlreadyExists()
	{
		var createAnImage = new CreateAnImage();

		// first copy for parallel test
		const string alreadyExistsFileName = "already_exists_file_thumbnail.jpg";
		const string beforeTestFileName = "before_test_thumbnail.jpg";
		
		_thumbnailStorage.FileCopy(_fileName, alreadyExistsFileName);
		await _thumbnailStorage.WriteStreamAsync(StringToStreamHelper.StringToStream("1"),
			beforeTestFileName);

		_thumbnailStorage.FileMove(beforeTestFileName, alreadyExistsFileName);

		Assert.AreEqual(CreateAnImage.Size, _thumbnailStorage.Info(alreadyExistsFileName).Size);

		File.Delete(Path.Combine(createAnImage.BasePath,
			alreadyExistsFileName));
		_thumbnailStorage.FileDelete(beforeTestFileName);
	}

	[TestMethod]
	public void Thumbnail_FileCopy_success()
	{
		var createNewImage = new CreateAnImage();

		_thumbnailStorage.FileCopy(_fileName,
			"StorageThumbnailFilesystemTest_FileCopy.jpg");

		var path = Path.Combine(createNewImage.BasePath, _fileName);
		Assert.IsTrue(File.Exists(path));
		var path2 = Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileCopy.jpg");
		Assert.IsTrue(File.Exists(path2));

		File.Delete(_fileName);
		File.Delete(Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileCopy.jpg"));

		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);
	}

	[TestMethod]
	public void Thumbnail_FileCopy_source_notFound()
	{
		var createNewImage = new CreateAnImage();

		_thumbnailStorage.FileCopy("not_found", "StorageThumbnailFilesystemTest_FileCopy2.jpg");

		var path2 = Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileCopy2.jpg");
		Assert.IsFalse(File.Exists(path2));
	}

	[TestMethod]
	public void Thumbnail_FileDelete_NotExist()
	{
		Assert.IsFalse(_thumbnailStorage.FileDelete("NotFound"));
	}

	[TestMethod]
	public void Thumbnail_ReadStream()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _thumbnailStorage.ReadStream(_fileName);
		Assert.AreEqual(CreateAnImage.Bytes.Length, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void Thumbnail_ReadStream_MaxLength()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _thumbnailStorage.ReadStream(_fileName, 100);
		Assert.AreEqual(100, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void Thumbnail_WriteStream()
	{
		const string filename = "StorageThumbnailFilesystemTest_WriteStream.jpg";
		var createNewImage = new CreateAnImage();

		_thumbnailStorage.WriteStream(new MemoryStream([.. CreateAnImage.Bytes]),
			filename);

		var readStream =
			_thumbnailStorage.ReadStream(filename);
		Assert.AreEqual(CreateAnImage.Bytes.Length, readStream.Length);
		readStream.Dispose();

		File.Delete(Path.Combine(createNewImage.BasePath, filename));
	}

	[TestMethod]
	public async Task Thumbnail_WriteStreamAsync()
	{
		var createNewImage = new CreateAnImage();
		const string filename = "StorageThumbnailFilesystemTest_WriteStreamAsync.jpg";

		await _thumbnailStorage.WriteStreamAsync(
			new MemoryStream([.. CreateAnImage.Bytes]),
			filename);

		var readStream = _thumbnailStorage.ReadStream(filename);
		Assert.AreEqual(CreateAnImage.Bytes.Length, readStream.Length);
		await readStream.DisposeAsync();

		File.Delete(Path.Combine(createNewImage.BasePath, filename));
	}

	[TestMethod]
	public async Task Thumbnail_IsFileReady_thumbnailStorage()
	{
		var createNewImage = new CreateAnImage();

		const string thumbnailId = "IsFileReady_thumbnailStorage";
		// first copy for parallel test
		_thumbnailStorage.FileCopy(_fileName, thumbnailId);

		var stream = _thumbnailStorage.ReadStream(thumbnailId);

		var result = _thumbnailStorage.IsFileReady(thumbnailId);
		Assert.IsFalse(result);

		// is disposed too late (as designed)
		await stream.DisposeAsync();

		if ( new AppSettings().IsWindows )
		{
			await Task.Delay(500, TestContext.CancellationTokenSource.Token);
		}

		var result2 = _thumbnailStorage.IsFileReady(thumbnailId);
		Assert.IsTrue(result2);

		File.Delete(Path.Combine(createNewImage.BasePath,
			$"{thumbnailId}"));

		Assert.IsFalse(_thumbnailStorage.ExistFile(thumbnailId));
	}

	[TestMethod]
	public void Thumbnail_IsFolderOrFile()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.IsFolderOrFile("not-found"));
	}

	[TestMethod]
	public void Thumbnail_FolderMove()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.FolderMove("not-found", "2"));
	}

	[TestMethod]
	public void Thumbnail_CreateDirectory()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.CreateDirectory("not-found"));
	}

	[TestMethod]
	public void Thumbnail_FolderDelete()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.FolderDelete("not-found"));
	}

	[TestMethod]
	public void Thumbnail_GetAllFilesInDirectoryRecursive()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.GetAllFilesInDirectoryRecursive("not-found"));
	}

	[TestMethod]
	public void Thumbnail_GetDirectories()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.GetDirectories("not-found"));
	}

	[TestMethod]
	public void Thumbnail_GetDirectoryRecursive()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.GetDirectoryRecursive("not-found"));
	}

	[TestMethod]
	public void Thumbnail_ReadStream_NotFound()
	{
		Assert.ThrowsExactly<FileNotFoundException>(() =>
			_thumbnailStorage.ReadStream("not-found"));
	}

	[TestMethod]
	public void Thumbnail_WriteStreamOpenOrCreate()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.WriteStreamOpenOrCreate(Stream.Null, "not-found"));
	}

	[TestMethod]
	public void Thumbnail_Info()
	{
		Assert.AreEqual(CreateAnImage.Size, _thumbnailStorage.Info(_fileName).Size);
	}

	public TestContext TestContext { get; set; }
}
