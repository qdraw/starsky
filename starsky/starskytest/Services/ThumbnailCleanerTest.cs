using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailgeneration.Services;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskycore.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using Query = starskycore.Services.Query;

namespace starskytest.Services
{
	[TestClass]
	public class ThumbnailCleanerTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly Query _query;

		public ThumbnailCleanerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("test");
			var options = builder.Options;
			var context = new ApplicationDbContext(options);
			_query = new Query(context,_memoryCache, new AppSettings());
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
			
			if (!File.Exists(Path.Join(existFullDir,"EXIST.jpg"))) File.Copy(createAnImage.FullFilePath, Path.Join(existFullDir,"EXIST.jpg"));
			if (!File.Exists(Path.Join(existFullDir,"DELETE.jpg"))) File.Copy(createAnImage.FullFilePath, Path.Join(existFullDir,"DELETE.jpg"));


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
			var hostFullPathFilesystem = new StorageHostFullPathFilesystem();
			var thumbnailCleaner = new ThumbnailCleaner(hostFullPathFilesystem, _query,appSettings);
			
			// there are now two files inside this dir
			var allThumbnailFilesBefore = thumbnailCleaner.GetAllThumbnailFiles();
			Assert.AreEqual(2,allThumbnailFilesBefore.Length);
			
			thumbnailCleaner.CleanAllUnusedFiles();
			
			// DELETE.jpg is removed > is missing in database
			var allThumbnailFilesAfter = thumbnailCleaner.GetAllThumbnailFiles();
			Assert.AreEqual(1,allThumbnailFilesAfter.Length);

			hostFullPathFilesystem.FolderDelete(existFullDir);
		}



	}
}
