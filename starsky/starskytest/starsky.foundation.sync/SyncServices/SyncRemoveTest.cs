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
			(_query, _serviceScopeFactory) = CreateNewExampleData(null);
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
			var result= await new SyncRemove(_appSettings,_query).Remove("/not_found");
			
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
		}

		[TestMethod]
		public async Task SingleItem_Remove()
		{
			var result= await new SyncRemove(_appSettings, 
				_query).Remove("/folder_no_content");
			
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
			Assert.AreEqual("/folder_no_content", result[0].FilePath);
		}

	}
}
