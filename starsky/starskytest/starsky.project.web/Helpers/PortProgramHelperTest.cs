using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.project.web.Helpers;
using starskytest.FakeMocks;
using starskytest.starsky.project.web.Helpers.Models;

namespace starskytest.starsky.project.web.Helpers;

[TestClass]
public class PortProgramHelperTest
{
	private readonly AppSettingsExampleModel.KestrelGlobalConfig _kestrelConfig = new()
	{
		Kestrel = new AppSettingsExampleModel.KestrelConfig
		{
			Endpoints = new AppSettingsExampleModel.ExampleEndpoints
			{
				Http = new AppSettingsExampleModel.EndpointObject
				{
					Url = "http://*:8000"
				},
				Https = new AppSettingsExampleModel.EndpointObject
				{
					Url = "https://*:8001"
				}
			}
		}
	};

	private readonly string? _preAspNetUrls;
	private readonly string? _prePort;

	public PortProgramHelperTest()
	{
		_prePort = Environment.GetEnvironmentVariable("PORT");
		_preAspNetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
	}

	[TestMethod]
	public void SetEnvPortAspNetUrls_ShouldSet()
	{
		Environment.SetEnvironmentVariable("PORT", "8000");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "");

		PortProgramHelper.SetEnvPortAspNetUrls(new List<string>());

		Assert.AreEqual("http://*:8000", Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

		Environment.SetEnvironmentVariable("PORT", _prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", _preAspNetUrls);
	}

	[TestMethod]
	public async Task SetEnvPortAspNetUrlsAndSetDefault_ShouldSet()
	{
		Environment.SetEnvironmentVariable("PORT", "8000");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "");

		await PortProgramHelper.SetEnvPortAspNetUrlsAndSetDefault(Array.Empty<string>(),
			string.Empty);
		Assert.AreEqual("http://*:8000", Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

		Environment.SetEnvironmentVariable("PORT", _prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", _preAspNetUrls);
	}

	[TestMethod]
	public void SetEnvPortAspNetUrls_ShouldIgnore()
	{
		Environment.SetEnvironmentVariable("PORT", "");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "");

		PortProgramHelper.SetEnvPortAspNetUrls(new List<string>());
		Assert.AreEqual(null, Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

		Environment.SetEnvironmentVariable("PORT", _prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", _preAspNetUrls);
	}

	[TestMethod]
	public async Task SetEnvPortAspNetUrlsAndSetDefault_ShouldIgnore_DueAppSettingsFile1()
	{
		Environment.SetEnvironmentVariable("PORT", "");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "");

		var appSettingsPath =
			Path.Combine(new AppSettings().BaseDirectoryProject, "appsettings-222.json");

		var stream = StringToStreamHelper.StringToStream(
			JsonSerializer.Serialize(_kestrelConfig));

		await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(stream,
			appSettingsPath);

		var result = await PortProgramHelper.SetEnvPortAspNetUrlsAndSetDefault([],
			appSettingsPath);

		Assert.IsTrue(result);

		Environment.SetEnvironmentVariable("PORT", _prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", _preAspNetUrls);

		// remove afterwards
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FileDelete(appSettingsPath);
	}


	[TestMethod]
	public async Task SkipForAppSettingsJsonFile_ShouldIgnore_DueAppSettingsFile()
	{
		Environment.SetEnvironmentVariable("PORT", "");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "");

		var appSettingsPath =
			Path.Combine(new AppSettings().BaseDirectoryProject, "appsettings-111.json");

		var stream = StringToStreamHelper.StringToStream(
			JsonSerializer.Serialize(_kestrelConfig));

		await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(stream,
			appSettingsPath);

		var result = await PortProgramHelper.SkipForAppSettingsJsonFile(appSettingsPath);

		Assert.IsTrue(result);
		Assert.AreEqual(null, Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

		Environment.SetEnvironmentVariable("PORT", _prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", _preAspNetUrls);

		// remove afterwards
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FileDelete(appSettingsPath);
	}

	[TestMethod]
	public async Task SkipForAppSettingsJsonFile_ShouldIgnore_DueAppSettingsFile2()
	{
		Environment.SetEnvironmentVariable("PORT", "");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "");

		var appSettingsPath =
			Path.Combine(new AppSettings().BaseDirectoryProject, "appsettings-333.json");

		var kestrelConfig = new AppSettingsExampleModel.KestrelGlobalConfig
		{
			Kestrel = new AppSettingsExampleModel.KestrelConfig
			{
				Endpoints = new AppSettingsExampleModel.ExampleEndpoints
				{
					Https = new AppSettingsExampleModel.EndpointObject
					{
						Url = "https://*:8001"
					}
				}
			}
		};

		var jsonString = JsonSerializer.Serialize(kestrelConfig);
		var stream = StringToStreamHelper.StringToStream(jsonString);

		await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(stream,
			appSettingsPath);

		var result = await PortProgramHelper.SkipForAppSettingsJsonFile(appSettingsPath);

		Assert.IsTrue(result);

		Environment.SetEnvironmentVariable("PORT", _prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", _preAspNetUrls);

		// remove afterwards
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FileDelete(appSettingsPath);
	}

	[TestMethod]
	public async Task SkipForAppSettingsJsonFile_ShouldFalse()
	{
		var result = await PortProgramHelper.SkipForAppSettingsJsonFile(string.Empty);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void SetDefaultAspNetCoreUrls_ShouldSet()
	{
		Environment.SetEnvironmentVariable("PORT", "");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "");

		PortProgramHelper.SetDefaultAspNetCoreUrls(Array.Empty<string>());

		// should set to default
		Assert.AreEqual("http://localhost:4000;https://localhost:4001",
			Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

		Environment.SetEnvironmentVariable("PORT", _prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", _preAspNetUrls);
	}

	[TestMethod]
	public void SetDefaultAspNetCoreUrls_ShouldIgnore()
	{
		Environment.SetEnvironmentVariable("PORT", "");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:4000");

		PortProgramHelper.SetDefaultAspNetCoreUrls(Array.Empty<string>());

		// should set port to 4000
		Assert.AreEqual("http://localhost:4000",
			Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

		Environment.SetEnvironmentVariable("PORT", _prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", _preAspNetUrls);
	}
}
