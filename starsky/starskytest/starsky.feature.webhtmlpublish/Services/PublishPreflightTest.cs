using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services
{
	[TestClass]
	public class PublishPreflightTest
	{
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
			var appSettings = new AppSettings{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{
						"test", new List<AppSettingsPublishProfiles>()
					}
				}
			};
			
			var list = new PublishPreflight(appSettings, 
				new ConsoleWrapper()).GetPublishProfileNames();
			
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("test", list[0].Item2);
			Assert.AreEqual(0, list[0].Item1);
		}
	}
}
