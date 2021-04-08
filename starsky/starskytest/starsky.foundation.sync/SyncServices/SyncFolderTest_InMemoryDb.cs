using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public class SyncFolderTestInMemoryDb
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;

		public SyncFolderTestInMemoryDb()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			_appSettings = new AppSettings{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase, 
				Verbose = true
			};

			provider.AddSingleton(_appSettings);

			new SetupDatabaseTypes(_appSettings, provider).BuilderDb();
			provider.AddScoped<IQuery,Query>();
			
			var serviceProvider = provider.BuildServiceProvider();
			
			_query = serviceProvider.GetRequiredService<IQuery>();
		}
			
		[TestMethod]
		public async Task Folder_FilesOnDiskButNotInTheDb()
		{
			var storage =  new FakeIStorage(
				new List<string>
				{
					"/", 
					"/Folder_FilesOnDiskButNotInTheDb"
				}, 
				new List<string>
				{
					"/Folder_FilesOnDiskButNotInTheDb/test1.jpg",
					"/Folder_FilesOnDiskButNotInTheDb/test2.jpg",
					"/Folder_FilesOnDiskButNotInTheDb/test3.jpg",
				},
				new List<byte[]>
				{
					CreateAnImage.Bytes,
					CreateAnImageColorClass.Bytes,
					CreateAnImageNoExif.Bytes,
				});
			
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.Folder("/Folder_FilesOnDiskButNotInTheDb");
			
			Assert.AreEqual("/Folder_FilesOnDiskButNotInTheDb/test1.jpg",result[0].FilePath);
			Assert.AreEqual("/Folder_FilesOnDiskButNotInTheDb/test2.jpg",result[1].FilePath);
			Assert.AreEqual("/Folder_FilesOnDiskButNotInTheDb/test3.jpg",result[2].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[1].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[2].Status);

			var files = await _query.GetAllFilesAsync("/Folder_FilesOnDiskButNotInTheDb");

			Assert.AreEqual(3,files.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, files[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, files[1].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, files[2].Status);
		}
		
		[TestMethod]
		public async Task Folder_InDbButNotOnDisk()
		{
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk/test.jpg"));
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk/test2.jpg"));

			var storage = new FakeIStorage();
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.Folder("/Folder_InDbButNotOnDisk");
			
			Assert.AreEqual("/Folder_InDbButNotOnDisk/test.jpg",result[0].FilePath);
			Assert.AreEqual("/Folder_InDbButNotOnDisk/test2.jpg",result[1].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[1].Status);
			
			Assert.AreEqual(null, 
				_query.SingleItem("/Folder_InDbButNotOnDisk/test.jpg"));
			Assert.AreEqual(null, 
				_query.SingleItem("/Folder_InDbButNotOnDisk/test2.jpg"));
		}
		
		[TestMethod]
		public async Task Folder_InDbButNotOnDisk_Floating_directories()
		{
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk2/test.jpg"));
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk2/test_dir"){IsDirectory = true});
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk2/test_dir/test.jpg"));

			var storage = new FakeIStorage();
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.Folder("/Folder_InDbButNotOnDisk2");

			
			Assert.AreEqual("/Folder_InDbButNotOnDisk2/test.jpg", 
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk2/test.jpg").FilePath);
			Assert.AreEqual("/Folder_InDbButNotOnDisk2/test_dir",
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk2/test_dir").FilePath);
			Assert.AreEqual("/Folder_InDbButNotOnDisk2/test_dir/test.jpg",
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk2").FilePath);

			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[1].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[2].Status);
			
			var data = await _query.GetAllRecursiveAsync("/Folder_InDbButNotOnDisk2");
			Assert.AreEqual(0, data.Count);
			
			// Check for database
			Assert.AreEqual(null, 
				await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk2"));
			Assert.AreEqual(null, 
				await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk2/test.jpg"));
			Assert.AreEqual(null, 
				await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk2/test_dir/test.jpg"));
			Assert.AreEqual(null, 
				await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk2/test_dir"));
		}
	}
}
