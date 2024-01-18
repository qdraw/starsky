using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class PortProgramHelperTest
{
	private readonly string _prePort;
	private readonly string _preAspNetUrls;

	public PortProgramHelperTest()
	{
		_prePort = Environment.GetEnvironmentVariable("PORT");
		_preAspNetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
	}
	
	[TestMethod]
	public void SetEnvPortAspNetUrls_ShouldSet()
	{
		Environment.SetEnvironmentVariable("PORT","8000");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");

		PortProgramHelper.SetEnvPortAspNetUrls(new List<string>());

		Assert.AreEqual("http://*:8000",Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}
	
	[TestMethod]
	public async Task SetEnvPortAspNetUrlsAndSetDefault_ShouldSet()
	{
		Environment.SetEnvironmentVariable("PORT","8000");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");
	
		await PortProgramHelper.SetEnvPortAspNetUrlsAndSetDefault(Array.Empty<string>(),string.Empty);
		Assert.AreEqual("http://*:8000",Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}
	
	[TestMethod]
	public void SetEnvPortAspNetUrls_ShouldIgnore()
	{
		Environment.SetEnvironmentVariable("PORT","");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");

		PortProgramHelper.SetEnvPortAspNetUrls(new List<string>());
		Assert.AreEqual(null,Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}

	[TestMethod]
	public async Task SetEnvPortAspNetUrlsAndSetDefault_ShouldIgnore_DueAppSettingsFile1()
	{
		Environment.SetEnvironmentVariable("PORT","");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");
		
		var appSettingsPath = Path.Combine(new AppSettings().BaseDirectoryProject,"appsettings-222.json");
		var stream = StringToStreamHelper.StringToStream("{     \"Kestrel\": {\n        \"Endpoints\": {\n          " +
			"  \"Https\": {\n                \"Url\": \"https://*:8001\"\n            },\n            \"Http\": {\n      " +
			"          \"Url\": \"http://*:8000\"\n            }\n        }\n    }\n }");
		await new StorageHostFullPathFilesystem().WriteStreamAsync(stream,appSettingsPath);
		
		await PortProgramHelper.SetEnvPortAspNetUrlsAndSetDefault(Array.Empty<string>(),appSettingsPath);

		Assert.AreEqual(null,Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
		
		// remove afterwards
		new StorageHostFullPathFilesystem().FileDelete(appSettingsPath);
	}
	
	
	[TestMethod]
	public async Task SkipForAppSettingsJsonFile_ShouldIgnore_DueAppSettingsFile()
	{
		Environment.SetEnvironmentVariable("PORT","");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");
		
		var appSettingsPath = Path.Combine(new AppSettings().BaseDirectoryProject,"appsettings-111.json");
		var stream = StringToStreamHelper.StringToStream("{     \"Kestrel\": {\n        \"Endpoints\": {\n          " +
			"  \"Https\": {\n                \"Url\": \"https://*:8001\"\n            },\n            \"Http\": {\n      " +
			"          \"Url\": \"http://*:8000\"\n            }\n        }\n    }\n }");
		await new StorageHostFullPathFilesystem().WriteStreamAsync(stream,appSettingsPath);
		
		var result = await PortProgramHelper.SkipForAppSettingsJsonFile(appSettingsPath);

		Assert.AreEqual(true,result);
		Assert.AreEqual(null,Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
		
		// remove afterwards
		new StorageHostFullPathFilesystem().FileDelete(appSettingsPath);
	}
	
	[TestMethod]
	public async Task SkipForAppSettingsJsonFile_ShouldIgnore_DueAppSettingsFile2()
	{
		Environment.SetEnvironmentVariable("PORT","");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");
		
		var appSettingsPath = Path.Combine(new AppSettings().BaseDirectoryProject,"appsettings-333.json");
		var stream = StringToStreamHelper.StringToStream("{     \"Kestrel\": {\n        \"Endpoints\": {\n          " +
			"  \"Https\": {\n                \"Url\": \"https://*:8001\"\n            }\n           " +
			"\n        }\n    }\n }");
		await new StorageHostFullPathFilesystem().WriteStreamAsync(stream,appSettingsPath);
		
		var result = await PortProgramHelper.SkipForAppSettingsJsonFile(appSettingsPath);

		Assert.AreEqual(true,result);
		Assert.AreEqual(null,Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
		
		// remove afterwards
		new StorageHostFullPathFilesystem().FileDelete(appSettingsPath);
	}
	
		
	[TestMethod]
	public async Task SkipForAppSettingsJsonFile_ShouldFalse()
	{
		var result = await PortProgramHelper.SkipForAppSettingsJsonFile(string.Empty);
		Assert.AreEqual(false,result);
	}
		
	[TestMethod]
	public void SetDefaultAspNetCoreUrls_ShouldSet()
	{
		Environment.SetEnvironmentVariable("PORT","");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");
	
		PortProgramHelper.SetDefaultAspNetCoreUrls(Array.Empty<string>());
		
		// should set to default
		Assert.AreEqual("http://localhost:4000;https://localhost:4001",
			Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}
	
	[TestMethod]
	public void SetDefaultAspNetCoreUrls_ShouldIgnore()
	{
		Environment.SetEnvironmentVariable("PORT","");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","http://localhost:4000");
	
		PortProgramHelper.SetDefaultAspNetCoreUrls(Array.Empty<string>());
		
		// should set port to 4000
		Assert.AreEqual("http://localhost:4000",Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}
}
