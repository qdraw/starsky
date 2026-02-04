using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class ReadAppSettingsTest
{
	[TestMethod]
	public void ReadAppSettingsTest_default()
	{
		var readAppSettings = ReadAppSettings.Read(string.Empty);
		Assert.IsNotNull(readAppSettings);
	}

	[TestMethod]
	public async Task ReadAppSettingsTest_readKestrelData()
	{
		var appSettingsPath =
			Path.Combine(new AppSettings().BaseDirectoryProject, "appsettings-test2.json");
		var stream = StringToStreamHelper.StringToStream(
			"{     \"Kestrel\": {\n        \"Endpoints\": {\n          " +
			"  \"Https\": {\n                \"Url\": \"https://*:8001\"\n            },\n            \"Http\": {\n      " +
			"          \"Url\": \"http://*:8000\"\n            }\n        }\n    }\n }");
		await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(stream,
			appSettingsPath);

		var readAppSettings = await ReadAppSettings.Read(appSettingsPath);

		// remove afterwards
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FileDelete(appSettingsPath);

		Assert.AreEqual("http://*:8000", readAppSettings?.Kestrel?.Endpoints?.Http?.Url);
		Assert.AreEqual("https://*:8001", readAppSettings?.Kestrel?.Endpoints?.Https?.Url);
	}

	[TestMethod]
	public async Task ReadAppSettingsTest_readApp_TrueSetting()
	{
		var appSettingsPath =
			Path.Combine(new AppSettings().BaseDirectoryProject, "appsettings-test3.json");
		var stream =
			StringToStreamHelper.StringToStream(
				"{\n    \"app\" : {\n        \"verbose\" : \"true\"\n    }\n}");
		await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(stream,
			appSettingsPath);

		var readAppSettings = await ReadAppSettings.Read(appSettingsPath);

		// remove afterwards
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FileDelete(appSettingsPath);

		Assert.IsTrue(readAppSettings?.App.Verbose);
	}
}
