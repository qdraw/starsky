using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Helpers;

[TestClass]
public sealed class ExportManifestTest
{
	[TestMethod]
	public void ExportManifestTest_Export()
	{
		var appSettings = new AppSettings();

		var storage = new FakeIStorage();
		new PublishManifest(storage)
			.ExportManifest(appSettings.StorageFolder, "Test",
				new Dictionary<string, bool>());

		var expectedPath = Path.Combine(appSettings.StorageFolder, "_settings.json");
		Assert.IsTrue(storage.ExistFile(expectedPath));
	}

	[TestMethod]
	public void ExportManifestTest_Result()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage();
		var result = new PublishManifest(storage)
			.ExportManifest(appSettings.StorageFolder, "Test",
				new Dictionary<string, bool>());

		Assert.AreEqual("Test", result.Name);
	}

	[TestMethod]
	public async Task ExportManifestTest_Export_JsonCompare()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage();
		new PublishManifest(storage)
			.ExportManifest(appSettings.StorageFolder, "Test",
				new Dictionary<string, bool>());

		var expectedPath = Path.Combine(appSettings.StorageFolder, "_settings.json");
		var output = ( await
			StreamToStringHelper.StreamToStringAsync(
				storage.ReadStream(expectedPath)) ).Replace("\r\n", "\n");

		const string expectedOutput = $"{{\n  \"Name\": \"Test\",\n  " +
		                              $"\"Copy\": {{}},\n  \"Slug\": \"test\",\n" +
		                              $"  \"Export\": \"";

		// expectedOutput without date and version to avoid flaky tests
		Assert.Contains(expectedOutput, output);
	}

	[TestMethod]
	public async Task ExportManifestTest_Export_JsonCompare_BoolValue()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage();
		new PublishManifest(storage)
			.ExportManifest(appSettings.StorageFolder, "Test2",
				new Dictionary<string, bool> { { "test.html", true } });

		var expectedPath = Path.Combine(appSettings.StorageFolder, "_settings.json");
		var output = ( await
			StreamToStringHelper.StreamToStringAsync(
				storage.ReadStream(expectedPath)) ).Replace("\r\n", "\n");

		const string expectedOutput = "  \"Name\": \"Test2\",\n";
		const string expectedOutput2 = "\"Copy\": {\n    \"test.html\": true\n  }";

		// expectedOutput without date and version to avoid flaky tests

		Assert.Contains(expectedOutput2, output);
		Assert.Contains(expectedOutput, output);
	}
}
