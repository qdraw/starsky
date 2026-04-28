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

	[TestMethod]
	public async Task ReadAppSettingsTest_ReadQueueSettings()
	{
		var appSettingsPath =
			Path.Combine(new AppSettings().BaseDirectoryProject, "appsettings-test-queue.json");
		var stream = StringToStreamHelper.StringToStream(
			"{\n  \"app\" : {\n    \"queue\" : {\n      \"default\" : \"RabbitMq\",\n      \"databasePollIntervalInMilliseconds\" : 750,\n      \"queues\" : {\n        \"Update\" : \"Database\",\n        \"Thumbnail\" : \"InMemory\"\n      },\n      \"rabbitMq\" : {\n        \"host\" : \"mq.internal\",\n        \"port\" : 5673,\n        \"username\" : \"starsky\",\n        \"password\" : \"secret\",\n        \"virtualHost\" : \"/starsky\"\n      }\n    }\n  }\n}");
		await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(stream,
			appSettingsPath);

		var readAppSettings = await ReadAppSettings.Read(appSettingsPath);

		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FileDelete(appSettingsPath);

		Assert.AreEqual(QueueBackendType.RabbitMq, readAppSettings?.App.Queue.Default);
		Assert.AreEqual(750, readAppSettings?.App.Queue.DatabasePollIntervalInMilliseconds);
		Assert.AreEqual(QueueBackendType.Database,
			readAppSettings?.App.Queue.Queues["Update"]);
		Assert.AreEqual("mq.internal", readAppSettings?.App.Queue.RabbitMq.Host);
		Assert.AreEqual(5673, readAppSettings?.App.Queue.RabbitMq.Port);
	}
}
