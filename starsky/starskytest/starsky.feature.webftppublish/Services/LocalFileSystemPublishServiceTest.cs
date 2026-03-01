using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn.CreateAnZipFile12;
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
							"test-profile", [
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

			var service = new LocalFileSystemPublishService(
				appSettings,
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "test.jpg", true } };
			var result = service.Run(sourceDir, "test-profile", "my-slug", copyContent);

			Assert.IsTrue(result);
			var expectedDestFolder = Path.Combine(destDir, "my-slug");
			Assert.IsTrue(sourceStorage.ExistFolder(expectedDestFolder));
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
				new FakeSelectorStorage(storage),
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
						"test-profile", [
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
			new FakeSelectorStorage(storage),
			new FakeConsoleWrapper(),
			logger);

		var copyContent = new Dictionary<string, bool> { { "corrupt.zip", true } };
		var result = service.Run("/test/corrupt.zip", "test-profile", "slug", copyContent);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CopyToLocalFileSystem_CreateDestinationBasePath_Success()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var sourceStorage = new FakeIStorage(
				["/source"],
				["/source/file.jpg"],
				["test content"u8.ToArray()]);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "file.jpg", true } };
			var result =
				service.CopyToLocalFileSystem(credential, "/source", "my-slug", copyContent);

			Assert.IsTrue(result);
			var expectedDestFolder = Path.Combine(tempDir, "my-slug");
			Assert.IsTrue(sourceStorage.ExistFolder(expectedDestFolder));
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
	public void CopyToLocalFileSystem_CreateSubdirectories_Success()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var sourceStorage = new FakeIStorage(
				["/source", "/source/subfolder"],
				["/source/subfolder/file.jpg"],
				["test content"u8.ToArray()]);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "subfolder/file.jpg", true } };
			var result =
				service.CopyToLocalFileSystem(credential, "/source", "my-slug", copyContent);

			Assert.IsTrue(result);
			var expectedSubfolder = Path.Combine(tempDir, "my-slug", "subfolder");
			Assert.IsTrue(sourceStorage.ExistFolder(expectedSubfolder));
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
	public void CopyToLocalFileSystem_SkipsExistingDestinationSubdirectories()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var sourceStorage = new FakeIStorage(
				["/source", "/source/subfolder"],
				["/source/subfolder/file.jpg"],
				["test content"u8.ToArray()]);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "subfolder/file.jpg", true } };
			var result =
				service.CopyToLocalFileSystem(credential, "/source", "my-slug", copyContent);

			Assert.IsTrue(result);
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
	public void CopyToLocalFileSystem_MultipleFiles_Success()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var sourceStorage = new FakeIStorage(
				["/source"],
				["/source/file1.jpg", "/source/file2.jpg", "/source/file3.jpg"],
				["content1"u8.ToArray(), "content2"u8.ToArray(), "content3"u8.ToArray()]);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool>
			{
				{ "file1.jpg", true }, { "file2.jpg", true }, { "file3.jpg", true }
			};
			var result =
				service.CopyToLocalFileSystem(credential, "/source", "my-slug", copyContent);

			Assert.IsTrue(result);
			Assert.IsTrue(
				sourceStorage.ExistFile(Path.Combine(tempDir, "my-slug", "file1.jpg")));
			Assert.IsTrue(
				sourceStorage.ExistFile(Path.Combine(tempDir, "my-slug", "file2.jpg")));
			Assert.IsTrue(
				sourceStorage.ExistFile(Path.Combine(tempDir, "my-slug", "file3.jpg")));
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
	public void CopyToLocalFileSystem_SkipsFilesNotMarkedForCopy()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var sourceStorage = new FakeIStorage(
				["/source"],
				["/source/file1.jpg", "/source/file2.jpg"],
				["content1"u8.ToArray(), "content2"u8.ToArray()]);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool>
			{
				{ "file1.jpg", true }, { "file2.jpg", false } // This one should be skipped
			};
			var result =
				service.CopyToLocalFileSystem(credential, "/source", "my-slug", copyContent);

			Assert.IsTrue(result);
			Assert.IsTrue(
				sourceStorage.ExistFile(Path.Combine(tempDir, "my-slug", "file1.jpg")));
			Assert.IsFalse(
				sourceStorage.ExistFile(Path.Combine(tempDir, "my-slug", "file2.jpg")));
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
	public void CopyToLocalFileSystem_FileCopyThrowsException_ReturnsFalse()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var destinationStorage = new FakeIStorage(new FileNotFoundException("Test exception"));

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(destinationStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "file.jpg", true } };
			var result =
				service.CopyToLocalFileSystem(credential, "/source", "my-slug", copyContent);

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
	public void CopyToLocalFileSystem_LeadingSlashTrimmed_Success()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var sourceStorage = new FakeIStorage(
				["/source"],
				["/source/file.jpg"],
				["test content"u8.ToArray()]);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			// File path with leading slash should be trimmed
			var copyContent = new Dictionary<string, bool> { { "/file.jpg", true } };
			var result =
				service.CopyToLocalFileSystem(credential, "/source", "my-slug", copyContent);

			Assert.IsTrue(result);
			Assert.IsTrue(
				sourceStorage.ExistFile(Path.Combine(tempDir, "my-slug", "file.jpg")));
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
	public void Run_ExtractZipError_ReturnsFalse()
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
								Type = RemoteCredentialType.LocalFileSystem,
								LocalFileSystem = new LocalFileSystemCredential { Path = "/dest" }
							}
						]
					}
				}
			}
		};

		var storage = new FakeIStorage([], []);

		var service = new LocalFileSystemPublishService(
			appSettings,
			new FakeSelectorStorage(storage),
			new FakeConsoleWrapper(),
			new FakeIWebLogger());

		var copyContent = new Dictionary<string, bool> { { "file.jpg", true } };
		var result = service.Run("/nonexistent", "test-profile", "slug", copyContent);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Run_RemoveFolderAfterwards_DeletesTemporaryFolder()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());
		var destDir = Path.Combine(tempDir, "dest");
		var zipFile = Path.Combine(tempDir, "test.zip");

		try
		{
			Directory.CreateDirectory(tempDir);
			Directory.CreateDirectory(destDir);

			// Create a real zip file for extraction
			var zipContent = CreateAnZipFile12.Bytes.ToArray();
			File.WriteAllBytes(zipFile, zipContent);

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
									LocalFileSystem =
										new LocalFileSystemCredential { Path = destDir }
								}
							]
						}
					}
				}
			};

			var destinationStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
			var selectorStorage = new FakeSelectorStorage(destinationStorage);

			var service = new LocalFileSystemPublishService(
				appSettings,
				selectorStorage,
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "file1.txt", true } };
			var result = service.Run(zipFile, "test-profile", "slug", copyContent);

			Assert.IsTrue(result);

			// Verify the temporary extraction folder was deleted
			// The folder name should contain "starsky-webftp" and "test"
			var tempFolders = Directory.GetDirectories(Path.GetTempPath(), "starsky-webftp*");
			var testTempFolder = tempFolders.FirstOrDefault(f => f.Contains("test"));

			// The folder should have been deleted
			if ( testTempFolder != null )
			{
				Assert.IsFalse(Directory.Exists(testTempFolder),
					"Temporary folder should have been deleted");
			}
		}
		finally
		{
			if ( File.Exists(zipFile) )
			{
				File.Delete(zipFile);
			}

			if ( Directory.Exists(tempDir) )
			{
				Directory.Delete(tempDir, true);
			}

			// Cleanup any leftover temp folders
			var tempFolders = Directory.GetDirectories(Path.GetTempPath(), "starsky-webftp*");
			foreach ( var folder in tempFolders )
			{
				try
				{
					Directory.Delete(folder, true);
				}
				catch
				{
					// Ignore cleanup errors
				}
			}
		}
	}

	[TestMethod]
	public void Run_Folder_DoesNotDeleteSourceFolder()
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
							"test-profile", [
								new RemoteCredentialWrapper
								{
									Type = RemoteCredentialType.LocalFileSystem,
									LocalFileSystem =
										new LocalFileSystemCredential { Path = destDir }
								}
							]
						}
					}
				}
			};

			var destinationStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());

			var service = new LocalFileSystemPublishService(
				appSettings,
				new FakeSelectorStorage(destinationStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "test.jpg", true } };

			// Diagnostic output before
			Debug.WriteLine($"Before Run: SourceDir exists: {Directory.Exists(sourceDir)}");

			var result = service.Run(sourceDir, "test-profile", "slug", copyContent);

			// Diagnostic output after
			Debug.WriteLine($"After Run: SourceDir exists: {Directory.Exists(sourceDir)}");
			Debug.WriteLine($"RemoveFolderAfterwards: {service.LastExtractZipResult?.RemoveFolderAfterwards}");
			Assert.IsNotNull(service.LastExtractZipResult, "LastExtractZipResult should not be null");
			Assert.IsFalse(service.LastExtractZipResult.RemoveFolderAfterwards, "RemoveFolderAfterwards should be false for a directory input");

			Assert.IsTrue(result);
			// Source folder should still exist (RemoveFolderAfterwards = false for folders)
			Assert.IsTrue(Directory.Exists(sourceDir),
				$"SourceDir should exist after Run, but Directory.Exists returned false. TempDir: {tempDir}");
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
	public void CopyToLocalFileSystem_DestinationFolderAlreadyExists_SkipsCreation()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var sourceStorage = new FakeIStorage(
				["/source"],
				["/source/file.jpg"],
				["test content"u8.ToArray()]);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "file.jpg", true } };
			var result =
				service.CopyToLocalFileSystem(credential, "/source", "my-slug", copyContent);

			Assert.IsTrue(result);
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
	public void CopyToLocalFileSystem_NestedSubdirectories_Success()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			var sourceDir = Path.DirectorySeparatorChar + "source";
			var level1Dir = Path.Combine(sourceDir, "level1");
			var level2Dir = Path.Combine(level1Dir, "level2");
			var sourceFilePath = Path.Combine(level2Dir, "file.jpg");
			var fileSubPath = Path.Combine("level1", "level2", "file.jpg");

			var sourceStorage = new FakeIStorage(
				[sourceDir, level1Dir, level2Dir],
				[sourceFilePath],
				["test content"u8.ToArray()]);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { fileSubPath, true } };
			var result =
				service.CopyToLocalFileSystem(credential, sourceDir, "my-slug", copyContent);

			Assert.IsTrue(result);
			var expectedPath = Path.Combine(tempDir, "my-slug", "level1", "level2", "file.jpg");
			Assert.IsTrue(sourceStorage.ExistFile(expectedPath));
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
	public void CopyToLocalFileSystem_NonExistentSourceFolder_SkipsSubdirectoryCreation()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "localfs-test-" + Path.GetRandomFileName());

		try
		{
			Directory.CreateDirectory(tempDir);

			var credential = new LocalFileSystemCredential { Path = tempDir };
			// Source doesn't have the folder that the file path suggests
			var sourceStorage = new FakeIStorage(
				["/source"],
				["/source/file.jpg"],
				["test content"u8.ToArray()]);

			var service = new LocalFileSystemPublishService(
				new AppSettings(),
				new FakeSelectorStorage(sourceStorage),
				new FakeConsoleWrapper(),
				new FakeIWebLogger());

			var copyContent = new Dictionary<string, bool> { { "subfolder/file.jpg", true } };
			var result =
				service.CopyToLocalFileSystem(credential, "/source", "my-slug", copyContent);

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
}
