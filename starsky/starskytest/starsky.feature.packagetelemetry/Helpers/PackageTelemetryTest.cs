using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.packagetelemetry.Services;
using starsky.foundation.database.Models;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.packagetelemetry.Helpers;

[TestClass]
public sealed class PackageTelemetryTest
{
	private readonly IServiceScopeFactory? _nullServiceScopeFactory = null;

	[TestMethod]
	public void GetCurrentOsPlatformTest()
	{
		var content = PackageTelemetry.GetCurrentOsPlatform();
		Assert.IsNotNull(content);

		var allOsPlatforms = new List<OSPlatform>
		{
			OSPlatform.Linux, OSPlatform.Windows, OSPlatform.OSX, OSPlatform.FreeBSD
		};
		Assert.IsTrue(allOsPlatforms.Contains(content.Value));
	}

	[TestMethod]
	public void GetSystemDataTest()
	{
		var httpProvider = new FakeIHttpProvider();
		var appSettings = new AppSettings();
		var httpClientHelper =
			new HttpClientHelper(httpProvider, _nullServiceScopeFactory, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, new AppSettings(),
			new FakeIWebLogger(), new FakeIQuery(), new FakeIDeviceIdService());

		var systemData = packageTelemetry.GetSystemData();
		Assert.IsTrue(systemData.Exists(p => p.Key == "AppVersion"));
		Assert.AreEqual(systemData.Find(p => p.Key == "AppVersion").Value,
			appSettings.AppVersion);
		Assert.IsTrue(systemData.Exists(p => p.Key == "NetVersion"));
		Assert.AreEqual(systemData.Find(p => p.Key == "NetVersion").Value,
			RuntimeInformation.FrameworkDescription);
		Assert.IsTrue(systemData.Exists(p => p.Key == "OSArchitecture"));
		Assert.AreEqual(systemData.Find(p => p.Key == "OSArchitecture").Value,
			RuntimeInformation.OSArchitecture.ToString());
		Assert.IsTrue(systemData.Exists(p => p.Key == "OSVersion"));
		Assert.IsTrue(systemData.Exists(p => p.Key == "OSDescriptionLong"));
		Assert.IsTrue(systemData.Exists(p => p.Key == "OSPlatform"));
		Assert.IsTrue(systemData.Exists(p => p.Key == "DockerContainer"));
		Assert.IsTrue(systemData.Exists(p => p.Key == "CurrentCulture"));
		Assert.IsTrue(systemData.Exists(p => p.Key == "AspNetCoreEnvironment"));
	}

	[TestMethod]
	public void GetSystemDataTestDocker()
	{
		var httpProvider = new FakeIHttpProvider();
		var httpClientHelper =
			new HttpClientHelper(httpProvider, _nullServiceScopeFactory, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, new AppSettings(),
			new FakeIWebLogger(), new FakeIQuery(), new FakeIDeviceIdService());

		var sourceValue = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
		Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", "true");

		var systemData = packageTelemetry.GetSystemData(OSPlatform.Linux);

		Assert.AreEqual("True", systemData.Find(p => p.Key == "DockerContainer").Value);

		// undo
		Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", "false");

		var systemDataFalse = packageTelemetry.GetSystemData(OSPlatform.Linux);

		Assert.AreEqual("False", systemDataFalse.Find(p => p.Key == "DockerContainer").Value);

		Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", sourceValue);
	}

	[TestMethod]
	public void GetSystemDataTestDocker_NonLinux_soFalse()
	{
		var httpProvider = new FakeIHttpProvider();
		var httpClientHelper =
			new HttpClientHelper(httpProvider, _nullServiceScopeFactory, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, new AppSettings(),
			new FakeIWebLogger(), new FakeIQuery(), new FakeIDeviceIdService());

		var sourceValue = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
		Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", "true");

		var systemData = packageTelemetry.GetSystemData(OSPlatform.Windows);

		// so False
		Assert.AreEqual("False", systemData.Find(p => p.Key == "DockerContainer").Value);

		// undo
		Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", "false");

		var systemDataFalse = packageTelemetry.GetSystemData(OSPlatform.Linux);

		Assert.AreEqual("False", systemDataFalse.Find(p => p.Key == "DockerContainer").Value);

		Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", sourceValue);
	}

	[TestMethod]
	public void GetSystemDataTestDocker_WebSiteName()
	{
		var httpProvider = new FakeIHttpProvider();
		var httpClientHelper =
			new HttpClientHelper(httpProvider, _nullServiceScopeFactory, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, new AppSettings(),
			new FakeIWebLogger(), new FakeIQuery(), new FakeIDeviceIdService());

		var sourceValue = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
		Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "test");

		var systemData = packageTelemetry.GetSystemData(OSPlatform.Windows);

		// so False
		Assert.AreEqual("9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08",
			systemData.Find(p => p.Key == "WebsiteName").Value);

		// undo
		Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "");

		var systemDataFalse = packageTelemetry.GetSystemData(OSPlatform.Linux);

		Assert.AreEqual("not set",
			systemDataFalse.Find(p => p.Key == "WebsiteName").Value.ToLowerInvariant());

		Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", sourceValue);
	}

	[TestMethod]
	public void ParseContent_Null()
	{
		var result = PackageTelemetry.ParseContent(null!);
		Assert.AreEqual("null", result);
	}

	[TestMethod]
	public void GetPropValue_Null()
	{
		var result = PackageTelemetry.GetPropValue(null, "test");
		Assert.IsNull(result);
	}

	[TestMethod]
	public void GetPropValue_Null_Null()
	{
		var result = PackageTelemetry.GetPropValue(null, null!);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void GetPropValue_Object_TestClass()
	{
		var result = PackageTelemetry.GetPropValue(new TestClass(), "Test") as bool?;
		Assert.IsTrue(result);
		Assert.IsTrue(new TestClass().Test);
	}

	[TestMethod]
	public void GetPropValue_ReadValue()
	{
		var result =
			PackageTelemetry.GetPropValue(new PropValueTestClass { Test = "1" }, "Test");
		Assert.AreEqual("1", result);
	}

	[TestMethod]
	public void AddAppSettingsData()
	{
		var httpProvider = new FakeIHttpProvider();
		var appSettings = new AppSettings();
		var httpClientHelper =
			new HttpClientHelper(httpProvider, _nullServiceScopeFactory, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings,
			new FakeIWebLogger(), new FakeIQuery(), new FakeIDeviceIdService());
		var result =
			packageTelemetry.AddAppSettingsData(new List<KeyValuePair<string, string>>());

		Assert.IsTrue(result.Exists(p => p.Key == "AppSettingsName"));
	}

	[TestMethod]
	public async Task PackageTelemetrySend_Disabled()
	{
		var httpProvider = new FakeIHttpProvider();
		var appSettings = new AppSettings { EnablePackageTelemetry = false };
		var httpClientHelper =
			new HttpClientHelper(httpProvider, _nullServiceScopeFactory, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings,
			new FakeIWebLogger(), new FakeIQuery(), new FakeIDeviceIdService());
		var result = await packageTelemetry.PackageTelemetrySend();
		Assert.IsNull(result);
	}


	[TestMethod]
	public async Task PackageTelemetrySend_HasSend()
	{
		var httpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			{
				"https://" + PackageTelemetry.PackageTelemetryUrl, new StringContent(string.Empty)
			}
		});
		var appSettings = new AppSettings { EnablePackageTelemetry = true };
		var httpClientHelper =
			new HttpClientHelper(httpProvider, _nullServiceScopeFactory, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings,
			new FakeIWebLogger(), new FakeIQuery(), new FakeIDeviceIdService());
		var result = await packageTelemetry.PackageTelemetrySend();
		Assert.IsTrue(result);
	}


	[TestMethod]
	public async Task PackageTelemetrySend_False_EnablePackageTelemetryDebug()
	{
		var httpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			{
				"https://" + PackageTelemetry.PackageTelemetryUrl, new StringContent(string.Empty)
			}
		});
		var appSettings = new AppSettings
		{
			EnablePackageTelemetry = true, EnablePackageTelemetryDebug = true
		};
		var httpClientHelper =
			new HttpClientHelper(httpProvider, _nullServiceScopeFactory, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings,
			new FakeIWebLogger(), new FakeIQuery(), new FakeIDeviceIdService());
		var result = await packageTelemetry.PackageTelemetrySend();
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task AddDatabaseData()
	{
		var appSettings = new AppSettings { EnablePackageTelemetry = true };
		var packageTelemetry = new PackageTelemetry(null!, appSettings, new FakeIWebLogger(),
			new FakeIQuery(new List<FileIndexItem>
			{
				new("/test.jpg"), new("/test") { IsDirectory = true }
			}), new FakeIDeviceIdService());

		var result =
			await packageTelemetry.AddDatabaseData(new List<KeyValuePair<string, string>>());

		var res1 =
			result.Find(p => p.Key == "FileIndexItemTotalCount");
		Assert.AreEqual("2", res1.Value);

		var res2 =
			result.Find(p => p.Key == "FileIndexItemDirectoryCount");
		Assert.AreEqual("1", res2.Value);

		var res3 =
			result.Find(p => p.Key == "FileIndexItemCount");
		Assert.AreEqual("1", res3.Value);
	}

	[TestMethod]
	public async Task AddDatabaseData_Exception()
	{
		var appSettings = new AppSettings { EnablePackageTelemetry = true };
		var packageTelemetry = new PackageTelemetry(null!, appSettings,
			new FakeIWebLogger(),
			new FakeIQueryException(new WebException("test")), new FakeIDeviceIdService());
		var result =
			await packageTelemetry.AddDatabaseData(new List<KeyValuePair<string, string>>());

		var res1 =
			result.Find(p => p.Key == "FileIndexItemTotalCount");
		Assert.AreEqual("-1", res1.Value);

		var res2 =
			result.Find(p => p.Key == "FileIndexItemDirectoryCount");
		Assert.AreEqual("-1", res2.Value);

		var res3 =
			result.Find(p => p.Key == "FileIndexItemCount");
		Assert.AreEqual("-1", res3.Value);
	}

	private sealed class TestClass
	{
		public bool Test { get; } = true;
	}

	private sealed class PropValueTestClass
	{
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		public string Test { get; set; } = string.Empty;
	}
}
