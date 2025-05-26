using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Storage;

[TestClass]
public sealed class StorageTemporaryFilesystemTests
{
	private readonly StorageTemporaryFilesystem _tempStorage;
	private string _fileName;

	public StorageTemporaryFilesystemTests()
	{
		var createNewImage = new CreateAnImage();
		var appSettings = new AppSettings { TempFolder = createNewImage.BasePath };
		_tempStorage = new StorageTemporaryFilesystem(appSettings, new FakeIWebLogger());
		_fileName = createNewImage.FileName;
	}

	[TestMethod]
	public void FileMove_Test()
	{
		var createNewImage = new CreateAnImage();

		// first copy for parallel test
		_tempStorage.FileCopy(_fileName, "start_move_file");

		_tempStorage.FileMove("start_move_file",
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
		Assert.IsFalse(_tempStorage.FileMove("not-found",
			"StorageThumbnailFilesystemTest_FileMove.jpg"));
	}

	[TestMethod]
	public async Task FileMove_SkipIfAlreadyExists()
	{
		var createAnImage = new CreateAnImage();

		// first copy for parallel test
		const string alreadyExistsFileName = "already_exists_file_tmpfs.jpg";
		const string beforeTestFileName = "before_test_tmpfs.jpg";
		File.Delete(Path.Combine(createAnImage.BasePath,
			alreadyExistsFileName));

		_tempStorage.FileCopy(_fileName, alreadyExistsFileName);
		await _tempStorage.WriteStreamAsync(StringToStreamHelper.StringToStream("1"),
			beforeTestFileName);

		_tempStorage.FileMove(beforeTestFileName, alreadyExistsFileName);

		Assert.AreEqual(CreateAnImage.Size, _tempStorage.Info(alreadyExistsFileName).Size);

		File.Delete(Path.Combine(createAnImage.BasePath,
			alreadyExistsFileName));
		_tempStorage.FileDelete(beforeTestFileName);
	}

	[TestMethod]
	public void FileCopy_success()
	{
		var createNewImage = new CreateAnImage();


		_tempStorage.FileCopy(_fileName,
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

		_tempStorage.FileCopy("not_found", "StorageThumbnailFilesystemTest_FileCopy2.jpg");

		var path2 = Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileCopy2.jpg");
		Assert.IsFalse(File.Exists(path2));
	}

	[TestMethod]
	public void FileDelete_NotExist()
	{
		Assert.IsFalse(_tempStorage.FileDelete("NotFound"));
	}

	[TestMethod]
	public void ReadStream()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _tempStorage.ReadStream(_fileName);
		Assert.AreEqual(CreateAnImage.Bytes.Length, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void ReadStream_MaxLength()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _tempStorage.ReadStream(_fileName, 100);
		Assert.AreEqual(100, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void WriteStream()
	{
		var createNewImage = new CreateAnImage();

		_tempStorage.WriteStream(new MemoryStream([.. CreateAnImage.Bytes]),
			"StorageThumbnailFilesystemTest_WriteStream.jpg");

		var readStream =
			_tempStorage.ReadStream("StorageThumbnailFilesystemTest_WriteStream.jpg");
		Assert.AreEqual(CreateAnImage.Bytes.Length, readStream.Length);
		readStream.Dispose();

		File.Delete(Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_WriteStream.jpg"));
	}

	[TestMethod]
	public async Task WriteStreamAsync()
	{
		var createNewImage = new CreateAnImage();

		await _tempStorage.WriteStreamAsync(
			new MemoryStream([.. CreateAnImage.Bytes]),
			"StorageThumbnailFilesystemTest_WriteStreamAsync.jpg");

		var readStream =
			_tempStorage.ReadStream("StorageThumbnailFilesystemTest_WriteStreamAsync.jpg");
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
		_tempStorage.FileCopy(_fileName, thumbnailId);

		var stream = _tempStorage.ReadStream(thumbnailId);

		var result = _tempStorage.IsFileReady(thumbnailId);
		Assert.IsFalse(result);

		// is disposed to late (as designed)
		stream.Dispose();

		var result2 = _tempStorage.IsFileReady(thumbnailId);
		Assert.IsTrue(result2);

		File.Delete(Path.Combine(createNewImage.BasePath,
			$"{thumbnailId}"));

		Assert.IsFalse(_tempStorage.ExistFile(thumbnailId));
	}

	[TestMethod]
	public void IsFolderOrFile_Exists_TempStorage()
	{
		// sometimes this test is flaky, it should have the file there
		if ( !File.Exists(new CreateAnImage().FullFilePath) )
		{
			_fileName = new CreateAnImage().FileName;
		}

		var result = _tempStorage.IsFolderOrFile(_fileName);
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File, result);
	}

	[TestMethod]
	public void IsFolderOrFile_NotFound_TempStorage()
	{
		var result = _tempStorage.IsFolderOrFile("not-found");
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Deleted, result);
	}

	[TestMethod]
	public void FolderMove_TempStorage()
	{
		const string from = "/test_folder_move_from";
		const string to = "/test_folder_move_to";

		_tempStorage.CreateDirectory(from);
		if ( _tempStorage.ExistFolder(to) )
		{
			_tempStorage.FolderDelete(to);
		}

		_tempStorage.FolderMove(from, to);

		if ( _tempStorage.ExistFolder(from) )
		{
			_tempStorage.FolderDelete(from);
		}

		Assert.IsFalse(_tempStorage.ExistFolder(from));
		Assert.IsTrue(_tempStorage.ExistFolder(to));

		_tempStorage.FolderDelete(to);
	}

	[TestMethod]
	public void CreateDirectory_TempStorage()
	{
		_tempStorage.CreateDirectory("/test");
		Assert.IsTrue(_tempStorage.ExistFolder("/test"));
		_tempStorage.FolderDelete("/test");
	}

	[TestMethod]
	public void FolderDelete_TempStorage()
	{
		_tempStorage.CreateDirectory("/test");
		Assert.IsTrue(_tempStorage.ExistFolder("/test"));
		_tempStorage.FolderDelete("/test");
		Assert.IsFalse(_tempStorage.ExistFolder("/test"));
	}

	[TestMethod]
	public void GetAllFilesInDirectoryRecursive_TempStorage()
	{
		// Setup env
		_tempStorage.CreateDirectory("/test_GetAllFilesInDirectoryRecursive");
		_tempStorage.CreateDirectory("/test_GetAllFilesInDirectoryRecursive/test");
		const string fileAlreadyExistSubPath =
			"/test_GetAllFilesInDirectoryRecursive/test/already_09010.tmp";
		_tempStorage.WriteStream(StringToStreamHelper.StringToStream("test"),
			fileAlreadyExistSubPath);

		var filesInFolder = _tempStorage.GetAllFilesInDirectoryRecursive(
			"/test_GetAllFilesInDirectoryRecursive").ToList();

		Assert.AreNotEqual(0, filesInFolder.Count);
		Assert.AreEqual("/test_GetAllFilesInDirectoryRecursive/test", filesInFolder[0]);
		Assert.AreEqual("/test_GetAllFilesInDirectoryRecursive/test/already_09010.tmp",
			filesInFolder[1]);

		_tempStorage.FolderDelete("/test_GetAllFilesInDirectoryRecursive");
	}

	[TestMethod]
	public void GetDirectories_Null_NotFound_TempStorage()
	{
		var result = _tempStorage.GetDirectories("/not_found");
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void GetDirectoryRecursive_Null_NotFound()
	{
		var result = _tempStorage.GetDirectoryRecursive("/not_found").Select(p => p.Key);
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void GetAllFilesInDirectoryRecursive_NotFound()
	{
		var filesInFolder = _tempStorage.GetAllFilesInDirectoryRecursive(
			"/not_found").ToList();
		Assert.AreEqual(0, filesInFolder.Count);
	}

	[TestMethod]
	public void ReadStream_NotFound()
	{
		var result = _tempStorage.ReadStream("not-found");
		Assert.AreEqual(Stream.Null, result);
	}

	[TestMethod]
	public void WriteStreamOpenOrCreate()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_tempStorage.WriteStreamOpenOrCreate(Stream.Null, "not-found"));
	}

	[TestMethod]
	public void Info()
	{
		Assert.AreEqual(CreateAnImage.Size, _tempStorage.Info(_fileName).Size);
	}
}
