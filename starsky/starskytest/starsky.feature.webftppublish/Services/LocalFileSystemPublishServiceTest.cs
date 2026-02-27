using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webftppublish.Services;

[TestClass]
public class LocalFileSystemPublishServiceTest
{
	[TestMethod]
	public void Run_Success()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());
		var sourceDir = Path.Combine(tempDir, "source");
		var destDir = Path.Combine(tempDir, "dest");

		try
		{
			Directory.CreateDirectory(sourceDir);
			Directory.CreateDirectory(destDir);
			File.WriteAllText(Path.Combine(sourceDir, "test.jpg"), "test content");

			var appSettings = new AppSettings
			{
				PublishProfilesRemote = new AppSettingsPublishProfilesRemote
				{
					Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
					{
						{
							"test-profile",
							[
								new RemoteCredentialWrapper
								{
									Type = RemoteCredentialType.LocalFileSystem,
									LocalFileSystem = new LocalFileSystemCredential
									{
										Path = destDir
									}
								}
							]
						}
					}
				}
			};

			var sourceStorage = new FakeIStorage(
				[sourceDir],
				[Path.Combine(sourceDir, "test.jpg")],
				["test content"u8.ToArray()]);
			var destinationStorage = new FakeIStorage();

			var service = new LocalFileSystemPublishService(
				appSettings,
				sourceStorage,
				new FakeSelectorStorage(destinationStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "test.jpg", true } };
			var result = service.Run(sourceDir, "test-profile", "my-slug", copyContent);

			Assert.IsTrue(result);
			var expectedDestFolder = Path.Combine(destDir, "my-slug");
			Assert.IsTrue(destinationStorage.ExistFolder(expectedDestFolder));
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
	public void Run_NoSettings_ReturnsFalse()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote()
		};

		var storage = new FakeIStorage(["/test"], ["/test/file.jpg"]);
		var logger = new FakeIWebLogger();

		var service = new LocalFileSystemPublishService(
			appSettings,
			storage,
			new FakeSelectorStorage(storage),
			new FakeConsoleWrapper(),
			logger);

		var copyContent = new Dictionary<string, bool> { { "file.jpg", true } };
		var result = service.Run("/test", "nonexistent-profile", "slug", copyContent);

		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedExceptions.Exists(p =>
			p.Item2?.Contains("No local file system settings found") == true));
	}

	[TestMethod]
	public void CopyToLocalFileSystem_SourceFileNotFound_ReturnsFalse()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var storage = new FakeIStorage([], []);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				storage,
				new FakeSelectorStorage(),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "missing.jpg", true } };
			var result = service.CopyToLocalFileSystem(credential, "/source", "slug", copyContent);

			Assert.IsFalse(result);
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
	public void Run_CorruptZipInput_ReturnsFalse()
	{
		var appSettings = new AppSettings
		{
			PublishProfilesRemote = new AppSettingsPublishProfilesRemote
			{
				Profiles = new Dictionary<string, List<RemoteCredentialWrapper>>
				{
					{
						"test-profile",
						[
							new RemoteCredentialWrapper
							{
								Type = RemoteCredentialType.LocalFileSystem,
								LocalFileSystem = new LocalFileSystemCredential { Path = "/dest" }
							}
						]
					}
				}
			}
		};

		var storage = new FakeIStorage(["/test"], ["/test/corrupt.zip"]);
		var logger = new FakeIWebLogger();


		var service = new LocalFileSystemPublishService(
			appSettings,
			storage,
			new FakeSelectorStorage(storage),
			new FakeConsoleWrapper(),
			logger);

		var copyContent = new Dictionary<string, bool> { { "corrupt.zip", true } };
		var result = service.Run("/test/corrupt.zip", "test-profile", "slug", copyContent);

		Assert.IsFalse(result);
	}

}
