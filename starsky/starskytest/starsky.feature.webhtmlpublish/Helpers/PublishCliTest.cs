using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Helpers
{
	[TestClass]
	public class PublishCliTest
	{
		[TestMethod]
		public void Run_Help()
		{
			var console = new FakeConsoleWrapper();

			new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
					new AppSettings(), console).Publisher(new []{"-h"});
			
			Assert.IsTrue(console.WrittenLines.FirstOrDefault().Contains("Starksy WebHtml Cli ~ Help:"));
			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("  use -v -help to show settings: "));
		}

		[TestMethod]
		public void Run_Default()
		{
			var console = new FakeConsoleWrapper();
			new PublishCli(new FakeSelectorStorage(), new FakeIPublishPreflight(), new FakeIWebHtmlPublishService(), 
				new AppSettings(), console).Publisher(new []{""});
			
			Assert.IsTrue(console.WrittenLines.FirstOrDefault().Contains("Please use the -p to add a path first"));
		}
		
		//
		// [TestMethod]
		// public void Run_PathArg()
		// {
		// 	var console = new FakeConsoleWrapper();
		// 	new WebFtpCli(_appSettings, new FakeSelectorStorage(), console, _webRequestFactory)
		// 		.Run(new []{"-p"});
		// 	
		// 	Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("is not found"));
		// }

	}
}
