using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.database.Models;
using starsky.foundation.optimisation.Helpers;
using starsky.foundation.optimisation.Models;
using starsky.foundation.optimisation.Services;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public sealed class WebHtmlPublishServiceTest
{
	public TestContext TestContext { get; set; }

	private static ThumbnailService SetThumbnailService(IStorage storage)
	{
		return new ThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings(),
			new FakeIUpdateStatusGeneratedThumbnailService(),
			new FakeIVideoProcess(new FakeSelectorStorage(storage)),
			new FileHashSubPathStorage(new FakeSelectorStorage(storage), new FakeIWebLogger()),
			new FakeINativePreviewThumbnailGenerator());
	}

	[TestMethod]
	public async Task RenderCopy_NoConfigItems()
	{
		var storage = new FakeIStorage();
		var selectorStorage = new FakeSelectorStorage(storage);
		var appSettings = new AppSettings();
		var service = new WebHtmlPublishService(new FakeIPublishPreflight(), selectorStorage,
			appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());
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
				{
					"same",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder = "1000",
							Append = "_kl1k"
						}
					}
				}
			}
		};
		var service = new WebHtmlPublishService(new FakeIPublishPreflight(), selectorStorage,
			appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());
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
				{
					"test",
					new List<AppSettingsPublishProfiles>
					{
						new() { ContentType = TemplateContentType.Html },
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder = "1000",
							Append = "_kl1k"
						},
						new()
						{
							ContentType = TemplateContentType.OnlyFirstJpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder = "1000",
							Append = "__fi_kl"
						},
						new() { ContentType = TemplateContentType.MoveSourceFiles },
						new() { ContentType = TemplateContentType.PublishContent },
						new() { ContentType = TemplateContentType.PublishManifest }
					}
				}
			}
		};
		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());

		var result = await service.RenderCopy(new List<FileIndexItem>(),
			"test", "test", "/");

		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task RenderCopy_OnlyFirstJpeg_ShouldNotCrash()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var selectorStorage = new FakeSelectorStorage(storage);
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"test",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.OnlyFirstJpeg,
							SourceMaxWidth = 300,
							OverlayMaxWidth = 380,
							Folder = "",
							Append = "__fi_kl"
						}
					}
				}
			}
		};
		var overlayService = new FakeIOverlayImage(selectorStorage);
		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), overlayService,
			new ConsoleWrapper(), new FakeIWebLogger(),
			SetThumbnailService(storage), new FakeImageOptimisationService());

		var result = await service.RenderCopy(
			new List<FileIndexItem> { new("/test.jpg") },
			"test", "test", "/");

		Assert.IsNotNull(result);
	}

	[TestMethod]
	public void AddFileHashIfNotExist_Test()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new WebHtmlPublishService(null!, selectorStorage, null!,
			null!, null!, null!, new FakeIWebLogger(),
			SetThumbnailService(storage), new FakeImageOptimisationService());
		var list =
			service.AddFileHashIfNotExist(new List<FileIndexItem> { new("/test.jpg") });
		Assert.AreNotEqual(string.Empty, list.FirstOrDefault()?.FileHash);
	}

	[TestMethod]
	public async Task PreGenerateThumbnail_Test()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new WebHtmlPublishService(new FakeIPublishPreflight(), selectorStorage,
			null!,
			null!, null!, null!, new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());
		var input = new List<FileIndexItem> { new("/test.jpg") { FileHash = "test_hash_01" } }
			.AsEnumerable();

		await service.PreGenerateThumbnail(input, "");

		Assert.IsTrue(storage.ExistFile("test_hash_01"));
	}

	[TestMethod]
	public void ShouldSkipExtraLarge_IncludeXtraLarge()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { new byte[10] });
		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new WebHtmlPublishService(
			new FakeIPublishPreflight(new List<AppSettingsPublishProfiles>
			{
				new() { SourceMaxWidth = 2000 }
			}),
			selectorStorage, null!,
			null!, null!, null!, new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());

		var result = service.ShouldSkipExtraLarge("");
		Assert.AreEqual(ThumbnailGenerationType.All, result);
	}

	[TestMethod]
	public void ShouldSkipExtraLarge_SkipExtraLarge()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { new byte[10] });
		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new WebHtmlPublishService(
			new FakeIPublishPreflight(new List<AppSettingsPublishProfiles>
			{
				new() { SourceMaxWidth = 1000 }
			}),
			selectorStorage, null!,
			null!, null!, null!, new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());

		var result = service.ShouldSkipExtraLarge("");
		Assert.AreEqual(ThumbnailGenerationType.SkipExtraLarge, result);
	}

	[TestMethod]
	public async Task GenerateWebHtml_RealFsTest()
	{
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"default",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Path = "index.html",
							Template = "Index.cshtml"
						}
					}
				}
			},
			Verbose = true
		};

		// REAL FS
		var storage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(),
			new FakeImageOptimisationService());

		// Write to actual Disk

		var profiles = new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger())
			.GetPublishProfileName("default");

		var output = await service.GenerateWebHtml(profiles,
			profiles.FirstOrDefault()!, "testItem", new string[1],
			new List<FileIndexItem> { new("test") },
			AppDomain.CurrentDomain.BaseDirectory
		);

		var outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html");

		Assert.IsTrue(storage.ExistFile(outputFile));
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
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							Path = "index.html",
							MetaData = true
						}
					}
				}
			},
			Verbose = true
		};
		var storage = new FakeIStorage(["/"],
			[
				ThumbnailNameHelper.Combine("fileHash", ThumbnailSize.Large,
					new AppSettings().ThumbnailImageFormat)
			],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			SetThumbnailService(storage), new FakeImageOptimisationService());

		var profiles = new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger())
			.GetPublishProfileName("default");

		var generateJpeg = await service.GenerateJpeg(profiles.FirstOrDefault()!,
			new List<FileIndexItem> { new("/test.jpg") { FileHash = "fileHash" } },
			Path.DirectorySeparatorChar.ToString(), 1);

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
						new()
						{
							ContentType = TemplateContentType.Jpeg,
							Path = "index.html",
							MetaData = true
						}
					}
				}
			},
			Verbose = true
		};
		var storage = new FakeIStorage(new List<string>(),
			new List<string> { "corrupt" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());

		var profiles = new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger())
			.GetPublishProfileName("default");

		var generateJpeg = await service.GenerateJpeg(profiles.FirstOrDefault()!,
			new List<FileIndexItem> { new("/test.jpg") { FileHash = "fileHash" } },
			Path.DirectorySeparatorChar.ToString(), 1);

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
						new()
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
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());

		var profiles = new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger())
			.GetPublishProfileName("default");

		var generateJpeg = await service.GenerateJpeg(profiles.FirstOrDefault()!,
			new List<FileIndexItem> { new("/test.jpg") },
			Path.DirectorySeparatorChar.ToString(), 1);

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
						new()
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
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());

		var profiles = new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger())
			.GetPublishProfileName("default");

		var generateJpeg = await service.GenerateJpeg(profiles.FirstOrDefault()!,
			new List<FileIndexItem> { new("/test.jpg") },
			Path.DirectorySeparatorChar.ToString(), 1);

		Assert.IsTrue(generateJpeg.ContainsKey("test.jpg"));
		Assert.IsTrue(storage.ExistFile(Path.DirectorySeparatorChar + "test.jpg"));
	}

	[TestMethod]
	public async Task MoveSourceFiles_True()
	{
		var profile = new AppSettingsPublishProfiles
		{
			ContentType = TemplateContentType.MoveSourceFiles, Folder = "src"
		};

		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" });
		var selectorStorage = new FakeSelectorStorage(storage);
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{ "default", new List<AppSettingsPublishProfiles> { profile } }
			},
			Verbose = true
		};

		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());

		await service.GenerateMoveSourceFiles(profile,
			new List<FileIndexItem> { new("/test.jpg") }, "/",
			true);

		// True param situation
		Assert.IsNotNull(storage.GetAllFilesInDirectoryRecursive("/")
			.Cast<string?>().Where(p => p != null).Cast<string>()
			.FirstOrDefault(p => p.Contains("src/test.jpg")));

		// is False instead of True
		Assert.IsFalse(storage.ExistFile("/test.jpg"));
	}

	[TestMethod]
	public async Task MoveSourceFiles_False()
	{
		var profile = new AppSettingsPublishProfiles
		{
			ContentType = TemplateContentType.MoveSourceFiles, Folder = "src"
		};

		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" });
		var selectorStorage = new FakeSelectorStorage(storage);
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{ "default", new List<AppSettingsPublishProfiles> { profile } }
			},
			Verbose = true
		};

		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(),
				new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());

		await service.GenerateMoveSourceFiles(profile,
			new List<FileIndexItem> { new("/test.jpg") }, "/",
			false);

		// False situation
		Assert.IsNotNull(storage.GetAllFilesInDirectoryRecursive("/").Cast<string?>()
			.FirstOrDefault(p => p != null && p.Contains("src/test.jpg")));

		// is True instead of False
		Assert.IsTrue(storage.ExistFile("/test.jpg"));
	}

	private static async Task<(WebHtmlPublishService, string, Dictionary<string, bool>,
		StorageHostFullPathFilesystem)> GenerateZipCreateSut()
	{
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"default",
					new List<AppSettingsPublishProfiles>
					{
						new()
						{
							ContentType = TemplateContentType.Html,
							Path = "index.html",
							Template = "Index.cshtml"
						}
					}
				}
			},
			Verbose = true
		};

		// REAL FS
		var storage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeIThumbnailService(),
			new FakeImageOptimisationService());

		// Write to actual Disk

		var profiles = new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger())
			.GetPublishProfileName("default");

		var outputFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
			"output_test_1234");
		storage.CreateDirectory(outputFolderPath);

		var output = await service.GenerateWebHtml(profiles,
			profiles.FirstOrDefault()!, "testItem", new string[1],
			new List<FileIndexItem> { new("test") },
			outputFolderPath
		);

		return ( service, outputFolderPath, output, storage );
	}

	/// <summary>
	///     Test GenerateZip
	/// </summary>
	/// <returns></returns>
	[TestMethod]
	public async Task GenerateZip_GenerateWebHtml_RealFsTest()
	{
		var (service, outputFolderPath, output, storage) =
			await GenerateZipCreateSut();

		await service.GenerateZip(outputFolderPath, "output_test_1234",
			output);

		// delete the folder
		Assert.IsFalse(storage.ExistFolder(Path.Combine(new AppSettings().TempFolder,
			"output_test_1234")));

		// This test creates a .done file in the temp folder
		// git/starsky/starsky/starsky/bin/Debug/net8.0/temp/output_test_1234.done

		Assert.IsTrue(storage.ExistFile(Path.Combine(outputFolderPath, "output_test_1234.zip")));

		// this realFS
		storage.FolderDelete(outputFolderPath);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	[DataRow(true)]
	[DataRow(false)]
	public async Task GenerateJpeg_ResizerLocal_ImageOptimisationThrows__UnixOnly(bool optimizerFailsBashScript)
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		// Use real filesystem storage so we can create a non-executable mozjpeg file
		var storage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		var selectorStorage = new FakeSelectorStorage(storage);
		var logger = new FakeIWebLogger();

		// Prepare appsettings with dependencies folder inside a temp folder
		var tempBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_deps_mozjpeg");
		if ( Directory.Exists(tempBase) )
		{
			Directory.Delete(tempBase, true);
		}

		Directory.CreateDirectory(tempBase);

		var appSettings = new AppSettings
		{
			DependenciesFolder = tempBase, StorageFolder = new CreateAnImage().BasePath
		};

		File.Delete(new CreateAnImage().FullFilePath.Replace(".jpg", "_temp.jpg"));
		File.Copy(new CreateAnImage().FullFilePath,
			new CreateAnImage().FullFilePath.Replace(".jpg", "_temp.jpg"));

		// Create a dummy mozjpeg file without +x permissions (Unix) to trigger permission denied
		var exePath = new ImageOptimisationExePath(appSettings).GetExePath("mozjpeg",
			CurrentArchitecture.GetCurrentRuntimeIdentifier());
		var parentFolder = Path.GetDirectoryName(exePath)!;
		Directory.CreateDirectory(parentFolder);

		// Write a small file and intentionally do NOT set executable bit
		if ( optimizerFailsBashScript )
		{
			await File.WriteAllLinesAsync(exePath, [""],
				TestContext.CancellationToken);
		}
		else
		{
			await File.WriteAllLinesAsync(exePath, ["#!/bin/bash\necho -ne '\\xFF\\xD8\\xFF\\xE0'"],
				TestContext.CancellationToken);
		}

		// Ensure it's not executable on unix systems
		if ( !appSettings.IsWindows )
		{
			// remove all execute bits if any (best-effort)
			try
			{
				var fi = new FileInfo(exePath);
				fi.Attributes &= ~FileAttributes.ReadOnly;
				// chmod 0644
				var proc = Process.Start(new ProcessStartInfo
				{
					FileName = "/bin/chmod",
					Arguments = "644 " + exePath,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				});
				await proc?.WaitForExitAsync(TestContext.CancellationToken)!;
			}
			catch
			{
				// best effort, continue
			}
		}

		var fakeSelectorStorage = new FakeSelectorStorageByType(
			new StorageSubPathFilesystem(appSettings, logger), storage, new FakeIStorage(),
			new FakeIStorage());

		// Use real MozJpegService but fake the download as already OK
		var download = new FakeMozJpegDownload(ImageOptimisationDownloadStatus.Ok)
		{
			FixPermissionsDelegate =
				new ImageOptimisationChmod(
						new FakeSelectorStorage(new StorageHostFullPathFilesystem(logger)), logger)
					.Chmod
		};

		var mozService = new MozJpegService(appSettings, new FakeSelectorStorage(storage), logger,
			download);

		// Use ImageOptimisationService that will call MozJpegService
		var optimisationService = new ImageOptimisationService(appSettings,
			new FakeSelectorStorage(storage), logger, mozService);

		// Use WebHtmlPublishService with the real optimisation service via dependency
		var publishProfile = new AppSettingsPublishProfiles
		{
			ContentType = TemplateContentType.Jpeg,
			Path = "index.html",
			SourceMaxWidth = 1001,
			// enable optimizer
			MetaData = false
		};

		appSettings.PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
		{
			{ "default", [publishProfile] }
		};

		// create an optimizer that matches jpg and is enabled
		appSettings.PublishProfilesDefaults.Optimizers =
		[
			new Optimizer
			{
				Enabled = true,
				Id = "mozjpeg",
				ImageFormats =
					[ExtensionRolesHelper.ImageFormat.jpg],
				Options = new OptimizerOptions { Quality = 80 }
			}
		];

		var photoPath = new CreateAnImage().FileName.Replace(".jpg", "_temp.jpg");
		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				new ConsoleWrapper(), fakeSelectorStorage
				, new FakeIWebLogger()),
			selectorStorage, appSettings,
			new FakeExifTool(storage, appSettings), new FakeIOverlayImage(selectorStorage),
			new ConsoleWrapper(), logger,
			new FakeIThumbnailService(selectorStorage), optimisationService);

		var profiles = new PublishPreflight(appSettings,
				new ConsoleWrapper(), new FakeSelectorStorage(storage), new FakeIWebLogger())
			.GetPublishProfileName("default");

		const string fileHash = "BA65AKADKJK7X7JCOGYADPPHF4";
		var item = new FileIndexItem(photoPath)
		{
			FileHash = fileHash,
			FilePath = photoPath,
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
			Status = FileIndexItem.ExifStatus.Ok
		};

		await service.GenerateJpeg(profiles.FirstOrDefault()!,
			new List<FileIndexItem> { item },
			appSettings.StorageFolder, 1);

		if ( optimizerFailsBashScript )
		{
			Assert.Contains(
				p =>
					p.Item2!.Contains("[ImageOptimisationService] MozJPEG failed to run"),
				logger.TrackedExceptions);
			Assert.DoesNotContain( // NOT CONTAIN
				p =>
					p.Item2!.Contains("[ImageOptimisationService] MozJPEG optimized"),
				logger.TrackedInformation);
		}
		else
		{
			Assert.Contains(
				p =>
					p.Item2!.Contains("[ImageOptimisationService] MozJPEG optimized"),
				logger.TrackedInformation);
			Assert.DoesNotContain( // NOT CONTAIN
				p =>
					p.Item2!.Contains("[ImageOptimisationService] MozJPEG failed to run"),
				logger.TrackedExceptions);
		}


		File.Delete(new CreateAnImage().FullFilePath.Replace(".jpg", "_temp.jpg"));
		Directory.Delete(tempBase, true);
	}
}
