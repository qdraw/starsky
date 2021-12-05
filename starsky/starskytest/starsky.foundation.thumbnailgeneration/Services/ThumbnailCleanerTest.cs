using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
// #pragma warning disable 618

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
			var appSettings = new AppSettings {ThumbnailTempFolder = "\""};
			new ThumbnailCleaner(new FakeIStorage(), _query,appSettings, new FakeIWebLogger()).CleanAllUnusedFiles();
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
			var thumbnailStorage = new StorageThumbnailFilesystem(appSettings, new FakeIWebLogger());
			
			var thumbnailCleaner = new ThumbnailCleaner(thumbnailStorage, _query,appSettings, new FakeIWebLogger());
			
			// there are now two files inside this dir
			var allThumbnailFilesBefore = thumbnailStorage.GetAllFilesInDirectory("/");
			Assert.AreEqual(2,allThumbnailFilesBefore.Count());
			
			thumbnailCleaner.CleanAllUnusedFiles();
			
			// DELETE.jpg is removed > is missing in database
			var allThumbnailFilesAfter = thumbnailStorage.GetAllFilesInDirectory("/");
			Assert.AreEqual(1,allThumbnailFilesAfter.Count());

			new StorageHostFullPathFilesystem().FolderDelete(existFullDir);
		}

		[TestMethod]
		public void ThumbnailCleanerTest_Cleaner_WithDifferentSizes()
		{
			var fakeStorage = new FakeIStorage(new List<string> {"/"},
				new List<string>
				{
					ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large),
					ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.ExtraLarge),
					ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.TinyMeta),
					ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta),
					ThumbnailNameHelper.Combine("exist", ThumbnailSize.ExtraLarge),
					ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta),
					ThumbnailNameHelper.Combine("exist", ThumbnailSize.Large),
					ThumbnailNameHelper.Combine("12234456677", ThumbnailSize.ExtraLarge),
				});

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg"){FileHash = "exist"}
			});
			
			var thumbnailCleaner = new ThumbnailCleaner(fakeStorage, fakeQuery, 
				new AppSettings(), new FakeIWebLogger());

			thumbnailCleaner.CleanAllUnusedFiles();

			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta)));
			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.ExtraLarge)));
			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.Large)));
			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta)));
			
			Assert.IsFalse(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.TinyMeta)));
			Assert.IsFalse(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.ExtraLarge)));
			Assert.IsFalse(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large)));
			Assert.IsFalse(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("12234456677", ThumbnailSize.ExtraLarge)));
		}
	
		[TestMethod]
		public void ThumbnailCleanerTest_CatchException()
		{
			var fakeStorage = new FakeIStorage(new List<string> {"/"},
				new List<string>
				{
					ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large),
				});

			var fakeQuery = new FakeIQueryException(new Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException());
			
			var thumbnailCleaner = new ThumbnailCleaner(fakeStorage, fakeQuery, 
				new AppSettings(), new FakeIWebLogger());

			thumbnailCleaner.CleanAllUnusedFiles();

			// the file is there even the connection is crashed
			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large)));
		}
		
		
		[TestMethod]
		[ExpectedException(typeof(DirectoryNotFoundException))]
		public async Task ThumbnailCleanerTestAsync_DirectoryNotFoundException()
		{
			var appSettings = new AppSettings {ThumbnailTempFolder = "\""};
			await new ThumbnailCleaner(new FakeIStorage(), _query,appSettings, new FakeIWebLogger()).CleanAllUnusedFilesAsync();
		}
		
		[TestMethod]
		public async Task ThumbnailCleanerTestAsync_Cleaner()
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

			await _query.AddItemAsync(new FileIndexItem
			{
				FileHash = "EXIST",
				FileName = "exst2"
			});

			var appSettings = new AppSettings
			{
				ThumbnailTempFolder = existFullDir,
				Verbose = true
			};
			var thumbnailStorage = new StorageThumbnailFilesystem(appSettings, new FakeIWebLogger());
			
			var thumbnailCleaner = new ThumbnailCleaner(thumbnailStorage, _query,appSettings, new FakeIWebLogger());
			
			// there are now two files inside this dir
			var allThumbnailFilesBefore = thumbnailStorage.GetAllFilesInDirectory("/");
			Assert.AreEqual(2,allThumbnailFilesBefore.Count());
			
			await thumbnailCleaner.CleanAllUnusedFilesAsync();
			
			// DELETE.jpg is removed > is missing in database
			var allThumbnailFilesAfter = thumbnailStorage.GetAllFilesInDirectory("/");
			Assert.AreEqual(1,allThumbnailFilesAfter.Count());

			new StorageHostFullPathFilesystem().FolderDelete(existFullDir);
		}

			
		[TestMethod]
		public async Task ThumbnailCleanerTestAsync_CatchException()
		{
			var fakeStorage = new FakeIStorage(new List<string> {"/"},
				new List<string>
				{
					ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large),
				});

			var fakeQuery = new FakeIQueryException(new Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException());
			
			var thumbnailCleaner = new ThumbnailCleaner(fakeStorage, fakeQuery, 
				new AppSettings(), new FakeIWebLogger());

			await thumbnailCleaner.CleanAllUnusedFilesAsync();

			// the file is there even the connection is crashed
			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large)));
		}
		
		[TestMethod]
		public async Task ThumbnailCleanerTestAsync_Cleaner_WithDifferentSizes()
		{
			var fakeStorage = new FakeIStorage(new List<string> {"/"},
				new List<string>
				{
					ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large),
					ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.ExtraLarge),
					ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.TinyMeta),
					ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta),
					ThumbnailNameHelper.Combine("exist", ThumbnailSize.ExtraLarge),
					ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta),
					ThumbnailNameHelper.Combine("exist", ThumbnailSize.Large),
					ThumbnailNameHelper.Combine("12234456677", ThumbnailSize.ExtraLarge),
				});

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg"){FileHash = "exist"}
			});
			
			var thumbnailCleaner = new ThumbnailCleaner(fakeStorage, fakeQuery, 
				new AppSettings(), new FakeIWebLogger());

			await thumbnailCleaner.CleanAllUnusedFilesAsync(1);

			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta)));
			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.ExtraLarge)));
			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.Large)));
			Assert.IsTrue(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("exist", ThumbnailSize.TinyMeta)));
			
			Assert.IsFalse(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.TinyMeta)));
			Assert.IsFalse(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.ExtraLarge)));
			Assert.IsFalse(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("hash1234", ThumbnailSize.Large)));
			Assert.IsFalse(fakeStorage.ExistFile(
				ThumbnailNameHelper.Combine("12234456677", ThumbnailSize.ExtraLarge)));
		}
	}
}
