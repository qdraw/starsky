using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Storage
{
	[TestClass]
	public sealed class StorageHostFullPathFilesystemTest
	{
		[TestMethod]
		public void Files_GetFilesRecursiveTest()
		{
			var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) +
			           Path.DirectorySeparatorChar;

			var content = new StorageHostFullPathFilesystem().GetAllFilesInDirectoryRecursive(path)
				.ToList();

			Console.WriteLine("count => " + content.Count);

			// Gives a list of the content in the temp folder.
			Assert.AreEqual(true, content.Count != 0);
		}

		[TestMethod]
		public void Files_GetFilesRecursive_NotFound()
		{
			var service = new StorageHostFullPathFilesystem();
			var content = service.GetAllFilesInDirectory("not-found-directory-24785895348934598543")
				.ToList();
			Assert.AreEqual(0, content.Count);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
		public void GetAllFilesInDirectoryRecursive_NotFound()
		{
			var logger = new FakeIWebLogger();
			var realStorage = new StorageHostFullPathFilesystem(logger);
			var directories = realStorage.GetAllFilesInDirectoryRecursive("NOT:\\t");

			if ( new AppSettings().IsWindows )
			{
				Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2
					?.Contains(
						"The filename, directory name, or volume label syntax is incorrect"));
			}
			else
			{
				Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2
					?.Contains("Could not find a part of the path"));
			}

			Assert.AreEqual(0, directories.Count());
		}

		[TestMethod]
		public void InfoNotFound()
		{
			var info =
				new StorageHostFullPathFilesystem().Info("C://folder-not-found-992544124712741");
			Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Deleted, info.IsFolderOrFile);
		}

		[TestMethod]
		public void Info_Directory()
		{
			var rootDir = Path.Combine(new CreateAnImage().BasePath, "4895893_here");
			new StorageHostFullPathFilesystem().CreateDirectory(rootDir);

			var info = new StorageHostFullPathFilesystem().Info(rootDir);
			new StorageHostFullPathFilesystem().FolderDelete(rootDir);

			Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Folder, info.IsFolderOrFile);
			Assert.AreEqual(false, info.IsFileSystemReadOnly);
			Assert.AreEqual(true, info.IsDirectory);
		}

		[TestMethod]
		public void TestIfFileSystemIsReadOnly_True()
		{
			// not found is also readonly
			var result = StorageHostFullPathFilesystem.TestIfFileSystemIsReadOnly("/test_test_test",
				FolderOrFileModel.FolderOrFileTypeList.Folder);

			Assert.AreEqual(true, result);
		}

		[TestMethod]
		public void TestIfFileSystemIsReadOnly_False()
		{
			var rootDir = Path.Combine(new CreateAnImage().BasePath, "4895893hier");
			new StorageHostFullPathFilesystem().CreateDirectory(rootDir);

			var result = StorageHostFullPathFilesystem.TestIfFileSystemIsReadOnly(rootDir,
				FolderOrFileModel.FolderOrFileTypeList.Folder);

			new StorageHostFullPathFilesystem().FolderDelete(rootDir);

			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void FolderDelete_ChildFoldersAreDeleted()
		{
			var rootDir = Path.Combine(new CreateAnImage().BasePath, "test01010");
			var childDir =
				Path.Combine(new CreateAnImage().BasePath, "test01010", "included-folder");

			var realStorage = new StorageHostFullPathFilesystem();
			realStorage.CreateDirectory(rootDir);
			realStorage.CreateDirectory(childDir);

			realStorage.FolderDelete(rootDir);

			Assert.AreEqual(false, realStorage.ExistFolder(rootDir));
			Assert.AreEqual(false, realStorage.ExistFolder(childDir));
		}

		[TestMethod]
		public void FolderDelete_NotFound()
		{
			var realStorage = new StorageHostFullPathFilesystem();
			var result = realStorage.FolderDelete("not-found-directory-24785895348934598543");

			Assert.AreEqual(false, result);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
		public void GetFilesAndDirectories_Exception_NotFound()
		{
			var logger = new FakeIWebLogger();
			var realStorage = new StorageHostFullPathFilesystem(logger);
			var directories = realStorage.GetFilesAndDirectories("NOT:\\t");

			if ( new AppSettings().IsWindows )
			{
				Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2?
					.Contains("The filename, directory name, or volume label syntax is incorrect"));
			}
			else
			{
				Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2
					?.Contains("Could not find a part of the path"));
			}

			Assert.AreEqual(0, directories.Item1.Length);
			Assert.AreEqual(0, directories.Item2.Length);
		}

		[TestMethod]
		public void SetLastWriteTime_Dir()
		{
			var rootDir = Path.Combine(new CreateAnImage().BasePath, "test01012");

			var realStorage = new StorageHostFullPathFilesystem();
			realStorage.CreateDirectory(rootDir);

			var shouldBe = DateTime.Now.AddDays(-1);
			realStorage.SetLastWriteTime(rootDir, shouldBe);

			var lastWriteTime2 = realStorage.Info(rootDir).LastWriteTime;
			realStorage.FolderDelete(rootDir);

			Assert.AreEqual(shouldBe, lastWriteTime2);
		}

		[TestMethod]
		public void SetLastWriteTime_File()
		{
			var tmpFile = Path.Combine(new CreateAnImage().BasePath, "test01012.tmp");

			var realStorage = new StorageHostFullPathFilesystem();
			realStorage.WriteStream(new MemoryStream(new byte[1]), tmpFile);

			var shouldBe = DateTime.Now.AddDays(-1);
			realStorage.SetLastWriteTime(tmpFile, shouldBe);

			var lastWriteTime2 = realStorage.Info(tmpFile).LastWriteTime;
			realStorage.FileDelete(tmpFile);

			Assert.AreEqual(shouldBe, lastWriteTime2);
		}

		[TestMethod]
		public void SetLastWriteTime_File_Now()
		{
			var tmpFile = Path.Combine(new CreateAnImage().BasePath, "test01013.tmp");

			var realStorage = new StorageHostFullPathFilesystem();
			realStorage.WriteStream(new MemoryStream(new byte[1]), tmpFile);

			var result = realStorage.SetLastWriteTime(tmpFile);

			var lastWriteTime2 = realStorage.Info(tmpFile).LastWriteTime;
			realStorage.FileDelete(tmpFile);

			Assert.AreEqual(result, lastWriteTime2);
		}

		[TestMethod]
		public void SetLastWriteTime_File_LongTimeAgo()
		{
			var tmpFile = Path.Combine(new CreateAnImage().BasePath, "test01014.tmp");

			var realStorage = new StorageHostFullPathFilesystem();
			realStorage.WriteStream(new MemoryStream(new byte[1]), tmpFile);

			var date = new DateTime(1999, 01, 01,
				01, 01, 01, kind: DateTimeKind.Local);

			var result = realStorage.SetLastWriteTime(tmpFile, date);

			var lastWriteTime2 = realStorage.Info(tmpFile).LastWriteTime;
			realStorage.FileDelete(tmpFile);

			Assert.AreEqual(result, lastWriteTime2);
		}

		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public void ReadStream_NotFound_System_IO_FileNotFoundException()
		{
			var service = new StorageHostFullPathFilesystem();
			service.ReadStream("not-found-directory-24785895348934598543");
		}

		[TestMethod]
		public void FileMove_SamePaths()
		{
			var service = new StorageHostFullPathFilesystem();
			var result = service.FileMove("test", "test");
			Assert.AreEqual(false, result);
		}
	}
}
