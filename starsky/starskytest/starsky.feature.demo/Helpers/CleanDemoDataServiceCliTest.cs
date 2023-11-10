using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.demo.Helpers;
using starsky.feature.demo.Services;
using starsky.foundation.database.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.SyncInterfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.demo.Helpers;

[TestClass]
public class CleanDemoDataServiceCliTest
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly AppSettings _appSettings;
	private readonly FakeIHttpProvider _fakeProvider;
	private FakeIWebSocketConnectionsService _fakeIWebSocketConnectionsService;
	private readonly FakeIWebLogger _logger;
	private readonly ISelectorStorage _selectorStorage;
	private readonly IConsole _console;

	public CleanDemoDataServiceCliTest()
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
		services.AddSingleton<IConsole, FakeConsoleWrapper>();

		var serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		_appSettings = serviceProvider.GetRequiredService<AppSettings>();
		_fakeProvider = serviceProvider.GetRequiredService<IHttpProvider>() as FakeIHttpProvider;
		_fakeIWebSocketConnectionsService = serviceProvider.GetRequiredService<IWebSocketConnectionsService>() as FakeIWebSocketConnectionsService;
		_logger = serviceProvider.GetRequiredService<IWebLogger>() as FakeIWebLogger;
		_selectorStorage =
			serviceProvider.GetRequiredService<ISelectorStorage>();
		_console = serviceProvider.GetRequiredService<IConsole>();
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

		var httpClientHelper = new HttpClientHelper(fakeIHttpClientHelper, _serviceScopeFactory, _logger, 
			new AppSettings{ AllowedHttpsDomains = new List<string>{"qdraw.nl"}});

		var service = new CleanDemoDataServiceCli(appSettings, httpClientHelper,
			_selectorStorage, _logger, _console, new FakeISynchronize());
		
		await service.SeedCli(System.Array.Empty<string>());
		
		var c1 = fakeIHttpClientHelper.UrlCalled.Count(p => p == "https://qdraw.nl/_settings.json");
		Assert.AreEqual(1,c1);
		
		var c1A = fakeIHttpClientHelper.UrlCalled.Count(p => p == "https://qdraw.nl/1000/20211117_091926_dsc00514_e_kl1k.jpg");
		Assert.AreEqual(1,c1A);
	}

	[TestMethod]
	public async Task SeedCli_Help()
	{
		var fakeIHttpClientHelper = new FakeIHttpProvider();
		
		var httpClientHelper = new HttpClientHelper(fakeIHttpClientHelper, _serviceScopeFactory, _logger, 
			new AppSettings{ AllowedHttpsDomains = new List<string>{"qdraw.nl"}});

		var service = new CleanDemoDataServiceCli(_appSettings, httpClientHelper,
			_selectorStorage, _logger, _console, new FakeISynchronize());
		
		await service.SeedCli(new string[]{"-h"});

		var fakeConsoleWrapper = _console as FakeConsoleWrapper;
		var isTrue = fakeConsoleWrapper?.WrittenLines.FirstOrDefault(p => p.Contains("--help")) != null;
		Assert.IsTrue(isTrue);
	}
	
	[TestMethod]
	public async Task SeedCli_HelpVerbose()
	{
		var fakeIHttpClientHelper = new FakeIHttpProvider();
		
		var httpClientHelper = new HttpClientHelper(fakeIHttpClientHelper, _serviceScopeFactory, _logger, 
			new AppSettings{ AllowedHttpsDomains = new List<string>{"qdraw.nl"}});

		var appSettings = new AppSettings{Verbose = false};
		var service = new CleanDemoDataServiceCli(appSettings, httpClientHelper,
			_selectorStorage, _logger, _console, new FakeISynchronize());

		await service.SeedCli(new string[]{"-h", "-v"});

		Assert.IsTrue(appSettings.Verbose);
	}
}
