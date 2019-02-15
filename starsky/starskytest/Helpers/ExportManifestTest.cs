using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Models;
using starskytest.FakeMocks;

namespace starskytests.Helpers
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

		public void ExportManifestTest_Import()
		{
			var test = "{\"Name\":\"Test\",\"Slug\":\"test\"}";
			var plainTextFileHelper = new FakePlainTextFileHelper(test);
			var appSettings = new AppSettings { Name = "Test" };



		}
	}
}
