using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Storage
{
	[TestClass]
	public class StorageHostFullPathFilesystemTest
	{
		[TestMethod]
		public void Files_GetFilesRecursiveTest()
		{            
			var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;

			var content = new StorageHostFullPathFilesystem().GetAllFilesInDirectoryRecursive(path);

			Console.WriteLine("count => "+ content.Count());

			// Gives a list of the content in the temp folder.
			Assert.AreEqual(true, content.Any());            
		}
		
		[TestMethod]
		public void GetAllFilesInDirectoryRecursive_NotFound()
		{
			var logger = new FakeIWebLogger();
			var realStorage = new StorageHostFullPathFilesystem(logger);
			var directories = realStorage.GetAllFilesInDirectoryRecursive("NOT:\\t");
			
			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("Could not find a part of the path"));
			Assert.AreEqual(directories.Count(),0);
		}
		
		[TestMethod]
		public void InfoNotFound()
		{
			var info = new StorageHostFullPathFilesystem().Info("C://folder-not-found-992544124712741");
			Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Deleted, info.IsFolderOrFile );
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
		public void GetFilesAndDirectories_Exception_NotFound()
		{
			var logger = new FakeIWebLogger();
			var realStorage = new StorageHostFullPathFilesystem(logger);
			var directories = realStorage.GetFilesAndDirectories("NOT:\\t");
			
			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("Could not find a part of the path"));
			Assert.AreEqual(directories.Item1.Length,0);
			Assert.AreEqual(directories.Item2.Length,0);
		}

	}
}
