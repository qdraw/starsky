using System;
using System.IO;
using System.Linq;
using System.Threading;
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

	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task Temporary_FileMove_Test()
	{
		var createNewImage = new CreateAnImage();
		const string toMoveFileName = "StorageTemporaryFilesystemTest_FileMove.jpg";
		const string startMoveFile = "start_move_file.jpg";

		// first copy for parallel test
		_tempStorage.FileCopy(_fileName, startMoveFile);

		// Retry logic for flaky UnauthorizedAccessException
		const int maxRetries = 3;
		var attempt = 0;
		var moved = false;
		Exception? lastException = null;
		while ( attempt < maxRetries && !moved )
		{
			try
			{
				_tempStorage.FileMove(startMoveFile, toMoveFileName);
				moved = true;
			}
			catch ( Exception ex )
			{
				lastException = ex;
				await Task.Delay(100, TestContext.CancellationTokenSource.Token);
				attempt++;
			}
		}

		if ( !moved && lastException != null )
		{
			Assert.Fail($"FileMove failed after {maxRetries} attempts: {lastException.Message}");
		}

		var path = Path.Combine(createNewImage.BasePath, startMoveFile);
		Assert.IsFalse(File.Exists(path));
		var path2 = Path.Combine(createNewImage.BasePath, toMoveFileName);
		Assert.IsTrue(File.Exists(path2));

		File.Delete(Path.Combine(createNewImage.BasePath, startMoveFile));
		File.Delete(Path.Combine(createNewImage.BasePath, toMoveFileName));

		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);
	}

	[TestMethod]
	public void Temporary_FileMove_NotFound()
	{
		Assert.IsFalse(_tempStorage.FileMove("not-found",
			"StorageThumbnailFilesystemTest_FileMove.jpg"));
	}

	[TestMethod]
	public async Task Temporary_FileMove_SkipIfAlreadyExists()
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
	public async Task Temporary_FileCopy_success()
	{
		var createNewImage = new CreateAnImage();
		const string fileCopyName = "StorageTemporaryFilesystemTestSuccess_FileCopy.jpg";

		_tempStorage.FileCopy(_fileName, fileCopyName);

		var path = Path.Combine(createNewImage.BasePath, _fileName);
		Assert.IsTrue(File.Exists(path));
		var path2 = Path.Combine(createNewImage.BasePath, fileCopyName);
		Assert.IsTrue(File.Exists(path2));

		File.Delete(_fileName);
		try
		{
			File.Delete(Path.Combine(createNewImage.BasePath, fileCopyName));
		}
		catch ( IOException )
		{
			Console.WriteLine(fileCopyName +
			                  " was not deleted, retrying");
			await Task.Delay(1000, TestContext.CancellationTokenSource.Token);
			File.Delete(Path.Combine(createNewImage.BasePath, fileCopyName));
		}

		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);
	}

	[TestMethod]
	public void Temporary_FileCopy_source_notFound()
	{
		var createNewImage = new CreateAnImage();

		_tempStorage.FileCopy("not_found", "StorageThumbnailFilesystemTest_FileCopy2.jpg");

		var path2 = Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileCopy2.jpg");
		Assert.IsFalse(File.Exists(path2));
	}

	[TestMethod]
	public void Temporary_FileDelete_NotExist()
	{
		Assert.IsFalse(_tempStorage.FileDelete("NotFound"));
	}

	[TestMethod]
	public void Temporary_ReadStream()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _tempStorage.ReadStream(_fileName);
		Assert.AreEqual(CreateAnImage.Bytes.Length, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void Temporary_ReadStream_MaxLength()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _tempStorage.ReadStream(_fileName, 100);
		Assert.AreEqual(100, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void Temporary_WriteStream()
	{
		var createNewImage = new CreateAnImage();
		const string filename = "StorageTemporaryFilesystemTest_WriteStream.jpg";

		_tempStorage.WriteStream(new MemoryStream([.. CreateAnImage.Bytes]),
			filename);

		var readStream =
			_tempStorage.ReadStream(filename);
		Assert.AreEqual(CreateAnImage.Bytes.Length, readStream.Length);
		readStream.Dispose();

		File.Delete(Path.Combine(createNewImage.BasePath, filename));
	}

	[TestMethod]
	public async Task Temporary_WriteStreamAsync()
	{
		var createNewImage = new CreateAnImage();
		const string filename = "StorageTemporaryFilesystemTest_WriteStreamAsync.jpg";

		await _tempStorage.WriteStreamAsync(
			new MemoryStream([.. CreateAnImage.Bytes]),
			filename);

		var readStream =
			_tempStorage.ReadStream(filename);
		Assert.AreEqual(CreateAnImage.Bytes.Length, readStream.Length);
		await readStream.DisposeAsync();

		File.Delete(Path.Combine(createNewImage.BasePath, filename));
	}

	[TestMethod]
	public async Task Temporary_IsFileReady()
	{
		var createNewImage = new CreateAnImage();

		const string thumbnailId = "IsFileReady_thumbnailStorage";
		// first copy for parallel test
		_tempStorage.FileCopy(_fileName, thumbnailId);

		var stream = _tempStorage.ReadStream(thumbnailId);

		var result = _tempStorage.IsFileReady(thumbnailId);
		Assert.IsFalse(result);

		// is disposed too late (as designed)
		await stream.DisposeAsync();

		try
		{
			var result2 = _tempStorage.IsFileReady(thumbnailId);
			Assert.IsTrue(result2);
		}
		catch ( Exception )
		{
			await Task.Delay(100, TestContext.CancellationToken);
			var result2 = _tempStorage.IsFileReady(thumbnailId);
			Assert.IsTrue(result2);
		}

		File.Delete(Path.Combine(createNewImage.BasePath,
			$"{thumbnailId}"));

		Assert.IsFalse(_tempStorage.ExistFile(thumbnailId));
	}

	[TestMethod]
	public void Temporary_IsFolderOrFile_Exists()
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
	public void Temporary_IsFolderOrFile_NotFound()
	{
		var result = _tempStorage.IsFolderOrFile("not-found");
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Deleted, result);
	}

	[TestMethod]
	public void Temporary_FolderMove()
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
	public void Temporary_CreateDirectory()
	{
		_tempStorage.CreateDirectory("/test");
		Assert.IsTrue(_tempStorage.ExistFolder("/test"));
		_tempStorage.FolderDelete("/test");
	}

	[TestMethod]
	public void Temporary_CreateDirectory_OverFile()
	{
		_tempStorage.FolderDelete("/test2");

		_tempStorage.WriteStream(StringToStreamHelper.StringToStream("test"), "/test2");
		Assert.IsFalse(_tempStorage.CreateDirectory("/test2"));

		_tempStorage.FileDelete("/test2");
	}

	[TestMethod]
	public void Temporary_FolderDelete()
	{
		_tempStorage.CreateDirectory("/test");
		Assert.IsTrue(_tempStorage.ExistFolder("/test"));
		_tempStorage.FolderDelete("/test");
		Assert.IsFalse(_tempStorage.ExistFolder("/test"));
	}

	[TestMethod]
	public void Temporary_GetAllFilesInDirectoryRecursive()
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
	public void Temporary_GetDirectories_Null_NotFound()
	{
		var result = _tempStorage.GetDirectories("/not_found");
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void Temporary_GetDirectoryRecursive_Null_NotFound()
	{
		var result = _tempStorage.GetDirectoryRecursive("/not_found").Select(p => p.Key);
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void Temporary_GetAllFilesInDirectoryRecursive_NotFound()
	{
		var filesInFolder = _tempStorage.GetAllFilesInDirectoryRecursive(
			"/not_found").ToList();
		Assert.IsEmpty(filesInFolder);
	}

	[TestMethod]
	public void Temporary_ReadStream_NotFound()
	{
		var result = _tempStorage.ReadStream("not-found");
		Assert.AreEqual(Stream.Null, result);
	}

	[TestMethod]
	public void Temporary_WriteStreamOpenOrCreate()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_tempStorage.WriteStreamOpenOrCreate(Stream.Null, "not-found"));
	}

	[TestMethod]
	public void Temporary_Info()
	{
		var size = _tempStorage.Info(_fileName).Size;

		// If the file is not found, it will create a new image
		if ( size <= 8000 )
		{
			var createNewImage = new CreateAnImage();
			_fileName = createNewImage.FileName;

			size = _tempStorage.Info(_fileName).Size;
		}

		Assert.AreEqual(CreateAnImage.Size, size);
	}

	[TestMethod]
	public void Temporary_ReadLinesAsync()
	{
		Assert.ThrowsExactly<NotSupportedException>(() =>
			_tempStorage.ReadLinesAsync("not-found", new CancellationToken(true)));
	}
}
