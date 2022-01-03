using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public class SingleFileIntegrationTest
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly CreateAnImage _createAnImage;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IMemoryCache _memoryCache;

		public SingleFileIntegrationTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			_createAnImage = new CreateAnImage();
			_appSettings = new AppSettings{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase, 
				StorageFolder = _createAnImage.BasePath,
				Verbose = true
			};

			provider.AddSingleton(_appSettings);

			new SetupDatabaseTypes(_appSettings, provider).BuilderDb();
			provider.AddScoped<IQuery,Query>();
			provider.AddScoped<IWebLogger, FakeIWebLogger>();
			
			var serviceProvider = provider.BuildServiceProvider();
			
			_iStorage = new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger());
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			_memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

			_query = serviceProvider.GetRequiredService<IQuery>();
		}
		
		// [TestMethod]
		// public async Task NewItem()
		// {
		// 	var stopWatch = new Stopwatch();
		// 	stopWatch.Start();
		//
		// 	var sync = new Synchronize(_appSettings, _query, new FakeSelectorStorage(_iStorage), new FakeIWebLogger(),null);
		// 	var result = await sync.Sync(_createAnImage.DbPath);
		//
		// 	stopWatch.Stop();
		// 	var ts = stopWatch.Elapsed;
		// 	// Format and display the TimeSpan value.
		// 	var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
		// 	Console.WriteLine("RunTime " + elapsedTime);
		// 	Assert.IsNotNull(result);
		// }
		
		[TestMethod]
		public async Task ExistingItem()
		{
			await _query.AddItemAsync(new FileIndexItem(_createAnImage.DbPath)
			{
				Size = 9998,
				FileHash = "INKV4BSQ54PIAIS5XUFAKBUW5Y"
			});

				
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			
			var sync = new Synchronize(_appSettings, _query, new FakeSelectorStorage(_iStorage), new FakeIWebLogger(),_memoryCache);
			var result = await sync.Sync("/");
			
			stopWatch.Stop();
			var ts = stopWatch.Elapsed;
			var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
			Console.WriteLine("RunTime " + elapsedTime);
			
			Assert.IsNotNull(result);
		}
	}
}
