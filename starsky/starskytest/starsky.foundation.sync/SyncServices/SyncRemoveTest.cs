using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public sealed class SyncRemoveTest
	{
		private readonly AppSettings _appSettings;
		private readonly IQuery _query;

		public SyncRemoveTest()
		{
			_appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			(_query, _) = CreateNewExampleData(null);
		}

		private Tuple<IQuery, IServiceScopeFactory> CreateNewExampleData(List<FileIndexItem> content)
		{
			// ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
			if ( content == null )
			{
				content = new List<FileIndexItem>
				{
					new FileIndexItem("/folder_no_content/") {IsDirectory = true},
					new FileIndexItem("/folder_content") {IsDirectory = true},
					new FileIndexItem("/folder_content/test.jpg"),
					new FileIndexItem("/folder_content/test2.jpg")
				};
			}
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			services.AddScoped(p =>_appSettings);
			var query = new FakeIQuery(content);
			services.AddScoped<IQuery, FakeIQuery>(p => query);
			var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			return new Tuple<IQuery, IServiceScopeFactory>(query, serviceScopeFactory);
		}

		[TestMethod]
		public async Task FileNotOnDrive()
		{
			var remove = new SyncRemove(_appSettings, _query, null, null);
			var result= await remove.RemoveAsync("/not_found");
			
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
		}
		
		[TestMethod]
		public async Task FileNotOnDrive_Object()
		{
			var remove = new SyncRemove(_appSettings, _query, null, null);
			await _query.AddItemAsync(new FileIndexItem("/FileNotOnDrive_Object.jpg"));
			var item = await 
				_query.GetObjectByFilePathAsync("/FileNotOnDrive_Object.jpg");
			Assert.IsNotNull(item);

			item.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
			var result= await remove.RemoveAsync(new List<FileIndexItem>{item});
			
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result[0].Status);

			var allRecursive = await _query.GetAllRecursiveAsync();
			var item2 = allRecursive.FirstOrDefault(p =>
				p.FilePath == "/FileNotOnDrive_Object.jpg" && p.Status != FileIndexItem.ExifStatus.NotFoundSourceMissing);

			Assert.IsNull(item2);
		}
		
		[TestMethod]
		public async Task FileNotOnDrive_Object_Ignore_wrongStatus()
		{
			var remove = new SyncRemove(_appSettings, _query, null, null);
			await _query.AddItemAsync(new FileIndexItem("/FileNotOnDrive_Object_Ignore_wrongStatus.jpg"));
			var item = await 
				_query.GetObjectByFilePathAsync("/FileNotOnDrive_Object_Ignore_wrongStatus.jpg");
			Assert.IsNotNull(item);
			
			item.Status = FileIndexItem.ExifStatus.Ok;
			var result= await remove.RemoveAsync(new List<FileIndexItem>{item});
			
			Assert.AreEqual(0, result.Count);

			var allRecursive = await _query.GetAllRecursiveAsync();
			var queryResult = allRecursive.FirstOrDefault(p =>
				p.FilePath == "/FileNotOnDrive_Object_Ignore_wrongStatus.jpg");

			Assert.AreEqual(item,queryResult);
		}

		[TestMethod]
		public async Task SingleItem_Folder_Remove()
		{
			var remove = new SyncRemove(_appSettings, _query, null, null);
			var result= await remove.RemoveAsync("/folder_no_content");
			
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
			Assert.AreEqual("/folder_no_content", result[0].FilePath);
			
			var getResult = await _query.GetObjectByFilePathAsync("/folder_no_content");
			Assert.IsNull(getResult);
		}

		
		[TestMethod]
		public async Task SingleFile_RemoveSidecarFile()
		{
			var queryContent = new List<FileIndexItem>
			{
				new FileIndexItem("/sidecar_test__1") {IsDirectory = true},
				new FileIndexItem("/sidecar_test__1/test.dng")
				{
					SidecarExtensions = "xmp"
				},
				new FileIndexItem("/sidecar_test__1/test.xmp"),
				new FileIndexItem("/sidecar_test__2") {IsDirectory = true},
				new FileIndexItem("/sidecar_test__2/test.dng")
				{
					SidecarExtensions = "xmp"
				},
				new FileIndexItem("/sidecar_test__2/test.xmp")
			};
			var query = new FakeIQuery(queryContent);
			var remove = new SyncRemove(_appSettings, query, null, null);

			var result= await remove.RemoveAsync(new List<string>{
				"/sidecar_test__1/test.xmp",
				"/sidecar_test__2/test.xmp"
			});

			Assert.AreEqual(2, result.Count);
			
			var item = await query.GetObjectByFilePathAsync("/sidecar_test__1/test.dng");
			Assert.IsNotNull(item);

			Console.WriteLine(JsonSerializer.Serialize(item.SidecarExtensionsList));

			// add retry 
			if (item.SidecarExtensionsList.Count >= 1)
			{
				item = await query.GetObjectByFilePathAsync("/sidecar_test__1/test.dng");
			}
			
			Assert.AreEqual(0, item.SidecarExtensionsList.Count);
			
			var item2 = await query.GetObjectByFilePathAsync("/sidecar_test__2/test.dng");
			Assert.AreEqual(0, item2?.SidecarExtensionsList.Count);
		}
		
	}
}
