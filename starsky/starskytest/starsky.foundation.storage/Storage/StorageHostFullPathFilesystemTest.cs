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
			var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + Path.DirectorySeparatorChar;

			var content = new StorageHostFullPathFilesystem().GetAllFilesInDirectoryRecursive(path).ToList();

			Console.WriteLine("count => "+ content.Count());

			// Gives a list of the content in the temp folder.
			Assert.AreEqual(true, content.Any());            
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
				Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("The filename, directory name, or volume label syntax is incorrect"));
			}
			else
			{
				Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("Could not find a part of the path"));
			}
			
			Assert.AreEqual(0,directories.Count());
		}
		
		[TestMethod]
		public void InfoNotFound()
		{
			var info = new StorageHostFullPathFilesystem().Info("C://folder-not-found-992544124712741");
			Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Deleted, info.IsFolderOrFile );
		}
		
		[TestMethod]
		public void Info_Directory()
		{
			var rootDir = Path.Combine(new CreateAnImage().BasePath, "4895893hier");
			new StorageHostFullPathFilesystem().CreateDirectory(rootDir);

			var info = new StorageHostFullPathFilesystem().Info(rootDir);
			new StorageHostFullPathFilesystem().FolderDelete(rootDir);
			
			Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Folder, info.IsFolderOrFile );
			Assert.AreEqual(false, info.IsFileSystemReadOnly );
		}
		
		[TestMethod]
		public void TestIfFileSystemIsReadOnly_True()
		{
			// not found is also readonly
			var result = StorageHostFullPathFilesystem.TestIfFileSystemIsReadOnly("/382sfdkjssfd", FolderOrFileModel.FolderOrFileTypeList.Folder);
			
			Assert.AreEqual(true, result );
		}
		
		[TestMethod]
		public void TestIfFileSystemIsReadOnly_False()
		{
			var rootDir = Path.Combine(new CreateAnImage().BasePath, "4895893hier");
			new StorageHostFullPathFilesystem().CreateDirectory(rootDir);

			var result = StorageHostFullPathFilesystem.TestIfFileSystemIsReadOnly(rootDir, FolderOrFileModel.FolderOrFileTypeList.Folder);

			new StorageHostFullPathFilesystem().FolderDelete(rootDir);
			
			Assert.AreEqual(false, result );
		}
		
		[TestMethod]
		public void FolderDelete_ChildFoldersAreDeleted()
		{
			var rootDir = Path.Combine(new CreateAnImage().BasePath, "test01010");
			var childDir = Path.Combine(new CreateAnImage().BasePath, "test01010","included-folder");

			var realStorage = new StorageHostFullPathFilesystem();
			realStorage.CreateDirectory(rootDir);
			realStorage.CreateDirectory(childDir);

			realStorage.FolderDelete(rootDir);

			Assert.AreEqual(false,realStorage.ExistFolder(rootDir));
			Assert.AreEqual(false,realStorage.ExistFolder(childDir));
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
				Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("The filename, directory name, or volume label syntax is incorrect"));
			}
			else
			{
				Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("Could not find a part of the path"));
			}
			
			Assert.AreEqual(0,directories.Item1.Length);
			Assert.AreEqual(0,directories.Item2.Length);
		}

	}
}
