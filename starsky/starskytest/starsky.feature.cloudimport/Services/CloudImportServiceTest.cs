using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.cloudimport;
using starsky.feature.cloudimport.Clients;
using starsky.feature.cloudimport.Services;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.cloudimport.Services;

[TestClass]
public class CloudImportServiceTest
{
	public TestContext TestContext { get; set; }


	[TestMethod]
	public void LastSyncResults()
	{
		var service = new CloudImportService(null!, null!, null!);
		Assert.IsEmpty(service.LastSyncResults);
	}

	[TestMethod]
	public async Task SyncAsync_WhenDisabled_ShouldReturnErrorResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
					[new CloudImportProviderSettings { Id = "test", Enabled = false }]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClient();
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var result = await service.SyncAsync("test", CloudImportTriggerType.Manual);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.Contains(e => e.Contains("disabled"), result.Errors);
	}

	[TestMethod]
	public async Task SyncAsync_WhenConnectionFails_ShouldReturnErrorResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "test",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/test"
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClient { ShouldTestConnectionSucceed = false };
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var result = await service.SyncAsync("test", CloudImportTriggerType.Scheduled);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.Contains(e => e.Contains("Failed to connect"), result.Errors);
	}

	[TestMethod]
	public async Task SyncAsync_WithValidFiles_ShouldImportSuccessfully()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "test",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/photos",
						DeleteAfterImport = false
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClient
		{
			FilesToReturn =
			[
				new CloudFile
				{
					Id = "1",
					Name = "photo1.jpg",
					Path = "/photos/photo1.jpg",
					Size = 1024,
					Hash = "abc123"
				},

				new CloudFile
				{
					Id = "2",
					Name = "photo2.jpg",
					Path = "/photos/photo2.jpg",
					Size = 2048,
					Hash = "def456"
				}
			]
		};
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ =>
			new FakeIImport(
				new FakeSelectorStorage(new StorageHostFullPathFilesystem(new FakeIWebLogger()))));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var result = await service.SyncAsync("test", CloudImportTriggerType.Manual);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(2, result.FilesFound);
		Assert.AreEqual(2, result.FilesImportedSuccessfully);
		Assert.AreEqual(0, result.FilesFailed);
		Assert.IsEmpty(fakeClient.DeletedFiles); // DeleteAfterImport is false
	}

	[TestMethod]
	public async Task SyncAsync_WithDeleteAfterImport_ShouldDeleteSuccessfulFiles()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "test",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/photos",
						DeleteAfterImport = true
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClient
		{
			FilesToReturn =
			[
				new CloudFile
				{
					Id = "1",
					Name = "photo1.jpg",
					Path = "/photos/photo1.jpg",
					Size = 1024,
					Hash = "abc123"
				}
			]
		};
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ =>
			new FakeIImport(
				new FakeSelectorStorage(
					new StorageHostFullPathFilesystem(new FakeIWebLogger()))));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var result = await service.SyncAsync("test", CloudImportTriggerType.Scheduled);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.AreEqual(1, result.FilesImportedSuccessfully);
		Assert.HasCount(1, fakeClient.DeletedFiles);
		Assert.AreEqual("photo1.jpg", fakeClient.DeletedFiles[0].Name);
	}

	[TestMethod]
	public async Task SyncAsync_WhenImportFails_ShouldNotDeleteFile()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "test",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/photos",
						DeleteAfterImport = true
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClient
		{
			FilesToReturn =
			[
				new CloudFile
				{
					Id = "1",
					Name = "corrupted.jpg",
					Path = "/photos/corrupted.jpg",
					Size = 1024,
					Hash = "abc123"
				}
			]
		};
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ =>
			new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var result = await service.SyncAsync("test", CloudImportTriggerType.Manual);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.AreEqual(1, result.FilesFailed);
		Assert.IsEmpty(fakeClient.DeletedFiles); // Should not delete failed import
	}

	[TestMethod]
	public async Task SyncAsync_PreventsConcurrentExecution()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "test",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/photos"
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var syncBlocker = new ManualResetEventSlim(false);
		var fakeClient = new FakeCloudImportClient
		{
			FilesToReturn =
			[
				new CloudFile
				{
					Id = "1",
					Name = "photo1.jpg",
					Path = "/photos/photo1.jpg",
					Size = 1024,
					Hash = "abc123"
				}
			],
			SyncBlocker = syncBlocker
		};
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act - Start first sync (will block)
		var task1 = Task.Run(async () =>
			await service.SyncAsync("test", CloudImportTriggerType.Manual));

		// Ensure the first sync is running and blocked
		await Task.Delay(50,
			TestContext.CancellationToken); // Give time for the first sync to start and block

		// Act - Try to start second sync while first is running
		await service.SyncAsync("test", CloudImportTriggerType.Manual);

		// Unblock the first sync
		syncBlocker.Set();
		await task1;


		// Assert
		Assert.Contains(e =>
			e.Item2!.Contains("already in progress"), logger.TrackedExceptions);
	}

	[TestMethod]
	public void IsSyncInProgress_WhenNotRunning_ShouldReturnFalse()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
					[new CloudImportProviderSettings { Id = "test", Enabled = true }]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped<ICloudImportClient>(_ => new FakeCloudImportClient());
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Assert
		Assert.IsFalse(service.IsSyncInProgress);
	}

	[TestMethod]
	public async Task SyncAllAsync_WithMultipleProviders_ShouldReturnResultsForEach()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "provider1",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/folder1"
					},

					new CloudImportProviderSettings
					{
						Id = "provider2",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/folder2"
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped<ICloudImportClient>(_ => new FakeCloudImportClient());
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var results = await service.SyncAllAsync(CloudImportTriggerType.Manual);

		// Assert
		Assert.HasCount(2, results);
		Assert.Contains(r => r.ProviderId == "provider1", results);
		Assert.Contains(r => r.ProviderId == "provider2", results);
		Assert.IsTrue(results.All(r => r.Success));
	}

	[TestMethod]
	public async Task SyncAllAsync_NoEnabledProviders_ShouldReturnEmptyList()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "provider1",
						Enabled = false,
						Provider = "FakeProvider",
						RemoteFolder = "/folder1"
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped<ICloudImportClient>(_ => new FakeCloudImportClient());
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var results = await service.SyncAllAsync(CloudImportTriggerType.Manual);

		// Assert
		Assert.IsEmpty(results);
	}

	[TestMethod]
	public async Task SyncAllAsync_WhenProviderThrowsException_ShouldReturnErrorResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "provider1",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/folder1"
					},

					new CloudImportProviderSettings
					{
						Id = "provider2",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/folder2"
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		// provider1: throws, provider2: works
		serviceCollection.AddScoped<ICloudImportClient>(sp =>
			sp.GetService<AppSettings>()?.CloudImport?.Providers[0].Id == "provider1"
				? throw new Exception("Test exception")
				: new FakeCloudImportClient());
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		serviceCollection.AddSingleton(appSettings);
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var results = await service.SyncAllAsync(CloudImportTriggerType.Manual);

		// Assert
		Assert.HasCount(2, results);
		Assert.Contains(r =>
			r.ProviderId == "provider1" && r.Errors.Any(e => e.Contains("Sync failed")), results);
		Assert.Contains(r => r.ProviderId == "provider2", results);
	}

	[TestMethod]
	public async Task SyncAsync_WhenCloudClientNotEnabled_ShouldReturnErrorResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "test", Enabled = true, Provider = "FakeProvider"
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		// Fake client with Enabled = false
		var fakeClient = new FakeCloudImportClient { Enabled = false };
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var result = await service.SyncAsync("test", CloudImportTriggerType.Manual);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.Contains(e =>
			e.Contains("not available") || e.Contains("not enabled"), result.Errors);
	}
	
	[TestMethod]
	public async Task SyncAsync_WhenCloudClientNotFound_ShouldReturnErrorResult()
	{
		// Arrange
		var appSettings = new AppSettings();
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClient { Enabled = false };
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var result = await service.SyncAsync("not-found", CloudImportTriggerType.Manual);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.Contains(e => e.Contains("not found"), result.Errors);
	}

	[TestMethod]
	public async Task SyncAsync_WhenCloudClientIsDropboxCloudImportClient_ShouldInitializeClient()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "dropbox-test",
						Enabled = true,
						Provider = "Dropbox",
						RemoteFolder = "/dropbox-folder",
						Credentials = new CloudProviderCredentials
						{
							RefreshToken = "refresh-token",
							AppKey = "app-key",
							AppSecret = "app-secret"
						}
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeDropboxClient = new DropboxCloudImportClient(new FakeIWebLogger(),
			new AppSettings(), new FakeDropboxCloudImportRefreshToken());
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeDropboxClient);
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var result = await service.SyncAsync("dropbox-test", CloudImportTriggerType.Manual);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.Contains(e =>
			e.Contains("not available") || e.Contains("not enabled"), result.Errors);
	}

	[TestMethod]
	public async Task GetCloudFiles_WhenListFilesAsyncThrowsException_ShouldReturnErrorResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Id = "test", Enabled = true, Provider = "FakeProvider"
					}
				]
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClientWithException();
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ => new FakeIImport(new FakeSelectorStorage()));
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);
		var providerSettings = appSettings.CloudImport.Providers[0];
		var result = new CloudImportResult();
		var (_, errorResult) = await service.GetCloudFiles(fakeClient, result,
			providerSettings, providerSettings.Id);

		// Assert
		Assert.IsNotNull(errorResult);
		Assert.Contains(e =>
			e.Contains("Failed to list files from cloud storage"), errorResult.Errors);
	}

	[TestMethod]
	public async Task ProcessFileLoopAsync_WhenProcessFileThrowsException_ShouldAddError()
	{
		// Arrange
		var appSettings = new AppSettings();
		var logger = new FakeIWebLogger();
		var fakeClient = new FakeCloudImportClientThrowsOnProcess();
		var fakeImport = new FakeIImport(new FakeSelectorStorage());
		var files = new List<CloudFile>
		{
			new() { Id = "1", Name = "file1.jpg", Path = "/file1.jpg", Size = 100 }
		};
		var tempFolder = Path.GetTempPath();
		var result = new CloudImportResult();
		var providerSettings = new CloudImportProviderSettings();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped<ICloudImportClient>(_ => fakeClient);
		serviceCollection.AddScoped<IImport>(_ => fakeImport);
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		await service.ProcessFileLoopAsync(fakeClient, fakeImport, files, tempFolder, result,
			providerSettings);

		// Assert
		Assert.AreEqual(1, result.FilesFailed);
		Assert.Contains(e => e.Contains("Download failed"), result.Errors);
	}

	[TestMethod]
	public async Task Import_SuccessfulImport_ShouldUpdateResultAndReturnTrue()
	{
		// Arrange
		var providerSettings = new CloudImportProviderSettings { Id = "provider1" };
		var result = new CloudImportResult();
		var file = new CloudFile { Name = "file1.jpg" };
		const string fileKey = "file1.jpg";
		const string localPath = "/tmp/file1.jpg";
		var importIndexItem = new ImportIndexItem { Status = ImportStatus.Ok };
		var fakeImport = new FakeIImportForImportTest
		{
			ImporterFunc = (_, _) => [importIndexItem]
		};
		var appSettings = new AppSettings();
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped<IImport>(_ => fakeImport);
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var success = await service.Import(providerSettings, fakeImport, result, localPath, file,
			fileKey);

		// Assert
		Assert.IsTrue(success);
		Assert.AreEqual(1, result.FilesImportedSuccessfully);
		Assert.Contains(file.Name, result.SuccessfulFiles);
	}

	[TestMethod]
	public async Task Import_ImportFails_ShouldUpdateResultAndReturnFalse()
	{
		// Arrange
		var providerSettings = new CloudImportProviderSettings { Id = "provider1" };
		var result = new CloudImportResult();
		var file = new CloudFile { Name = "file2.jpg" };
		var fileKey = "file2.jpg";
		var localPath = "/tmp/file2.jpg";
		var importIndexItem = new ImportIndexItem { Status = ImportStatus.FileError }; // Not Ok
		var fakeImport = new FakeIImportForImportTest
		{
			ImporterFunc = (_, _) => [importIndexItem]
		};
		var appSettings = new AppSettings();
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped<IImport>(_ => fakeImport);
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var success = await service.Import(providerSettings, fakeImport,
			result, localPath, file, fileKey);

		// Assert
		Assert.IsFalse(success);
		Assert.AreEqual(1, result.FilesFailed);
		Assert.Contains(file.Name, result.FailedFiles);
		Assert.Contains(e => e.Contains("Import failed"), result.Errors);
	}

	[TestMethod]
	public async Task Import_WhenImporterThrowsException_ShouldUpdateResultAndReturnFalse()
	{
		// Arrange
		var providerSettings = new CloudImportProviderSettings { Id = "provider1" };
		var result = new CloudImportResult();
		var file = new CloudFile { Name = "file3.jpg" };
		const string fileKey = "file3.jpg";
		const string localPath = "/tmp/file3.jpg";
		var fakeImport = new FakeIImportForImportTest { ThrowOnImport = true };
		var appSettings = new AppSettings();
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped<IImport>(_ => fakeImport);
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act
		var success = await service.Import(providerSettings, fakeImport,
			result, localPath, file, fileKey);

		// Assert
		Assert.IsFalse(success);
		Assert.AreEqual(1, result.FilesFailed);
		Assert.Contains(file.Name, result.FailedFiles);
		Assert.Contains(e => e.Contains("Import failed"), result.Errors);
	}

	[TestMethod]
	[DataRow("jpg", "file1.jpg;file2.JPG", "file1.jpg;file2.JPG")]
	[DataRow("png", "file1.jpg;file2.png", "file2.png")]
	[DataRow("jpg,png", "file1.jpg;file2.png;file3.JPG;file4.txt", "file1.jpg;file2.png;file3.JPG")]
	[DataRow("", "file1.jpg;file2.png", "file1.jpg;file2.png")]
	[DataRow("gif", "file1.jpg;file2.png", "")]
	[DataRow("JPG", "file1.jpg;file2.JPG", "file1.jpg;file2.JPG")]
	[DataRow(".JPG", "file1.jpg;file2.JPG", "file1.jpg;file2.JPG")]
	[DataRow("jpg", "", "")]
	public void FilterExtensions_Theory(string extensionsCsv, string fileNamesCsv,
		string expectedCsv)
	{
		var extensions = string.IsNullOrEmpty(extensionsCsv)
			? []
			: extensionsCsv.Split(',').ToList();
		var files = string.IsNullOrEmpty(fileNamesCsv)
			? []
			: fileNamesCsv.Split(';').Select(f => new CloudFile { Name = f }).ToList();
		var expected = string.IsNullOrEmpty(expectedCsv)
			? []
			: expectedCsv.Split(';').ToList();

		var providerSettings = new CloudImportProviderSettings { Extensions = extensions };
		var result = CloudImportService.FilterExtensions(files, providerSettings);
		var resultNames = result.Select(f => f.Name).ToList();
		CollectionAssert.AreEquivalent(expected, resultNames);
	}
}
