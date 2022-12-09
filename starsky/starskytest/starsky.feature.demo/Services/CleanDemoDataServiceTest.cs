using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.demo.Services;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.SyncInterfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.demo.Services;

[TestClass]
public class CleanDemoDataServiceTest
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly AppSettings _appSettings;
	private readonly FakeIHttpProvider _fakeProvider;
	private FakeIWebSocketConnectionsService _fakeIWebSocketConnectionsService;
	private readonly FakeIWebLogger _logger;
	private readonly IStorage _storage;

	public CleanDemoDataServiceTest()
	{
		var services = new ServiceCollection();
		services.AddSingleton<AppSettings>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		services.AddSingleton<ISynchronize, FakeISynchronize>();
		services.AddSingleton<IHttpClientHelper, HttpClientHelper>();
		services.AddSingleton<IHttpProvider, FakeIHttpProvider>();
		services.AddSingleton<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();
		services.AddSingleton<INotificationQuery, FakeINotificationQuery>();

		var serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		_appSettings = serviceProvider.GetRequiredService<AppSettings>();
		_fakeProvider = serviceProvider.GetRequiredService<IHttpProvider>() as FakeIHttpProvider;
		_fakeIWebSocketConnectionsService = serviceProvider.GetRequiredService<IWebSocketConnectionsService>() as FakeIWebSocketConnectionsService;
		_logger = serviceProvider.GetRequiredService<IWebLogger>() as FakeIWebLogger;
		_storage = serviceProvider.GetRequiredService<ISelectorStorage>().Get(SelectorStorage.StorageServices.SubPath) as FakeIStorage;
	}
	
	[TestMethod]
	[Timeout(300)]
	public void ExecuteAsync_StartAsync_Test()
	{
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);
		var service = new CleanDemoDataService(_serviceScopeFactory);
			
		CancellationTokenSource source = new CancellationTokenSource();
		CancellationToken token = source.Token;
		source.Cancel(); // <- cancel before start

		MethodInfo dynMethod = service.GetType().GetMethod("ExecuteAsync", 
			BindingFlags.NonPublic | BindingFlags.Instance);
		if ( dynMethod == null )
			throw new Exception("missing ExecuteAsync");
		dynMethod.Invoke(service, new object[]
		{
			token
		});
			
		Assert.IsTrue(!_logger.TrackedExceptions.Any());
	}
	
	[TestMethod]
	public async Task RunAsync_MissingEnvVariable()
	{
		_appSettings.DemoUnsafeDeleteStorageFolder = true;
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

		var result = await new CleanDemoDataService(_serviceScopeFactory).RunAsync();
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public async Task RunAsync_NotInDemoMode()
	{
		_appSettings.DemoUnsafeDeleteStorageFolder = false;
		Environment.SetEnvironmentVariable("app__storageFolder", "/tmp");

		var result = await new CleanDemoDataService(_serviceScopeFactory).RunAsync();
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);
		
		Assert.IsFalse(result);
	}
	
	[TestMethod]
	public async Task RunAsync_NotInWebAppButDemo()
	{
		_appSettings.DemoUnsafeDeleteStorageFolder = true;
		_appSettings.ApplicationType = AppSettings.StarskyAppType.WebHtml;
		Environment.SetEnvironmentVariable("app__storageFolder", "/tmp");

		var result = await new CleanDemoDataService(_serviceScopeFactory).RunAsync();
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

		Assert.IsFalse(result);
	}
		
	[TestMethod]
	public async Task RunAsync_ShouldTrigger()
	{
		_appSettings.DemoUnsafeDeleteStorageFolder = true;
		_appSettings.ApplicationType = AppSettings.StarskyAppType.WebController;

		_appSettings.DemoData = new List<AppSettingsKeyValue>
		{
			new()
			{
				Key = "https://qdraw.nl",
				Value = "1"
			}
		};
		
		Environment.SetEnvironmentVariable("app__storageFolder", "/tmp");
		var result = await new CleanDemoDataService(_serviceScopeFactory).RunAsync();
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

		Assert.IsTrue(result);
		Assert.AreEqual(_fakeProvider.UrlCalled.FirstOrDefault(),_appSettings.DemoData.FirstOrDefault()!.Key);
	}

	[TestMethod]
	public void CleanData_Ignore()
	{
		var storage = new FakeIStorage(new List<string> {  "/", "/.stfolder" });
		CleanDemoDataService.CleanData(storage, new FakeIWebLogger());
		
		Assert.IsTrue(storage.ExistFolder("/.stfolder"));
	}
	
	[TestMethod]
	public void CleanData_Remove_Folder()
	{
		var storage = new FakeIStorage(new List<string> { "/",  "/test" });
		CleanDemoDataService.CleanData(storage, new FakeIWebLogger());
		
		Assert.IsFalse(storage.ExistFolder("/test"));
	}
	
	[TestMethod]
	public void CleanData_Remove_File()
	{
		var storage = new FakeIStorage(new List<string> { "/",  "/test", "/t2" }, new List<string>
		{
			"/test.jpg"
		});
		
		CleanDemoDataService.CleanData(storage, new FakeIWebLogger());
		
		Assert.IsFalse(storage.ExistFile("/test.jpg"));
	}
	
	[TestMethod]
	public void CleanData_Remove_File_KeepGitIgnore()
	{
		var storage = new FakeIStorage(new List<string> { "/",  "/test", "/t2" }, new List<string>
		{
			"/.gitignore",
			"/.gitkeep"
		});
		
		CleanDemoDataService.CleanData(storage, new FakeIWebLogger());
		
		Assert.IsTrue(storage.ExistFile("/.gitignore"));
		Assert.IsTrue(storage.ExistFile("/.gitkeep"));
	}
	
	[TestMethod]
	public void CleanData_Remove_Nothing()
	{
		var storage = new FakeIStorage();
		
		CleanDemoDataService.CleanData(storage, new FakeIWebLogger());
		
		Assert.IsFalse(storage.ExistFile("/test.jpg"));
	}
	
	[TestMethod]
	public async Task PushToSockets_Nothing()
	{
		var updatedList = new List<FileIndexItem>();
		var result = await new CleanDemoDataService(_serviceScopeFactory).PushToSockets(updatedList);
		
		Assert.IsFalse(result);
	}
	
		
	[TestMethod]
	public async Task PushToSockets_PushData()
	{
		_fakeIWebSocketConnectionsService.FakeSendToAllAsync =
			new List<string>();
		var updatedList = new List<FileIndexItem>
		{
			new FileIndexItem("/test.jpg")
			{
				Status = FileIndexItem.ExifStatus.Ok
			}
		};
		var result = await new CleanDemoDataService(_serviceScopeFactory).PushToSockets(updatedList);
		
		Assert.IsTrue(result);

		Assert.AreEqual(1,_fakeIWebSocketConnectionsService.FakeSendToAllAsync.Count);
		Assert.IsTrue(_fakeIWebSocketConnectionsService.FakeSendToAllAsync.FirstOrDefault()!.Contains("/test.jpg"));
	}
	
	
	[TestMethod]
	public async Task DownloadAsync_AppSettingsMissing()
	{
		var appSettings = new AppSettings();
		var fakeIHttpClientHelper =
			new FakeIHttpProvider(new Dictionary<string, HttpContent>());
		var httpClientHelper = new HttpClientHelper(fakeIHttpClientHelper, _serviceScopeFactory, _logger);
		var storage = new FakeIStorage();
		
		var result = await CleanDemoDataService.DownloadAsync(appSettings, httpClientHelper, storage, storage, _logger);
		Assert.IsFalse(result);
	}
	
	[TestMethod]
	public async Task DownloadAsync_IsDownloading()
	{
		var appSettings = new AppSettings{DemoData = new List<AppSettingsKeyValue>
		{
			new AppSettingsKeyValue{Key = "https://qdraw.nl/_settings.json", Value = "1"}
		}};

		var content = "{" +
		              "\"Copy\": {" +
		              "\"1000/20211117_091926_dsc00514_e_kl1k.jpg\": true" +
		              "}" +
		              "}";
		var fakeIHttpClientHelper = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			{"https://qdraw.nl/_settings.json",new StringContent(content)},
			{"https://qdraw.nl/1000/20211117_091926_dsc00514_e_kl1k.jpg",new StringContent("test")}
		});

		var httpClientHelper = new HttpClientHelper(fakeIHttpClientHelper, _serviceScopeFactory, _logger);
		
		var result = await CleanDemoDataService.DownloadAsync(appSettings, httpClientHelper, _storage, _storage, _logger);
		
		Assert.IsTrue(result);
		
		var c1 = fakeIHttpClientHelper.UrlCalled.Count(p => p == "https://qdraw.nl/_settings.json");
		Assert.AreEqual(1,c1);
		
		var c1A = fakeIHttpClientHelper.UrlCalled.Count(p => p == "https://qdraw.nl/1000/20211117_091926_dsc00514_e_kl1k.jpg");
		Assert.AreEqual(1,c1A);
	}
	
	[TestMethod]
	public async Task SeedCli_IsDownloading()
	{
		var appSettings = new AppSettings{DemoData = new List<AppSettingsKeyValue>
		{
			new AppSettingsKeyValue{Key = "https://qdraw.nl/_settings.json", Value = "1"}
		}};

		var content = "{" +
		              "\"Copy\": {" +
		              "\"1000/20211117_091926_dsc00514_e_kl1k.jpg\": true" +
		              "}" +
		              "}";
		var fakeIHttpClientHelper = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			{"https://qdraw.nl/_settings.json",new StringContent(content)},
			{"https://qdraw.nl/1000/20211117_091926_dsc00514_e_kl1k.jpg",new StringContent("test")}
		});

		var httpClientHelper = new HttpClientHelper(fakeIHttpClientHelper, _serviceScopeFactory, _logger);
		
		await CleanDemoDataService.SeedCli(appSettings, httpClientHelper, _storage, _storage, _logger, new FakeISynchronize());
		
		var c1 = fakeIHttpClientHelper.UrlCalled.Count(p => p == "https://qdraw.nl/_settings.json");
		Assert.AreEqual(1,c1);
		
		var c1A = fakeIHttpClientHelper.UrlCalled.Count(p => p == "https://qdraw.nl/1000/20211117_091926_dsc00514_e_kl1k.jpg");
		Assert.AreEqual(1,c1A);
	}

		
	[TestMethod]
	public void Deserialize_ParsingFailed()
	{
		var result = CleanDemoDataService.Deserialize(string.Empty, new FakeIWebLogger(), new FakeIStorage(), string.Empty);
		
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public void Deserialize_Success()
	{
		var input = "{" +
		            "\"Copy\": {" +
		            "\"1000/20211117_091926_dsc00514_e_kl1k.jpg\": true" +
		            "}" +
		            "}";
		var result = CleanDemoDataService.Deserialize(input, new FakeIWebLogger(), new FakeIStorage(), string.Empty);
		
		Assert.AreEqual("1000/20211117_091926_dsc00514_e_kl1k.jpg", result!.Copy.FirstOrDefault().Key);
	}
	
}
