using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;

namespace starskytest.Services
{
	[TestClass]
	public class StorageFilesystemTest
	{
		[TestMethod]
		public void StorageFilesystem_GetAllFilesDirectoryTest()
		{
			// Assumes that
			//     ~/.nuget/packages/microsoft.testplatform.testhost/15.6.0/lib/netstandard1.5/
			// has subfolders
            
			// Used For subfolders
			var newImage = new CreateAnImage();
			var appSettings = new AppSettings {StorageFolder = newImage.BasePath};
			var storage = new StorageSubPathFilesystem(appSettings);
			storage.CreateDirectory("test");
			var filesInFolder = storage.GetDirectoryRecursive("/").ToList();
			
			Assert.AreEqual(true,filesInFolder.Any());
		}
	}
}
