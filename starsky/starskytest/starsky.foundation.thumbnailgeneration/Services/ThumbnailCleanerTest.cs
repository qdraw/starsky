using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Services
{
	[TestClass]
	public class ThumbnailCleanerTest
	{
		private readonly Query _query;

		public ThumbnailCleanerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("test");
			var options = builder.Options;
			var context = new ApplicationDbContext(options);
			_query = new Query(context,memoryCache, new AppSettings());
		}
		
		[TestMethod]
		[ExpectedException(typeof(DirectoryNotFoundException))]
		public void ThumbnailCleanerTest_DirectoryNotFoundException()
		{
			var appsettings = new AppSettings {ThumbnailTempFolder = "\""};
			new ThumbnailCleaner(new FakeIStorage(), _query,appsettings).CleanAllUnusedFiles();
		}
		
		[TestMethod]
		public void ThumbnailCleanerTest_Cleaner()
		{
			var createAnImage = new CreateAnImage();

			var existFullDir = createAnImage.BasePath + Path.DirectorySeparatorChar + "thumb";
			if (!Directory.Exists(existFullDir))
			{
				Directory.CreateDirectory(existFullDir);
			}
			
			if (!File.Exists(Path.Join(existFullDir,"EXIST.jpg"))) File.Copy(createAnImage.FullFilePath, 
				Path.Join(existFullDir,"EXIST.jpg"));
			if (!File.Exists(Path.Join(existFullDir,"DELETE.jpg"))) File.Copy(createAnImage.FullFilePath, 
				Path.Join(existFullDir,"DELETE.jpg"));

			_query.AddItem(new FileIndexItem
			{
				FileHash = "EXIST",
				FileName = "exst2"
			});

			var appSettings = new AppSettings
			{
				ThumbnailTempFolder = existFullDir,
				Verbose = true
			};
			var thumbnailStorage = new StorageThumbnailFilesystem(appSettings);
			
			var thumbnailCleaner = new ThumbnailCleaner(thumbnailStorage, _query,appSettings);
			
			// there are now two files inside this dir
			var allThumbnailFilesBefore = thumbnailStorage.GetAllFilesInDirectory("/");
			Assert.AreEqual(2,allThumbnailFilesBefore.Count());
			
			thumbnailCleaner.CleanAllUnusedFiles();
			
			// DELETE.jpg is removed > is missing in database
			var allThumbnailFilesAfter = thumbnailStorage.GetAllFilesInDirectory("/");
			Assert.AreEqual(1,allThumbnailFilesAfter.Count());

			new StorageHostFullPathFilesystem().FolderDelete(existFullDir);
		}



	}
}
