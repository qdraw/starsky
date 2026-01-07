using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.cloudimport;
using starsky.feature.cloudimport.Services;
using starsky.foundation.import.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.cloudimport.Services;

[TestClass]
public class CloudImportServiceTest
{
	[TestMethod]
	public async Task SyncAsync_WhenDisabled_ShouldReturnErrorResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new() { Id = "test", Enabled = false }
				}
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
		Assert.IsTrue(result.Errors.Any(e => e.Contains("disabled")));
	}

	[TestMethod]
	public async Task SyncAsync_WhenConnectionFails_ShouldReturnErrorResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new()
					{
						Id = "test",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/test"
					}
				}
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
		Assert.IsTrue(result.Errors.Any(e => e.Contains("Failed to connect")));
	}

	[TestMethod]
	public async Task SyncAsync_WithValidFiles_ShouldImportSuccessfully()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new()
					{
						Id = "test",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/photos",
						DeleteAfterImport = false
					}
				}
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClient
		{
			FilesToReturn = new List<CloudFile>
			{
				new()
				{
					Id = "1",
					Name = "photo1.jpg",
					Path = "/photos/photo1.jpg",
					Size = 1024,
					Hash = "abc123"
				},
				new()
				{
					Id = "2",
					Name = "photo2.jpg",
					Path = "/photos/photo2.jpg",
					Size = 2048,
					Hash = "def456"
				}
			}
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
				Providers = new List<CloudImportProviderSettings>
				{
					new()
					{
						Id = "test",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/photos",
						DeleteAfterImport = true
					}
				}
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClient
		{
			FilesToReturn = new List<CloudFile>
			{
				new()
				{
					Id = "1",
					Name = "photo1.jpg",
					Path = "/photos/photo1.jpg",
					Size = 1024,
					Hash = "abc123"
				}
			}
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
				Providers = new List<CloudImportProviderSettings>
				{
					new()
					{
						Id = "test",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/photos",
						DeleteAfterImport = true
					}
				}
			}
		};
		var logger = new FakeIWebLogger();
		var serviceCollection = new ServiceCollection();
		var fakeClient = new FakeCloudImportClient
		{
			FilesToReturn = new List<CloudFile>
			{
				new()
				{
					Id = "1",
					Name = "corrupted.jpg",
					Path = "/photos/corrupted.jpg",
					Size = 1024,
					Hash = "abc123"
				}
			}
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
		await Task.Delay(50); // Give time for the first sync to start and block

		// Act - Try to start second sync while first is running
		await service.SyncAsync("test", CloudImportTriggerType.Manual);

		// Unblock the first sync
		syncBlocker.Set();
		await task1;


		// Assert
		Assert.IsTrue(logger.TrackedExceptions.Any(e =>
			e.Item2!.Contains("already in progress")));
	}

	[TestMethod]
	public void IsSyncInProgress_WhenNotRunning_ShouldReturnFalse()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new() { Id = "test", Enabled = true }
				}
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
				Providers = new List<CloudImportProviderSettings>
				{
					new()
					{
						Id = "provider1",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/folder1"
					},
					new()
					{
						Id = "provider2",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/folder2"
					}
				}
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
		Assert.IsTrue(results.Any(r => r.ProviderId == "provider1"));
		Assert.IsTrue(results.Any(r => r.ProviderId == "provider2"));
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
				Providers = new List<CloudImportProviderSettings>
				{
					new()
					{
						Id = "provider1",
						Enabled = false,
						Provider = "FakeProvider",
						RemoteFolder = "/folder1"
					}
				}
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
				Providers = new List<CloudImportProviderSettings>
				{
					new()
					{
						Id = "provider1",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/folder1"
					},
					new()
					{
						Id = "provider2",
						Enabled = true,
						Provider = "FakeProvider",
						RemoteFolder = "/folder2"
					}
				}
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
		Assert.IsTrue(results.Any(r =>
			r.ProviderId == "provider1" && r.Errors.Any(e => e.Contains("Sync failed"))));
		Assert.IsTrue(results.Any(r => r.ProviderId == "provider2"));
	}
}
