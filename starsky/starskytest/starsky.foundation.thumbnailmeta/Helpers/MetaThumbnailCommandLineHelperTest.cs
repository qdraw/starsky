using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starsky.foundation.http.Services;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailmeta.Helpers
{
	[TestClass]
	public sealed class MetaThumbnailCommandLineHelperTest
	{
		
		[TestMethod]
		public async Task GeoCliInput_Help()
		{
			var console = new FakeConsoleWrapper();
			var metaCli = new MetaThumbnailCommandLineHelper(new FakeSelectorStorage(), 
				new AppSettings(), console, new FakeIMetaExifThumbnailService(),
				new FakeIMetaUpdateStatusThumbnailService());
			
			await metaCli.CommandLineAsync(new List<string> {"-h",}.ToArray());

			Assert.IsTrue(console.WrittenLines[0].Contains("Help"));
		}

		[TestMethod]
		public async Task GeoCliInput_DefaultFlow()
		{
			var console = new FakeConsoleWrapper();
			var fakeMetaThumb = new FakeIMetaExifThumbnailService();
			var metaCli = new MetaThumbnailCommandLineHelper(new FakeSelectorStorage(), 
				new AppSettings(), console, fakeMetaThumb,
				new FakeIMetaUpdateStatusThumbnailService());
			
			await metaCli.CommandLineAsync(new List<string> {"-p", "/test"}.ToArray());
			
			Assert.AreEqual("/test",fakeMetaThumb.Input[0].Item1);

			Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("Done"));
		}
		
		[TestMethod]
		public async Task GeoCliInput_RelativePath()
		{
			var console = new FakeConsoleWrapper();
			var fakeMetaThumb = new FakeIMetaExifThumbnailService();
			var metaCli = new MetaThumbnailCommandLineHelper(new FakeSelectorStorage(), 
				new AppSettings(), console, fakeMetaThumb,
				new FakeIMetaUpdateStatusThumbnailService());
			
			await metaCli.CommandLineAsync(new List<string> {"-g", "0"}.ToArray());

			var inputDate = fakeMetaThumb.Input[0].Item1;
			var currentYear = DateTime.Now.Year.ToString();

			Assert.IsTrue(inputDate.Contains(currentYear));
			Assert.IsTrue(console.WrittenLines.LastOrDefault()!.Contains("Done"));
		}
	}
}
