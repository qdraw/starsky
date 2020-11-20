using System;
using System.Collections.Generic;
using System.Linq;
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
	public class SyncRemoveTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly AppSettings _appSettings;
		private readonly IQuery _query;

		public SyncRemoveTest()
		{
			_appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			(_query, _serviceScopeFactory) = CreateNewExampleData();
		}

		private Tuple<IQuery, IServiceScopeFactory> CreateNewExampleData()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			services.AddScoped(p =>_appSettings);
			var query = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/folder_no_content/") {IsDirectory = true},
				new FileIndexItem("/folder_content") {IsDirectory = true},
				new FileIndexItem("/folder_content/test.jpg"),
				new FileIndexItem("/folder_content/test2.jpg")
			});
			services.AddScoped<IQuery, FakeIQuery>(p => query);
			var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			return new Tuple<IQuery, IServiceScopeFactory>(query, serviceScopeFactory);
		}

		[TestMethod]
		public async Task FileNotOnDrive()
		{
			var result= await new SyncRemove(_appSettings, 
				_serviceScopeFactory,_query).Remove("/not_found");
			
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
		}

		[TestMethod]
		public async Task SingleItem_Remove()
		{
			var result= await new SyncRemove(_appSettings, 
				_serviceScopeFactory, _query).Remove("/folder_no_content");
			
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
			Assert.AreEqual("/folder_no_content", result[0].FilePath);
		}

		[TestMethod]
		public async Task Folder_With_ChildItems()
		{
			var (query, serviceScopeFactory) = CreateNewExampleData();
			
			var result= await new SyncRemove(_appSettings, 
				serviceScopeFactory, query).Remove("/folder_content");
			
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[1].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[2].Status);
			Assert.AreEqual("/folder_content", result[0].FilePath);
			Assert.AreEqual("/folder_content/test.jpg", result[1].FilePath);
			Assert.AreEqual("/folder_content/test2.jpg", result[2].FilePath);
		}
	}
}
