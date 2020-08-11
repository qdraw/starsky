using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.webhtmlpublish.Services
{
	[TestClass]
	public class WebHtmlPublishServiceTest
	{
		[TestMethod]
		public async Task RenderCopy_NoConfigItems()
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
		public async Task RenderCopy_KeyNotFound()
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
		public async Task RenderCopy_ShouldNotCrash()
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
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper());
			
			var result = await service.RenderCopy(new List<FileIndexItem>(), 
				"test", "test", "/");

			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void AddFileHashIfNotExist_Test()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(null,selectorStorage,null,
				null,null,null);
			var list = service.AddFileHashIfNotExist(new List<FileIndexItem> {new FileIndexItem("/test.jpg")});
			Assert.IsTrue(list.FirstOrDefault().FileHash != string.Empty);
		}
		
		[TestMethod]
		public void PreGenerateThumbnail_Test()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(null,selectorStorage,null,
				null,null,null);
			var input = new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = "test"
					
				}
			}.AsEnumerable();
			service.PreGenerateThumbnail(input);
			// should not crash
		}
		
		[TestMethod]
		public async Task GenerateWebHtml_RealFsTest()
		{
			var appSettings = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{"default", new List<AppSettingsPublishProfiles>
					{
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.Html,
							Path = "index.html",
							Template = "Index.cshtml"
						}
					}}
				},
				Verbose = true
			};

			// REAL FS
			var storage = new StorageHostFullPathFilesystem();
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper());

			// Write to actual Disk

			var profiles = new PublishPreflight(appSettings, 
				new ConsoleWrapper()).GetPublishProfileName("default");

			var output = await service.GenerateWebHtml(profiles, 
				profiles.FirstOrDefault(), "testItem", new string[1],
				new List<FileIndexItem>{new FileIndexItem("test")}, 
				AppDomain.CurrentDomain.BaseDirectory
			);

			var outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html");
			
			Assert.IsTrue( storage.ExistFile(outputFile));
			Assert.IsTrue(output.ContainsKey("index.html"));

			// this realFS
			storage.FileDelete(outputFile);
		}

		[TestMethod]
		public void GenerateJpeg_Thumbnail_Test()
		{
			var appSettings = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{
						"default",
						new List<AppSettingsPublishProfiles>
						{
							new AppSettingsPublishProfiles
							{
								ContentType = TemplateContentType.Jpeg,
								Path = "index.html",
								MetaData = true,
							}
						}
					}
				},
				Verbose = true
			};
			var storage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper());
			
			var profiles = new PublishPreflight(appSettings, 
				new ConsoleWrapper()).GetPublishProfileName("default");

			var generateJpeg = service.GenerateJpeg(profiles.FirstOrDefault(), 
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")},
				"/");
			
			Assert.IsTrue(generateJpeg.ContainsKey("test.jpg"));
			Assert.IsTrue(storage.ExistFile(Path.DirectorySeparatorChar + "test.jpg"));
		}
		
		[TestMethod]
		public void GenerateJpeg_Large_Test()
		{
			var appSettings = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{
						"default",
						new List<AppSettingsPublishProfiles>
						{
							new AppSettingsPublishProfiles
							{
								ContentType = TemplateContentType.Jpeg,
								Path = "index.html",
								MetaData = false,
								SourceMaxWidth = 1001
							}
						}
					}
				},
				Verbose = true
			};
			var storage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper());
			
			var profiles = new PublishPreflight(appSettings, 
				new ConsoleWrapper()).GetPublishProfileName("default");

			var generateJpeg = service.GenerateJpeg(profiles.FirstOrDefault(), 
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")},
				"/");
			
			Assert.IsTrue(generateJpeg.ContainsKey("test.jpg"));
			Assert.IsTrue(storage.ExistFile(Path.DirectorySeparatorChar + "test.jpg"));
		}

		[TestMethod]
		public async Task MoveSourceFiles_True()
		{
			var profile = new AppSettingsPublishProfiles
			{
				ContentType = TemplateContentType.MoveSourceFiles, Folder = "src",
			};
			
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"});
			var selectorStorage = new FakeSelectorStorage(storage);
			var appSettings = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{
						"default",
						new List<AppSettingsPublishProfiles>
						{
							profile
						}
					}
				},
				Verbose = true
			};
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper());

			await service.GenerateMoveSourceFiles(profile,
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")}, "/", 
				true);

			Assert.IsTrue(storage.ExistFile(Path.Combine("src","test.jpg")));
			Assert.IsFalse(storage.ExistFile($"{Path.DirectorySeparatorChar}test.jpg"));
		}
		
		[TestMethod]
		public async Task MoveSourceFiles_False()
		{
			var profile = new AppSettingsPublishProfiles
			{
				ContentType = TemplateContentType.MoveSourceFiles, Folder = "src",
			};
			
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"});
			var selectorStorage = new FakeSelectorStorage(storage);
			var appSettings = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{
						"default",
						new List<AppSettingsPublishProfiles>
						{
							profile
						}
					}
				},
				Verbose = true
			};
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, new ConsoleWrapper()), 
				selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper());

			await service.GenerateMoveSourceFiles(profile,
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")}, "/", 
				false);

			Assert.IsTrue(storage.ExistFile(Path.Combine("src","test.jpg")));
			// is True instead of False
			Assert.IsTrue(storage.ExistFile($"{Path.DirectorySeparatorChar}test.jpg"));
		}
	}
}
