using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.packagetelemetry.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.webtelemetry.Helpers
{
	
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
			var packageTelemetry = new PackageTelemetry(httpClientHelper, new AppSettings(), new FakeIWebLogger(), new FakeIQuery());

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
			Assert.IsTrue(systemData.Any(p => p.Key == "AspNetCoreEnvironment"));
		}

		[TestMethod]
		public void GetSystemDataTestDocker()
		{
			var httpProvider = new FakeIHttpProvider();
			var httpClientHelper = new HttpClientHelper(httpProvider, null!, new FakeIWebLogger());
			var packageTelemetry = new PackageTelemetry(httpClientHelper, new AppSettings(), new FakeIWebLogger(), new FakeIQuery());

			var sourceValue = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
			Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER","true");
			
			var systemData = packageTelemetry.GetSystemData(OSPlatform.Linux);
			
			Assert.AreEqual("True", systemData.FirstOrDefault(p => p.Key == "DockerContainer").Value);
			
			Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER","false");
			
			var systemDataFalse = packageTelemetry.GetSystemData(OSPlatform.Linux);

			Assert.AreEqual("False", systemDataFalse.FirstOrDefault(p => p.Key == "DockerContainer").Value);
			
			Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER",sourceValue);
		}

		[TestMethod]
		public void ParseContent_Null()
		{
			var result = PackageTelemetry.ParseContent(null);
			Assert.AreEqual("null",result);
		}
	
		[TestMethod]
		public void GetPropValue_Null()
		{
			var result = PackageTelemetry.GetPropValue(null, "test");
			Assert.AreEqual(null,result);
		}

		private class TestClass
		{
			public bool Test { get; set; } = true;
		}
		
		[TestMethod]
		public void GetPropValue_Object_TestClass()
		{
			var result = PackageTelemetry.GetPropValue(new TestClass(), "Test");
			Assert.AreEqual(true,result);
		}

		private class PropValueTestClass
		{
			public string Test { get; set; }
		}
	
		[TestMethod]
		public void GetPropValue_ReadValue()
		{
			var result = PackageTelemetry.GetPropValue(new PropValueTestClass{Test = "1"}, "Test");
			Assert.AreEqual("1",result);
		}

	
		[TestMethod]
		public void AddAppSettingsData()
		{
			var httpProvider = new FakeIHttpProvider();
			var appSettings = new AppSettings();
			var httpClientHelper = new HttpClientHelper(httpProvider, null!, new FakeIWebLogger());
			var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings, new FakeIWebLogger(), new FakeIQuery());
			var result = packageTelemetry.AddAppSettingsData(new List<KeyValuePair<string, string>>());

			Assert.IsTrue(result.Any(p => p.Key == "AppSettingsName"));
		}

		[TestMethod]
		public async Task PackageTelemetrySend_Disabled()
		{
			var httpProvider = new FakeIHttpProvider();
			var appSettings = new AppSettings{EnablePackageTelemetry = false};
			var httpClientHelper = new HttpClientHelper(httpProvider, null!, new FakeIWebLogger());
			var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings, new FakeIWebLogger(), new FakeIQuery());
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
			var packageTelemetry = new PackageTelemetry(httpClientHelper, appSettings, new FakeIWebLogger(), new FakeIQuery());
			var result = await packageTelemetry.PackageTelemetrySend();
			Assert.IsTrue(result);
		}

		[TestMethod]
		public async Task AddDatabaseData()
		{
			var appSettings = new AppSettings{EnablePackageTelemetry = true};
			var packageTelemetry = new PackageTelemetry(null!, appSettings, new FakeIWebLogger(), new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg"),
				new FileIndexItem("/test"){IsDirectory = true},
			}));
			
			var result = await packageTelemetry.AddDatabaseData(new List<KeyValuePair<string, string>>());

			var res1 =
				result.FirstOrDefault(p => p.Key == "FileIndexItemTotalCount");
			Assert.AreEqual("2", res1.Value);
			
			var res2 =
				result.FirstOrDefault(p => p.Key == "FileIndexItemDirectoryCount");
			Assert.AreEqual("1", res2.Value);
			
			var res3 =
				result.FirstOrDefault(p => p.Key == "FileIndexItemCount");
			Assert.AreEqual("1", res3.Value);
		}


		[TestMethod]
		public async Task AddDatabaseData_Exception()
		{
			var appSettings = new AppSettings {EnablePackageTelemetry = true};
			var packageTelemetry = new PackageTelemetry(null!, appSettings,
				new FakeIWebLogger(),
				new FakeIQueryException(new ArgumentNullException()));
			var result = await packageTelemetry.AddDatabaseData(new List<KeyValuePair<string, string>>());
			
			var res1 =
				result.FirstOrDefault(p => p.Key == "FileIndexItemTotalCount");
			Assert.AreEqual("-1", res1.Value);
			
			var res2 =
				result.FirstOrDefault(p => p.Key == "FileIndexItemDirectoryCount");
			Assert.AreEqual("-1", res2.Value);
			
			var res3 =
				result.FirstOrDefault(p => p.Key == "FileIndexItemCount");
			Assert.AreEqual("-1", res3.Value);

		}

	}

}
