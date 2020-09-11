using System.Collections.Generic;
using System.Linq;
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
		public void Publisher_Help()
		{
			var console = new FakeConsoleWrapper();

			new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
					new AppSettings(), console).Publisher(new []{"-h"});
			
			Assert.IsTrue(console.WrittenLines.FirstOrDefault().Contains("Starksy WebHtml Cli ~ Help:"));
			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("  use -v -help to show settings: "));
		}

		[TestMethod]
		public void Publisher_Default()
		{
			var console = new FakeConsoleWrapper();
			new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
				new AppSettings(), console).Publisher(new []{""});
			
			Assert.IsTrue(console.WrittenLines.FirstOrDefault().Contains("Please use the -p to add a path first"));
		}
		
		[TestMethod]
		public void Publisher_PathArg()
		{
			var console = new FakeConsoleWrapper();
			new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
				new AppSettings(), console).Publisher(new []{"-p"});
			
			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("is not found"));
		}
		
		[TestMethod]
		public void Publisher_NoSettingsFileInFolder()
		{			
			var console = new FakeConsoleWrapper();
			var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"}));

			new PublishCli(fakeSelectorStorage, new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
				new AppSettings(), console).Publisher(new []{"-p", "/test"});

			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("done"));
		}
		
		[TestMethod]
		public void Publisher_WarnWhenAlreadyRun()
		{			
			var console = new FakeConsoleWrapper();
			var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"}, 
				new List<string>{"/test/_settings.json"}));

			new PublishCli(fakeSelectorStorage, new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
				new AppSettings(), console).Publisher(new []{"-p", "/test"});

			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("_settings.json"));
		}


	}
}
