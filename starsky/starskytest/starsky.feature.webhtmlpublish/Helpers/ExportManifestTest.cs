using System;
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
			var plainTextFileHelper = new FakePlainTextFileHelper();
			var appSettings = new AppSettings();

			var storage = new FakeIStorage();
			new PublishManifest(storage, plainTextFileHelper)
				.ExportManifest(appSettings.StorageFolder, "Test", 
					new List<Tuple<string, bool>>());

			var expectedPath = Path.Combine(appSettings.StorageFolder, "_settings.json");
			Assert.IsTrue(storage.ExistFile(expectedPath));
		}
		
	}
}
