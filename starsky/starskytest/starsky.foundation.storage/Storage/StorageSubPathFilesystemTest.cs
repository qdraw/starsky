using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Storage;

/// <summary>
///     StorageSubPathFilesystemTest or StorageFilesystemTest
/// </summary>
[TestClass]
public sealed class StorageSubPathFilesystemTest
{
	private readonly CreateAnImage _newImage;
	private readonly StorageSubPathFilesystem _storage;
	private readonly string _fileName;

	public StorageSubPathFilesystemTest()
	{
		_newImage = new CreateAnImage();
		_fileName  = _newImage.FileName;
		var appSettings = new AppSettings { StorageFolder = _newImage.BasePath };
		_storage = new StorageSubPathFilesystem(appSettings, new FakeIWebLogger());
	}

	[TestMethod]
	public void StorageFilesystem_GetAllFilesDirectoryTest()
	{
		// Assumes that
		//     ~/.nuget/packages/microsoft.testplatform.testhost/15.6.0/lib/netstandard1.5/
		// has subfolders

		// Used For subfolders
		_storage.CreateDirectory("/test");
		var filesInFolder = _storage.GetDirectoryRecursive("/").Select(p => p.Key).ToList();

		Assert.AreNotEqual(0, filesInFolder.Count);

		_storage.FolderDelete("/test");
	}

	[TestMethod]
	public void GetAllFilesInDirectory_Null_NotFound()
	{
		var result = _storage.GetAllFilesInDirectory("/not_found");
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void GetDirectories_Null_NotFound()
	{
		var result = _storage.GetDirectories("/not_found");
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void GetDirectoryRecursive_Null_NotFound()
	{
		var result = _storage.GetDirectoryRecursive("/not_found").Select(p => p.Key);
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public void GetAllFilesInDirectoryRecursive()
	{
		// Setup env
		_storage.CreateDirectory("/test_GetAllFilesInDirectoryRecursive");
		_storage.CreateDirectory("/test_GetAllFilesInDirectoryRecursive/test");
		var fileAlreadyExistSubPath =
			"/test_GetAllFilesInDirectoryRecursive/test/already_09010.tmp";
		_storage.WriteStream(StringToStreamHelper.StringToStream("test"),
			fileAlreadyExistSubPath);

		var filesInFolder = _storage.GetAllFilesInDirectoryRecursive(
			"/test_GetAllFilesInDirectoryRecursive").ToList();

		Assert.AreNotEqual(0, filesInFolder.Count);
		Assert.AreEqual("/test_GetAllFilesInDirectoryRecursive/test", filesInFolder[0]);
		Assert.AreEqual("/test_GetAllFilesInDirectoryRecursive/test/already_09010.tmp",
			filesInFolder[1]);

		_storage.FolderDelete("/test_GetAllFilesInDirectoryRecursive");
	}

	[TestMethod]
	public void GetAllFilesInDirectoryRecursive_NotFound()
	{
		var filesInFolder = _storage.GetAllFilesInDirectoryRecursive(
			"/not_found").ToList();
		Assert.AreEqual(0, filesInFolder.Count);
	}


	[TestMethod]
	public void FileCopy()
	{
		const string from = "/test_file_copy.tmp";
		const string to = "/test_file_copy_2.tmp";

		_storage.WriteStream(StringToStreamHelper.StringToStream("test"),
			from);
		_storage.FileCopy(from, to);

		Assert.IsTrue(_storage.ExistFile(from));
		Assert.IsTrue(_storage.ExistFile(to));

		_storage.FileDelete(from);
		_storage.FileDelete(to);
	}

	[TestMethod]
	public void FileMove_IsMoved()
	{
		const string from = "/test_file_move.tmp";
		const string to = "/test_file_move_2.tmp";

		_storage.WriteStream(StringToStreamHelper.StringToStream("test"),
			from);
		_storage.FileMove(from, to);

		Assert.IsFalse(_storage.ExistFile(from));
		Assert.IsTrue(_storage.ExistFile(to));

		_storage.FileDelete(from);
		_storage.FileDelete(to);
	}

	[TestMethod]
	public void FolderMove()
	{
		const string from = "/test_folder_move_from";
		const string to = "/test_folder_move_to";

		_storage.CreateDirectory(from);
		if ( _storage.ExistFolder(to) )
		{
			_storage.FolderDelete(to);
		}

		_storage.FolderMove(from, to);

		if ( _storage.ExistFolder(from) )
		{
			_storage.FolderDelete(from);
		}

		Assert.IsFalse(_storage.ExistFolder(from));
		Assert.IsTrue(_storage.ExistFolder(to));

		_storage.FolderDelete(to);
	}

	[TestMethod]
	public void ExistFileNotFound()
	{
		Assert.IsFalse(_storage.ExistFile("not_found"));
	}

	[TestMethod]
	public void FolderDelete()
	{
		var folderDeleteName = "temp_folder_delete";
		_storage.CreateDirectory($"/{folderDeleteName}");

		_storage.FolderDelete($"/{folderDeleteName}");

		Assert.IsFalse(Directory.Exists(Path.Combine(_newImage.BasePath, folderDeleteName)));
	}

	[TestMethod]
	public void ReadStream_MaxLength()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _storage.ReadStream(_newImage.DbPath, 100);
		Assert.AreEqual(100, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void ReadStream_NotFound()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _storage.ReadStream("not-found", 100);
		Assert.AreEqual(0, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void SetLastWriteTime_Dir()
	{
		const string rootDir = "/test01012_sub";

		_storage.CreateDirectory(rootDir);

		var shouldBe = DateTime.Now.AddDays(-1);
		_storage.SetLastWriteTime(rootDir, shouldBe);

		var lastWriteTime2 = _storage.Info(rootDir).LastWriteTime;
		_storage.FolderDelete(rootDir);

		Assert.AreEqual(shouldBe, lastWriteTime2);
	}

	[TestMethod]
	public void SetLastWriteTime_File()
	{
		const string tmpFile = "/test01012_sub.tmp";

		_storage.WriteStream(new MemoryStream(new byte[1]), tmpFile);

		var shouldBe = DateTime.Now.AddDays(-1);
		_storage.SetLastWriteTime(tmpFile, shouldBe);

		var lastWriteTime2 = _storage.Info(tmpFile).LastWriteTime;
		_storage.FileDelete(tmpFile);

		Assert.AreEqual(shouldBe, lastWriteTime2);
	}

	[TestMethod]
	public void IsFileReady_SubPath()
	{
		var createNewImage = new CreateAnImage();

		const string subPath = "/IsFileReady_SubPath.jpg";
		// first copy for parallel test
		_storage.FileCopy(createNewImage.DbPath, subPath);

		var stream = _storage.ReadStream(subPath);

		var result = _storage.IsFileReady(subPath);
		Assert.IsFalse(result);

		// is disposed to late (as designed)
		stream.Dispose();

		var result2 = _storage.IsFileReady(subPath);
		Assert.IsTrue(result2);

		_storage.FileDelete(subPath);
		Assert.IsFalse(_storage.ExistFile(subPath));
	}
	
	[TestMethod]
	public void IsFolderOrFile_Exists()
	{
		var result = _storage.IsFolderOrFile(_fileName);
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File, result);
	}
	
	[TestMethod]
	public void IsFolderOrFile_NotFound()
	{
		var result = _storage.IsFolderOrFile("not-found");
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Deleted, result);
	}
}
