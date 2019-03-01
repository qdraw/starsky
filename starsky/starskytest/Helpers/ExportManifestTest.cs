using Microsoft.VisualStudio.TestTools.UnitTesting;
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

			new ExportManifest(appSettings, plainTextFileHelper).Export();
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
