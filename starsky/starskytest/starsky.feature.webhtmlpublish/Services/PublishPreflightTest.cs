using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services
{
	[TestClass]
	public class PublishPreflightTest
	{
		private AppSettings _testAppSettings = new AppSettings{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test", new List<AppSettingsPublishProfiles>()
				}
			}
		};
		
		[TestMethod]
		public void GetPublishProfileNames_listNoContent()
		{
			var appSettings = new AppSettings();
			var list = new PublishPreflight(appSettings, 
				new ConsoleWrapper()).GetPublishProfileNames();
			
			Assert.AreEqual(0, list.Count);
		}
		
		[TestMethod]
		public void GetPublishProfileNames_list()
		{
			var list = new PublishPreflight(_testAppSettings, 
				new ConsoleWrapper()).GetPublishProfileNames();
			
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("test", list[0].Item2);
			Assert.AreEqual(0, list[0].Item1);
		}

		[TestMethod]
		public void GetAllPublishProfileNames_item()
		{
			var list = new PublishPreflight(_testAppSettings, 
				new ConsoleWrapper()).GetAllPublishProfileNames();
			
			Assert.AreEqual("test", list.FirstOrDefault());
		}

		[TestMethod]
		public void GetPublishProfileNameByIndex_0()
		{
			var data = new PublishPreflight(_testAppSettings, 
				new ConsoleWrapper()).GetPublishProfileNameByIndex(0);
			Assert.AreEqual("test", data);
		}

		[TestMethod]
		public void GetNameConsole_WithArg()
		{
			var result= new PublishPreflight(_testAppSettings,
				new FakeConsoleWrapper()).GetNameConsole("/", new List<string> {"-n", "t"});

			Assert.AreEqual("t", result );
		}
		
		[TestMethod]
		public void GetNameConsole_EnterDefaultOption()
		{
			var consoleWrapper = new FakeConsoleWrapper
			{
				LinesToRead = new List<string>{string.Empty}
			};

			var result= new PublishPreflight(_testAppSettings, 
				consoleWrapper).GetNameConsole("/test", new List<string> ());
			
			Assert.AreEqual("test",result);
		}
		
		[TestMethod]
		public void GetNameConsole_UpdateConsoleInput()
		{
			var consoleWrapper = new FakeConsoleWrapper
			{
				LinesToRead = new List<string>{"updated"}
			};

			var result= new PublishPreflight(_testAppSettings, 
				consoleWrapper).GetNameConsole("/test", new List<string> ());
			
			Assert.AreEqual("updated",result);
		}

	}
}
