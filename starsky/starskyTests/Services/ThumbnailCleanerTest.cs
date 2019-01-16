using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;
using starskytests.FakeCreateAn;

namespace starskytests.Services
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
			new ThumbnailCleaner(_query,appsettings).CleanAllUnusedFiles();
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
				FileHash = "EXIST"
			});

			var appSettings = new AppSettings
			{
				ThumbnailTempFolder = existFullDir,
				Verbose = true
			};
			
			var thumbnailCleaner = new ThumbnailCleaner(_query,appSettings);
			
			// there are now two files inside this dir
			var allThumbnailFilesBefore = thumbnailCleaner.GetAllThumbnailFiles();
			Assert.AreEqual(2,allThumbnailFilesBefore.Length);
			
			thumbnailCleaner.CleanAllUnusedFiles();
			
			// DELETE.jpg is removed > is missing in database
			var allThumbnailFilesAfter = thumbnailCleaner.GetAllThumbnailFiles();
			Assert.AreEqual(1,allThumbnailFilesAfter.Length);
			
			Files.DeleteDirectory(existFullDir);
		}



	}
}
