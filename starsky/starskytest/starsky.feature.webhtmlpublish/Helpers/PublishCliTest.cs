using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Helpers
{
	[TestClass]
	public class PublishCliTest
	{
		[TestMethod]
		public async Task Publisher_Help()
		{
			var console = new FakeConsoleWrapper();

			await new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
					new AppSettings(), console).Publisher(new []{"-h"});
			
			Assert.IsTrue(console.WrittenLines.FirstOrDefault().Contains("Starksy WebHtml Cli ~ Help:"));
			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("  use -v -help to show settings: "));
		}

		[TestMethod]
		public async Task Publisher_Default()
		{
			var console = new FakeConsoleWrapper();
			await new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
				new AppSettings(), console).Publisher(new []{""});
			
			Assert.IsTrue(console.WrittenLines.FirstOrDefault().Contains("Please use the -p to add a path first"));
		}
		
		[TestMethod]
		public async Task Publisher_PathArg()
		{
			var console = new FakeConsoleWrapper();
			await new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
				new AppSettings(), console).Publisher(new []{"-p"});
			
			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("is not found"));
		}
		
		[TestMethod]
		public async Task Publisher_NoSettingsFileInFolder()
		{			
			var console = new FakeConsoleWrapper();
			var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"}));

			await new PublishCli(fakeSelectorStorage, new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
				new AppSettings(), console).Publisher(new []{"-p", "/test"});

			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("done"));
		}
		
		[TestMethod]
		public async Task Publisher_WarnWhenAlreadyRun()
		{			
			var console = new FakeConsoleWrapper();
			var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{ Path.DirectorySeparatorChar + "test" }, 
				new List<string>{$"{Path.DirectorySeparatorChar}test{Path.DirectorySeparatorChar}_settings.json"}));

			await new PublishCli(fakeSelectorStorage, new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
				new AppSettings(), console).Publisher(new []{"-p", Path.DirectorySeparatorChar + "test"});

			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("_settings.json"));
		}


	}
}
