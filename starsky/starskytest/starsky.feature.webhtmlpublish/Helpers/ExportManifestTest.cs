using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Helpers
{
	[TestClass]
	public class ExportManifestTest
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
		
	}
}
