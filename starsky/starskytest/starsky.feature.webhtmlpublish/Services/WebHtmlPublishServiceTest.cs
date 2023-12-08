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
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.webhtmlpublish.Services
{
	[TestClass]
	public sealed class WebHtmlPublishServiceTest
	{
		[TestMethod]
		public async Task RenderCopy_NoConfigItems()
		{
			var storage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(storage);
			var appSettings = new AppSettings();
			var service = new WebHtmlPublishService(new FakeIPublishPreflight(), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
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
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
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
							ContentType = TemplateContentType.OnlyFirstJpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder =  "1000",
							Append = "__fi_kl"
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
					new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			
			var result = await service.RenderCopy(new List<FileIndexItem>(), 
				"test", "test", "/");

			Assert.IsNotNull(result);
		}

		[TestMethod]
		public async Task RenderCopy_OnlyFirstJpeg_ShouldNotCrash()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{CreateAnImage.Bytes.ToArray()});
			
			var selectorStorage = new FakeSelectorStorage(storage);
			var appSettings = new AppSettings
			{
				PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
				{
					{"test", new List<AppSettingsPublishProfiles>
					{
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.OnlyFirstJpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder =  "",
							Append = "__fi_kl"
						}
					}}
				}
			};
			var overlayService = new FakeIOverlayImage(selectorStorage);
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), overlayService,
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			
			var result = await service.RenderCopy(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg")
				}, 
				"test", "test", "/");
			
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void AddFileHashIfNotExist_Test()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{CreateAnImage.Bytes.ToArray()});
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(null,selectorStorage,null,
				null,null,null, new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			var list = service.AddFileHashIfNotExist(new List<FileIndexItem> {new FileIndexItem("/test.jpg")});
			Assert.IsTrue(list.FirstOrDefault()?.FileHash != string.Empty);
		}
		
		[TestMethod]
		public async Task PreGenerateThumbnail_Test()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{CreateAnImageNoExif.Bytes.ToArray()});
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(null,selectorStorage,null,
				null,null,null, new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			var input = new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = "test_hash_01"
				}
			}.AsEnumerable();
			
			await service.PreGenerateThumbnail(input,"");

			Assert.IsTrue(storage.ExistFile("test_hash_01"));
		}
		
		[TestMethod]
		public void ShouldSkipExtraLarge_IncludeXtraLarge()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{new byte[10]});
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(new FakeIPublishPreflight(new List<AppSettingsPublishProfiles>{
				{
					new AppSettingsPublishProfiles
					{
						SourceMaxWidth = 2000
					}
				}}),
				selectorStorage,null,
				null,null,null, new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			
			var result = service.ShouldSkipExtraLarge("");
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void ShouldSkipExtraLarge_SkipExtraLarge()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{new byte[10]});
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(new FakeIPublishPreflight(new List<AppSettingsPublishProfiles>{
				{
					new AppSettingsPublishProfiles
					{
						SourceMaxWidth = 1000
					}
				}}),
				selectorStorage,null,
				null,null,null, new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			
			var result = service.ShouldSkipExtraLarge("");
			Assert.IsTrue(result);
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
			var storage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService());
		
			// Write to actual Disk
		
			var profiles = new PublishPreflight(appSettings, 
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()).GetPublishProfileName("default");
		
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
		public async Task GenerateJpeg_Thumbnail_Test()
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
			var storage = new FakeIStorage(new List<string>(),
				new List<string>{"fileHash"}, 		
				new List<byte[]>{CreateAnImage.Bytes.ToArray()});
			
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			
			var profiles = new PublishPreflight(appSettings, 
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()).GetPublishProfileName("default");

			var generateJpeg = await service.GenerateJpeg(profiles.FirstOrDefault(), 
				new List<FileIndexItem> {new FileIndexItem("/test.jpg"){FileHash = "fileHash"}},
				Path.DirectorySeparatorChar.ToString(),1);
			
			Assert.IsTrue(generateJpeg.ContainsKey("test.jpg"));
			Assert.IsTrue(storage.ExistFile(Path.DirectorySeparatorChar + "test.jpg"));
		}
		
		
		[TestMethod]
		public async Task GenerateJpeg_Thumbnail_CorruptOutput_Test()
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
			var storage = new FakeIStorage(new List<string>(),
				new List<string>{"corrupt"}, 		
				new List<byte[]>{CreateAnImage.Bytes.ToArray()});
			
			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			
			var profiles = new PublishPreflight(appSettings, 
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()).GetPublishProfileName("default");

			var generateJpeg = await service.GenerateJpeg(profiles.FirstOrDefault(), 
				new List<FileIndexItem> {new FileIndexItem("/test.jpg"){FileHash = "fileHash"}},
				Path.DirectorySeparatorChar.ToString(),1);
			
			// should not output file due corrupt output of image generation
			Assert.IsTrue(generateJpeg.ContainsKey("test.jpg"));
			
			// removed in script due corrupt output
			Assert.IsFalse(storage.ExistFile(Path.DirectorySeparatorChar + "test.jpg"));
		}
		
		[TestMethod]
		public async Task GenerateJpeg_Large_NotFound_Test()
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
					new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			
			var profiles = new PublishPreflight(appSettings, 
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()).GetPublishProfileName("default");

			var generateJpeg = await service.GenerateJpeg(profiles.FirstOrDefault(), 
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")},
				Path.DirectorySeparatorChar.ToString(),1);
			
			Assert.IsTrue(generateJpeg.ContainsKey("test.jpg"));
			Assert.IsFalse(storage.ExistFile(Path.DirectorySeparatorChar + "test.jpg"));
		}
		
		[TestMethod]
		public async Task GenerateJpeg_Large_Test()
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
			var storage = new FakeIStorage(new List<string>(),
				new List<string>{"/test.jpg"}, 		
				new List<byte[]>{CreateAnImage.Bytes.ToArray()});

			var selectorStorage = new FakeSelectorStorage(storage);
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));
			
			var profiles = new PublishPreflight(appSettings, 
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()).GetPublishProfileName("default");

			var generateJpeg = await service.GenerateJpeg(profiles.FirstOrDefault(), 
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")},
				Path.DirectorySeparatorChar.ToString(),1);
			
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
					new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()), selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));

			await service.GenerateMoveSourceFiles(profile,
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")}, "/", 
				true);

			// True param situation
			Assert.IsTrue(storage.GetAllFilesInDirectoryRecursive("/").Where(p => p != null)
				.FirstOrDefault(p => p.Contains("src/test.jpg")) != null);

			// is False instead of True
			Assert.IsFalse(storage.ExistFile("/test.jpg"));
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
			
			var service = new WebHtmlPublishService(new PublishPreflight(appSettings, 
					new ConsoleWrapper(), 
					new FakeSelectorStorage(storage), new FakeIWebLogger()), 
				selectorStorage, appSettings,
				new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(selectorStorage));

			await service.GenerateMoveSourceFiles(profile,
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")}, "/", 
				false);

			// False situation
			Assert.IsTrue(storage.GetAllFilesInDirectoryRecursive("/")
				.FirstOrDefault(p => p != null && p.Contains("src/test.jpg")) != null);

			// is True instead of False
			Assert.IsTrue(storage.ExistFile("/test.jpg"));
		}

	}
}
