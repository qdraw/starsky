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
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	/// <summary>
	/// SyncFolderTest_InMemoryDb.cs
	/// </summary>
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
			provider.AddScoped<IWebLogger,FakeIWebLogger>();

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
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			var result = await syncFolder.Folder("/Folder_FilesOnDiskButNotInTheDb");

			var test1 = result.FirstOrDefault(p =>
				p.FilePath == "/Folder_FilesOnDiskButNotInTheDb/test1.jpg");
			Assert.IsNotNull(test1);
			var test2 = result.FirstOrDefault(p =>
				p.FilePath == "/Folder_FilesOnDiskButNotInTheDb/test2.jpg");
			Assert.IsNotNull(test2);
			var test3 = result.FirstOrDefault(p =>
				p.FilePath == "/Folder_FilesOnDiskButNotInTheDb/test3.jpg");
			Assert.IsNotNull(test3);

			Assert.AreEqual("/Folder_FilesOnDiskButNotInTheDb/test1.jpg",test1!.FilePath);
			Assert.AreEqual("/Folder_FilesOnDiskButNotInTheDb/test2.jpg",test2!.FilePath);
			Assert.AreEqual("/Folder_FilesOnDiskButNotInTheDb/test3.jpg",test3!.FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,test1.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,test2.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,test3.Status);

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
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			var result = await syncFolder.Folder("/Folder_InDbButNotOnDisk");

			var test0 = result.FirstOrDefault(p =>
				p.FilePath == "/Folder_InDbButNotOnDisk/test.jpg");
			var test2 = result.FirstOrDefault(p =>
				p.FilePath == "/Folder_InDbButNotOnDisk/test2.jpg");
			
			Assert.AreEqual("/Folder_InDbButNotOnDisk/test.jpg",test0!.FilePath);
			Assert.AreEqual("/Folder_InDbButNotOnDisk/test2.jpg",test2!.FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,test0.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,test2.Status);
			
			Assert.AreEqual(null, 
				_query.GetObjectByFilePath("/Folder_InDbButNotOnDisk/test.jpg"));
			Assert.AreEqual(null, 
				_query.GetObjectByFilePath("/Folder_InDbButNotOnDisk/test2.jpg"));
		}
		
		[TestMethod]
		public async Task Folder_InDbButNotOnDisk_Floating_directories()
		{
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk2/test.jpg"));
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk2/test_dir"){IsDirectory = true});
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk2/test_dir/test.jpg"));
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk2/test_dir/child"){IsDirectory = true});
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk2/test_dir/child/test.jpg"));

			var storage = new FakeIStorage();
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			var result = await syncFolder.Folder("/Folder_InDbButNotOnDisk2");

			
			Assert.AreEqual("/Folder_InDbButNotOnDisk2/test.jpg", 
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk2/test.jpg")?.FilePath);
			Assert.AreEqual("/Folder_InDbButNotOnDisk2/test_dir",
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk2/test_dir")?.FilePath);
			Assert.AreEqual("/Folder_InDbButNotOnDisk2",
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk2")?.FilePath);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk2/test.jpg")?.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk2/test_dir")?.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk2")?.Status);
			
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
		
		[TestMethod]
		public async Task Folder_InDbButNotOnDisk_Floating_directoriesWithinScanDir()
		{
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk4/test.jpg"));
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk4/test_dir"){IsDirectory = true});
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk4/test_dir/test.jpg"));
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk4/test_dir/child"){IsDirectory = true});
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk4/test_dir/child/test.jpg"));

			var storage = new FakeIStorage(new List<string>{
				"/Folder_InDbButNotOnDisk4", 
				"/Folder_InDbButNotOnDisk4/test_dir"
			});
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			var result = (await syncFolder.Folder("/Folder_InDbButNotOnDisk4"))
				.Where(p => p.FilePath != "/").ToList();
			
			Assert.AreEqual("/Folder_InDbButNotOnDisk4/test.jpg", 
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk4/test.jpg")?.FilePath);
			Assert.AreEqual("/Folder_InDbButNotOnDisk4",
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk4")?.FilePath);

			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk4/test.jpg")?.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk4/test_dir/test.jpg")?.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk4/test_dir/child")?.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
				result.FirstOrDefault(p => p.FilePath == "/Folder_InDbButNotOnDisk4")?.Status);

			// for debugging pipelines
			Console.WriteLine("\n--result---");
			foreach ( var item in result )
			{
				Console.WriteLine("$>~ " + item.FilePath + " ~ " +item.Status);
			}
			
			var data = await _query.GetAllRecursiveAsync("/Folder_InDbButNotOnDisk4");
			
			// for debugging pipelines
			Console.WriteLine("\n--GetAllRecursiveAsync---");
			foreach ( var item in data )
			{
				Console.WriteLine("%>~ " + item.FilePath + " ~ " +item.Status);
			}
			Console.WriteLine("\n");
			
			// Check for database
			Assert.AreEqual("/Folder_InDbButNotOnDisk4", 
				(await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk4")).FilePath);
			Assert.AreEqual("/Folder_InDbButNotOnDisk4/test_dir", 
				(await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk4/test_dir")).FilePath);
			Assert.AreEqual(null, 
				await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk4/test_dir/test.jpg"));
			Assert.AreEqual(null, 
				await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk4/test.jpg"));
		}
	}
}
