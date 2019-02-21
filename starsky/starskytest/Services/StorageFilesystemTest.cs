using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
			var filesInFolder = new StorageFilesystem(new AppSettings{StorageFolder = newImage.BasePath}).GetDirectoryRecursive("/");
			Assert.AreEqual(true,filesInFolder.Any());
            
		}
	}
}
