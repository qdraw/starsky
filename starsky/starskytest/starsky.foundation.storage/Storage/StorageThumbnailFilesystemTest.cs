using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
	public void CombinePathShouldEndWithTestJpg()
	{
		var result = _thumbnailStorage.CombinePath("test.jpg");
		Assert.IsTrue(result.EndsWith("test.jpg"));
	}

	[TestMethod]
	public void FileMove_Test()
	{
		var createNewImage = new CreateAnImage();

		// first copy for parallel test
		_thumbnailStorage.FileCopy(_fileName, "start_move_file");

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
	}

	[TestMethod]
	public void FileMove_NotFound()
	{
		Assert.IsFalse(_thumbnailStorage.FileMove("not-found",
			"StorageThumbnailFilesystemTest_FileMove.jpg"));
	}

	[TestMethod]
	public async Task FileMove_SkipIfAlreadyExists()
	{
		var createAnImage = new CreateAnImage();

		// first copy for parallel test
		const string alreadyExistsFileName = "already_exists_file.jpg";
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
	public void FileCopy_success()
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
	public void FileCopy_source_notFound()
	{
		var createNewImage = new CreateAnImage();

		_thumbnailStorage.FileCopy("not_found", "StorageThumbnailFilesystemTest_FileCopy2.jpg");

		var path2 = Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileCopy2.jpg");
		Assert.IsFalse(File.Exists(path2));
	}

	[TestMethod]
	public void FileDelete_NotExist()
	{
		Assert.IsFalse(_thumbnailStorage.FileDelete("NotFound"));
	}

	[TestMethod]
	public void ReadStream()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _thumbnailStorage.ReadStream(_fileName);
		Assert.AreEqual(CreateAnImage.Bytes.Length, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void ReadStream_MaxLength()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _thumbnailStorage.ReadStream(_fileName, 100);
		Assert.AreEqual(100, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void WriteStream()
	{
		var createNewImage = new CreateAnImage();

		_thumbnailStorage.WriteStream(new MemoryStream([.. CreateAnImage.Bytes]),
			"StorageThumbnailFilesystemTest_WriteStream.jpg");

		var readStream =
			_thumbnailStorage.ReadStream("StorageThumbnailFilesystemTest_WriteStream.jpg");
		Assert.AreEqual(CreateAnImage.Bytes.Length, readStream.Length);
		readStream.Dispose();

		File.Delete(Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_WriteStream.jpg"));
	}

	[TestMethod]
	public async Task WriteStreamAsync()
	{
		var createNewImage = new CreateAnImage();

		await _thumbnailStorage.WriteStreamAsync(
			new MemoryStream([.. CreateAnImage.Bytes]),
			"StorageThumbnailFilesystemTest_WriteStreamAsync.jpg");

		var readStream =
			_thumbnailStorage.ReadStream("StorageThumbnailFilesystemTest_WriteStreamAsync.jpg");
		Assert.AreEqual(CreateAnImage.Bytes.Length, readStream.Length);
		await readStream.DisposeAsync();

		File.Delete(Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_WriteStreamAsync.jpg"));
	}

	[TestMethod]
	public void IsFileReady_thumbnailStorage()
	{
		var createNewImage = new CreateAnImage();

		const string thumbnailId = "IsFileReady_thumbnailStorage";
		// first copy for parallel test
		_thumbnailStorage.FileCopy(_fileName, thumbnailId);

		var stream = _thumbnailStorage.ReadStream(thumbnailId);

		var result = _thumbnailStorage.IsFileReady(thumbnailId);
		Assert.IsFalse(result);

		// is disposed to late (as designed)
		stream.Dispose();

		var result2 = _thumbnailStorage.IsFileReady(thumbnailId);
		Assert.IsTrue(result2);

		File.Delete(Path.Combine(createNewImage.BasePath,
			$"{thumbnailId}"));

		Assert.IsFalse(_thumbnailStorage.ExistFile(thumbnailId));
	}

	[TestMethod]
	public void IsFolderOrFile()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.IsFolderOrFile("not-found"));
	}

	[TestMethod]
	public void FolderMove()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.FolderMove("not-found", "2"));
	}

	[TestMethod]
	public void CreateDirectory()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.CreateDirectory("not-found"));
	}

	[TestMethod]
	public void FolderDelete()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.FolderDelete("not-found"));
	}

	[TestMethod]
	public void GetAllFilesInDirectoryRecursive()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.GetAllFilesInDirectoryRecursive("not-found"));
	}

	[TestMethod]
	public void GetDirectories()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.GetDirectories("not-found"));
	}

	[TestMethod]
	public void GetDirectoryRecursive()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.GetDirectoryRecursive("not-found"));
	}

	[TestMethod]
	public void ReadStream_NotFound()
	{
		Assert.ThrowsExactly<FileNotFoundException>(() =>
			_thumbnailStorage.ReadStream("not-found"));
	}

	[TestMethod]
	public void WriteStreamOpenOrCreate()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_thumbnailStorage.WriteStreamOpenOrCreate(Stream.Null, "not-found"));
	}

	[TestMethod]
	public void Info()
	{
		Assert.AreEqual(CreateAnImage.Size, _thumbnailStorage.Info(_fileName).Size);
	}
}
