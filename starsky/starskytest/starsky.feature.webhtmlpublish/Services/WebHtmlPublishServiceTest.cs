using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.webhtmlpublish.Services
{
	[TestClass]
	public class WebHtmlPublishServiceTest
	{
		[TestMethod]
		public async Task NoConfigItems()
		{
			var storage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(storage);
			var appSettings = new AppSettings();
			var service = new WebHtmlPublishService(new FakeIPublishPreflight(), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper());
			var result = await service.RenderCopy(new List<FileIndexItem>(), 
				"test", "test", "/");

			Assert.IsNull(result);
		}	
		
		[TestMethod]
		public async Task KeyNotFound()
		{
			var storage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(storage);
			var appSettings = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{"same", new List<AppSettingsPublishProfiles>
					{
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder =  "1000",
							Append = "_kl1k"
						}
					}}
				}
			};
			var service = new WebHtmlPublishService(new FakeIPublishPreflight(), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper());
			var result = await service.RenderCopy(new List<FileIndexItem>(), 
				"test", "test", "/");

			Assert.IsNull(result);
		}
		
		[TestMethod]
		public async Task ShouldNotCrash()
		{
			var storage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(storage);
			var appSettings = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{"test", new List<AppSettingsPublishProfiles>
					{
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.Html,
						},
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder =  "1000",
							Append = "_kl1k"
						},
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.MoveSourceFiles,
						},
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.PublishContent,
						},
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.PublishManifest,
						},
					}}
				}
			};
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, new ConsoleWrapper()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper());
			
			var result = await service.RenderCopy(new List<FileIndexItem>(), 
				"test", "test", "/");

			Assert.IsNotNull(result);
		}
	}
}
