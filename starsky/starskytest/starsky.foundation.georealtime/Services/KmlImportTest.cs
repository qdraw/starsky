using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.georealtime.Helpers;
using starsky.foundation.georealtime.Models;
using starsky.foundation.georealtime.Services;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn.CreateAnKml;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.georealtime.Services;

[TestClass]
public class KmlImportTest
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public KmlImportTest()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IStorage, StorageHostFullPathFilesystem>();
		services.AddSingleton<ISelectorStorage, SelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory =
			serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task Test()
	{
		var createAnKml = new CreateAnKml().Bytes;
		var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			{"https://share.garmin.com/Feed/Share/test",  new StringContent(Encoding.UTF8.GetString(createAnKml))},
		});
		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory, new FakeIWebLogger(), 
			new AppSettings{ AllowedHttpsDomains = new List<string>{"share.garmin.com"}});
		
		await new KmlImport(httpClientHelper, new FakeSelectorStorage()).Import("https://share.garmin.com/Feed/Share/test");

		Console.WriteLine();
	}


}
