using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Models;
using starskycore.Helpers;
using starskycore.Models;
using starskytest.FakeMocks;

namespace starskytest.Helpers
{
	[TestClass]
	public class ExportManifestTest
	{

		[TestMethod]
		public void ExportManifestTest_Export()
		{
			var plainTextFileHelper = new FakePlainTextFileHelper();
			var appSettings = new AppSettings {Name = "Test"};

			var storage = new FakeIStorage();
			new PublishManifest(storage, appSettings, plainTextFileHelper).ExportManifest();

			var expectedPath = Path.Combine(appSettings.StorageFolder, "_settings.json");
			Assert.IsTrue(storage.ExistFile(expectedPath));
		}

		[TestMethod]
		public void ExportManifestTest_Import()
		{
			var test = "{\"Name\":\"Test\",\"Slug\":\"test\"}";
			var plainTextFileHelper = new FakePlainTextFileHelper(test);
			var appSettings = new AppSettings { Name = "Test" };
		}
		
	}
}
