using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.webtelemetry.Helpers;

[TestClass]
public class PackageTelemetryTest
{
	[TestMethod]
	public void GetCurrentOsPlatformTest()
	{
		var content = PackageTelemetry.GetCurrentOsPlatform();
		Assert.IsNotNull(content);

		var allOsPlatforms = new List<OSPlatform>
		{
			OSPlatform.Linux,
			OSPlatform.Windows,
			OSPlatform.OSX,
			OSPlatform.FreeBSD
		};
		Assert.IsTrue(allOsPlatforms.Contains(content.Value));
	}

	[TestMethod]
	public void GetSystemDataTest()
	{
		var httpProvider = new FakeIHttpProvider();
		var appSettings = new AppSettings();
		var httpClientHelper = new HttpClientHelper(httpProvider, null!, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, new AppSettings());

		var systemData = packageTelemetry.GetSystemData();
		Assert.IsTrue(systemData.Any(p => p.Key == "AppVersion"));
		Assert.AreEqual(systemData.FirstOrDefault(p => p.Key == "AppVersion").Value, 
			appSettings.AppVersion);
		Assert.IsTrue(systemData.Any(p => p.Key == "NetVersion"));
		Assert.AreEqual(systemData.FirstOrDefault(p => p.Key == "NetVersion").Value, 
			RuntimeInformation.FrameworkDescription);
		Assert.IsTrue(systemData.Any(p => p.Key == "OSArchitecture"));
		Assert.AreEqual(systemData.FirstOrDefault(p => p.Key == "OSArchitecture").Value, 
			RuntimeInformation.OSArchitecture.ToString());
		Assert.IsTrue(systemData.Any(p => p.Key == "OSVersion"));
		Assert.IsTrue(systemData.Any(p => p.Key == "OSDescriptionLong"));
		Assert.IsTrue(systemData.Any(p => p.Key == "OSPlatform"));
		Assert.IsTrue(systemData.Any(p => p.Key == "DockerContainer"));
		Assert.IsTrue(systemData.Any(p => p.Key == "CurrentCulture"));
		Assert.IsTrue(systemData.Any(p => p.Key == "BuildDate"));
		Assert.IsTrue(systemData.Any(p => p.Key == "AspNetCoreEnvironment"));
	}

	[TestMethod]
	public void ParseContent_Null()
	{
		var result = PackageTelemetry.ParseContent(null);
		Assert.AreEqual("null",result);
	}

	[TestMethod]
	public void AddAppSettingsData()
	{
		var httpProvider = new FakeIHttpProvider();
		var appSettings = new AppSettings();
		var httpClientHelper = new HttpClientHelper(httpProvider, null!, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings);
		var result = packageTelemetry.AddAppSettingsData(new List<KeyValuePair<string, string>>());

		Assert.IsTrue(result.Any(p => p.Key == "AppSettingsName"));
	}

	[TestMethod]
	public async Task PackageTelemetrySend_Disabled()
	{
		var httpProvider = new FakeIHttpProvider();
		var appSettings = new AppSettings{EnablePackageTelemetry = false};
		var httpClientHelper = new HttpClientHelper(httpProvider, null!, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings);
		var result = await packageTelemetry.PackageTelemetrySend();
		Assert.IsNull(result);
	}
	
	
	[TestMethod]
	public async Task PackageTelemetrySend_HasSend()
	{
		var httpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			{"https://" + PackageTelemetry.PackageTelemetryUrl,new StringContent(string.Empty)}
		});
		var appSettings = new AppSettings{EnablePackageTelemetry = true};
		var httpClientHelper = new HttpClientHelper(httpProvider, null!, new FakeIWebLogger());
		var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings);
		var result = await packageTelemetry.PackageTelemetrySend();
		Assert.IsTrue(result);
	}
}
