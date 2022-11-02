using System;
using System.Collections.Generic;
using System.Linq;
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
	}
	
	[TestMethod]
	public async Task RunAsync_MissingEnvVariable()
	{
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
	public void CleanData_Remove()
	{
		var storage = new FakeIStorage(new List<string> { "/",  "/test" });
		CleanDemoDataService.CleanData(storage, new FakeIWebLogger());
		
		Assert.IsFalse(storage.ExistFolder("/test"));
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
}
