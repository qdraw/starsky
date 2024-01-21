using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Helpers
{
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
		public async Task ExportManifestTest_Export_JsonCompare()
		{
			var appSettings = new AppSettings();

			var storage = new FakeIStorage();
			var manifest = new PublishManifest(storage)
				.ExportManifest(appSettings.StorageFolder, "Test", 
					new Dictionary<string, bool>());

			var expectedPath = Path.Combine(appSettings.StorageFolder, "_settings.json");
			var output = (await 
				StreamToStringHelper.StreamToStringAsync(
					storage.ReadStream(expectedPath))).Replace("\r\n","\n");

			var expectedOutput =
				$"{{\n  \"Name\": \"Test\",\n  \"Copy\": {{}},\n  \"Slug\": \"test\",\n" +
				$"  \"Export\": \"{manifest.Export}\",\n  \"Version\": \"{manifest.Version}\"\n}}";
			
			Assert.AreEqual(expectedOutput, output);
		}	
	}
}
