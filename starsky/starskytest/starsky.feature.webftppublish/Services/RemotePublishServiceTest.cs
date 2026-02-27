using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webftppublish.Services;

[TestClass]
public class RemotePublishServiceTest
{
	[TestMethod]
	public async Task IsValidZipOrFolder_DelegatesToFtpService()
	{
		var ftpService = new FakeIFtpService();
		var localFsService = new LocalFileSystemPublishService(
			new AppSettings(),
			new FakeIStorage(),
			new FakeSelectorStorage(),
			new FakeConsoleWrapper(),
			new FakeIWebLogger());

		var selector = new RemotePublishService(
			ftpService,
			localFsService,
			new AppSettings(),
			new FakeIWebLogger());

		var result = await selector.IsValidZipOrFolder("/test");

		Assert.IsNotNull(result);
		Assert.AreEqual("/test", ftpService.LastPath);
	}

	[TestMethod]
	public void Run_Ftp_DelegatesToFtpService()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote
			{
				Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
				{
					{
						"test-profile", [
							new RemoteCredentialWrapper
							{
								Type = RemoteCredentialType.Ftp,
								Ftp = new FtpCredential
								{
									WebFtp = "ftp://user:pass@example.com/path"
								}
							}
						]
					}
				}
			}
		};

		var ftpService = new FakeIFtpService();
		var localFsService = new LocalFileSystemPublishService(
			new AppSettings(),
			new FakeIStorage(),
			new FakeSelectorStorage(),
			new FakeConsoleWrapper(),
			new FakeIWebLogger());

		var selector = new RemotePublishService(
			ftpService,
			localFsService,
			appSettings,
			new FakeIWebLogger());

		var copyContent = new Dictionary<string, bool> { { "test.jpg", true } };
		var result = selector.Run("/test", "test-profile", "slug", copyContent);

		Assert.IsTrue(result);
		Assert.AreEqual("/test", ftpService.LastPath);
	}

	[TestMethod]
	public void Run_LocalFileSystem_DelegatesToLocalFsService()
	{
		var tempDir = Path.Combine(Path.GetTempPath(),
			"selector-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var appSettings = new AppSettings
			{
				PublishProfilesRemote = new AppSettingsPublishProfilesRemote
				{
					Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
					{
						{
							"test-profile", [
								new RemoteCredentialWrapper
								{
									Type = RemoteCredentialType.LocalFileSystem,
									LocalFileSystem = new LocalFileSystemCredential
									{
										Path = tempDir
									}
								}
							]
						}
					}
				}
			};

			var ftpService = new FakeIFtpService();
			var sourceStorage = new FakeIStorage(["/test"], ["/test/file.jpg"]);
			var localFsService = new LocalFileSystemPublishService(
				appSettings,
				sourceStorage,
				new FakeSelectorStorage(),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var selector = new RemotePublishService(
				ftpService,
				localFsService,
				appSettings,
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "file.jpg", true } };
			var result = selector.Run("/test", "test-profile", "slug", copyContent);

			Assert.IsTrue(result);
			// FtpService should not be called
			Assert.AreEqual(string.Empty, ftpService.LastPath);
			Assert.AreEqual(string.Empty, ftpService.LastSlug);
		}
		finally
		{
			if ( Directory.Exists(tempDir) )
			{
				Directory.Delete(tempDir, true);
			}
		}
	}

	[TestMethod]
	public void Run_UnknownType_ReturnsFalse()
	{
		var logger = new FakeIWebLogger();
		var ftpService = new FakeIFtpService();
		var localFsService = new LocalFileSystemPublishService(
			new AppSettings(),
			new FakeIStorage(),
			new FakeSelectorStorage(),
			new FakeConsoleWrapper(),
			new FakeIWebLogger());

		var appSettings = new AppSettings
		{
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote
			{
				Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
				{
					{
						"profile", [
							new RemoteCredentialWrapper
							{
								Type = RemoteCredentialType.Unknown,
								Ftp = null,
								LocalFileSystem = null
							}
						]
					}
				}
			}
		};

		var selector = new RemotePublishService(
			ftpService,
			localFsService,
			appSettings,
			logger);

		var copyContent = new Dictionary<string, bool> { { "test.jpg", true } };
		var result = selector.Run("/test", "profile", "slug", copyContent);

		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedExceptions.Exists(p =>
			p.Item2?.Contains("Unsupported remote credential type") == true));
	}

	[TestMethod]
	public void IsPublishEnabled_ProfileNotFound_ReturnsFalse()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote
			{
				Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>()
			},
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>()
		};

		var logger = new FakeIWebLogger();
		var service = new RemotePublishService(
			new FakeIFtpService(),
			new LocalFileSystemPublishService(
				appSettings,
				new FakeIStorage(),
				new FakeSelectorStorage(),
				new FakeConsoleWrapper(),
				logger),
			appSettings,
			logger);

		var result = service.IsPublishEnabled("missing-profile");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Run_ProfileNotFound_ReturnsFalse()
	{
		var appSettings = new AppSettings();

		var logger = new FakeIWebLogger();
		var service = new RemotePublishService(
			new FakeIFtpService(),
			new LocalFileSystemPublishService(
				appSettings,
				new FakeIStorage(),
				new FakeSelectorStorage(),
				new FakeConsoleWrapper(),
				logger),
			appSettings,
			logger);

		var result = service.Run("", "", "", new Dictionary<string, bool>());
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsPublishEnabled_ProfileEmptyValue_ReturnsFalse()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote
			{
				Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>()
			},
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{ "empty-profile", [] }
			}
		};

		var logger = new FakeIWebLogger();
		var service = new RemotePublishService(
			new FakeIFtpService(),
			new LocalFileSystemPublishService(
				appSettings,
				new FakeIStorage(),
				new FakeSelectorStorage(),
				new FakeConsoleWrapper(),
				logger),
			appSettings,
			logger);

		var result = service.IsPublishEnabled("empty-profile");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsPublishEnabled_ProfileValueNull_ReturnsFalseAndLogs()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote
			{
				Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>()
			},
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{ "null-profile", null! }
			}
		};

		var logger = new FakeIWebLogger();
		var service = new RemotePublishService(
			new FakeIFtpService(),
			new LocalFileSystemPublishService(
				appSettings,
				new FakeIStorage(),
				new FakeSelectorStorage(),
				new FakeConsoleWrapper(),
				logger),
			appSettings,
			logger);

		var result = service.IsPublishEnabled("null-profile");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsPublishEnabled_ProfileNoPublishRemote_ReturnsFalse()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote
			{
				Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>()
			},
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"profile", [
						new AppSettingsPublishProfiles { ContentType = TemplateContentType.Html },
						new AppSettingsPublishProfiles { ContentType = TemplateContentType.Jpeg }
					]
				}
			}
		};

		var logger = new FakeIWebLogger();
		var service = new RemotePublishService(
			new FakeIFtpService(),
			new LocalFileSystemPublishService(
				appSettings,
				new FakeIStorage(),
				new FakeSelectorStorage(),
				new FakeConsoleWrapper(),
				logger),
			appSettings,
			logger);

		var result = service.IsPublishEnabled("profile");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsPublishEnabled_ProfileHasPublishRemote_ReturnsTrue()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote
			{
				Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
				{
					{
						"profile", [
							new RemoteCredentialWrapper
							{
								Type = RemoteCredentialType.Ftp,
								Ftp = new FtpCredential { WebFtp = "ftp://user:test@example.com" }
							}
						]
					}
				}
			},
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{
					"profile", [
						new AppSettingsPublishProfiles
						{
							ContentType = TemplateContentType.PublishRemote
						},
						new AppSettingsPublishProfiles { ContentType = TemplateContentType.Html }
					]
				}
			}
		};

		var logger = new FakeIWebLogger();
		var service = new RemotePublishService(
			new FakeIFtpService(),
			new LocalFileSystemPublishService(
				appSettings,
				new FakeIStorage(),
				new FakeSelectorStorage(),
				new FakeConsoleWrapper(),
				logger),
			appSettings,
			logger);

		var result = service.IsPublishEnabled("profile");
		Assert.IsTrue(result);
	}
}
