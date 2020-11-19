using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public class SynchronizeTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly AppSettings _appSettings;

		public SynchronizeTest()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();
			_appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			services.AddScoped(p =>_appSettings);
			services.AddScoped<IQuery, FakeIQuery>();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task Sync_NotFound()
		{
			var sync = new Synchronize(_appSettings, new FakeIQuery(), new FakeSelectorStorage(), _serviceScopeFactory);
			var result = await sync.Sync("/not_found.jpg");
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
		}
		
		[TestMethod]
		public async Task Sync_File()
		{
			var sync = new Synchronize(new AppSettings(), new FakeIQuery(), 
				new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"}, 
					new List<string>{"/test.jpg"})), _serviceScopeFactory);
			
			var result = await sync.Sync("/test.jpg");

			// is missing actual bytes
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result[0].Status);
		}
				
		[TestMethod]
		public async Task Sync_Folder()
		{
			var sync = new Synchronize(new AppSettings(), new FakeIQuery(), 
				new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"}, 
					new List<string>{"/test.jpg"}, 
					new List<byte[]>
					{
						CreateAnImage.Bytes
					})), _serviceScopeFactory);
			
			var result = await sync.Sync("/");

			// subject to change
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[1].Status);
			Assert.AreEqual("/test.jpg", result[0].FilePath);
			Assert.AreEqual("/", result[1].FilePath);
		}
	}
}
