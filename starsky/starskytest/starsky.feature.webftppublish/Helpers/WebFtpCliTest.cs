using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webftppublish.Helpers
{
	[TestClass]
	public sealed class WebFtpCliTest
	{
		private readonly AppSettings _appSettings;
		private readonly FakeIFtpWebRequestFactory _webRequestFactory;

		public WebFtpCliTest()
		{
			_appSettings = new AppSettings
			{
				WebFtp = "ftp://test:test@testmedia.be",
			};
			_webRequestFactory = new FakeIFtpWebRequestFactory();
		}

		private static byte[] ExampleManifest()
		{
			var input = "{\n  \"Name\": \"Test\",\n  " +
			            "\"Copy\": {\n    \"1000/0_kl1k.jpg\": " +
			            "true,\n    \"_settings.json\": false\n  },\n" +
			            "  \"Slug\": \"test\",\n  \"Export\": \"20200808121411\",\n" +
			            "  \"Version\": \"0.3.0.0\"\n}";
			var stream = StringToStreamHelper.StringToStream(input) as MemoryStream;
			return stream?.ToArray();
		}
		
		[TestMethod]
		public async Task Run_Help()
		{
			var console = new FakeConsoleWrapper();
			await new WebFtpCli(_appSettings, new FakeSelectorStorage(), console,_webRequestFactory )
				.RunAsync(["-h"]);
			
			Assert.IsTrue(console.WrittenLines.FirstOrDefault()?.Contains("Starsky WebFtp Cli ~ Help:"));
			Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("  use -v -help to show settings: "));
		}

		[TestMethod]
		public async Task Run_Default()
		{
			var console = new FakeConsoleWrapper();
			await new WebFtpCli(_appSettings, new FakeSelectorStorage(), console, _webRequestFactory)
				.RunAsync(new []{""});
			
			Assert.IsTrue(console.WrittenLines.FirstOrDefault()?.Contains("Please use the -p to add a path first"));
		}
		
		[TestMethod]
		public async Task Run_PathArg()
		{
			var console = new FakeConsoleWrapper();
			await new WebFtpCli(_appSettings, new FakeSelectorStorage(), console, _webRequestFactory)
				.RunAsync(["-p"]);
			
			Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("is not found"));
		}

		[TestMethod]
		public async Task Run_NoFtpSettings()
		{			
			var console = new FakeConsoleWrapper();
			var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"}));
			
			// no ftp settings
			await new WebFtpCli(new AppSettings(),fakeSelectorStorage , console, _webRequestFactory)
				.RunAsync(new []{"-p", "/test"});
			
			Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("WebFtp settings"));
		}
		
		[TestMethod]
		public async Task Run_NoSettingsFileInFolder()
		{			
			var console = new FakeConsoleWrapper();
			var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"}));
			
			await new WebFtpCli(_appSettings,fakeSelectorStorage , console, _webRequestFactory)
				.RunAsync(["-p", "/test"]);
			
			Assert.IsTrue(console.WrittenLines.LastOrDefault()?.Contains("generate a settings file"));
		}
		
		[TestMethod]
		public async Task Run_SettingsFile_successful()
		{			
			var console = new FakeConsoleWrapper();

			var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/test"}, 
				new List<string>{$"/test{Path.DirectorySeparatorChar}_settings.json", 
					"/test/1000/0_kl1k.jpg"}, new List<byte[]> {ExampleManifest(), Array.Empty<byte>()}));
			// instead of new byte[0]
			
			await new WebFtpCli(_appSettings, fakeSelectorStorage , console, _webRequestFactory)
				.RunAsync(new []{"-p", "/test"});

			var isSuccess = console.WrittenLines?.LastOrDefault()?
				.Contains("Ftp copy successful done");

			switch ( isSuccess )
			{
				// To Debug why the test has failed
				case false:
				{
					foreach ( var line in console.WrittenLines )
					{
						Console.WriteLine(line);
					}

					break;
				}
				case null:
					Assert.IsNotNull(isSuccess);
					break;
			}

			Assert.IsTrue(isSuccess);
		}

	}
}
