using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.storage.Storage
{
	[TestClass]
	public class StorageFilesystemTest
	{
		private  readonly StorageSubPathFilesystem _storage;

		public StorageFilesystemTest()
		{
			var newImage = new CreateAnImage();
			var appSettings = new AppSettings {StorageFolder = newImage.BasePath};
			_storage = new StorageSubPathFilesystem(appSettings);
		}
		
		[TestMethod]
		public void StorageFilesystem_GetAllFilesDirectoryTest()
		{
			// Assumes that
			//     ~/.nuget/packages/microsoft.testplatform.testhost/15.6.0/lib/netstandard1.5/
			// has subfolders
            
			// Used For subfolders
			_storage.CreateDirectory("/test");
			var filesInFolder = _storage.GetDirectoryRecursive("/").ToList();
			
			Assert.AreEqual(true,filesInFolder.Any());

			_storage.FolderDelete("/test");
		}

		[TestMethod]
		public void GetAllFilesInDirectory_Null_NotFound()
		{
			var result = _storage.GetAllFilesInDirectory("/not_found");
			Assert.AreEqual(0,result.Count());
		}
		
		[TestMethod]
		public void GetDirectories_Null_NotFound()
		{
			var result = _storage.GetDirectories("/not_found");
			Assert.AreEqual(0,result.Count());
		}

		[TestMethod]
		public void GetDirectoryRecursive_Null_NotFound()
		{
			var result = _storage.GetDirectoryRecursive("/not_found");
			Assert.AreEqual(0,result.Count());
		}
		
		[TestMethod]
		public void GetAllFilesInDirectoryRecursive()
		{
			// Setup env
			_storage.CreateDirectory("/test_GetAllFilesInDirectoryRecursive");
			_storage.CreateDirectory("/test_GetAllFilesInDirectoryRecursive/test");
			var fileAlreadyExistSubPath = "/test_GetAllFilesInDirectoryRecursive/test/already_09010.tmp";
			_storage.WriteStream(new PlainTextFileHelper().StringToStream("test"),
				fileAlreadyExistSubPath);
			
			var filesInFolder = _storage.GetAllFilesInDirectoryRecursive(
				"/test_GetAllFilesInDirectoryRecursive").ToList();

			Assert.AreEqual(true,filesInFolder.Any());
			Assert.AreEqual("/test_GetAllFilesInDirectoryRecursive/test", filesInFolder[0]);
			Assert.AreEqual("/test_GetAllFilesInDirectoryRecursive/test/already_09010.tmp", filesInFolder[1]);

			_storage.FolderDelete("/test_GetAllFilesInDirectoryRecursive");
		}
		
	}
}
