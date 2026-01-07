using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.cloudimport;
using starsky.feature.cloudimport.Services;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.platform.Models;
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
		serviceCollection.AddScoped<IImport>(_ => new FakeImport());
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
		serviceCollection.AddScoped<IImport>(_ => new FakeImport());
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
		serviceCollection.AddScoped<IImport>(_ => new FakeImport());
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
		serviceCollection.AddScoped<IImport>(_ => new FakeImport());
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
			new FakeImport { ImportStatusFunc = _ => ImportStatus.FileError });
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
		serviceCollection.AddScoped<IImport>(_ => new FakeImport());
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Act - Start first sync
		var task1 = Task.Run(async () =>
			await service.SyncAsync("test", CloudImportTriggerType.Manual));

		// Act - Try to start second sync while first is running
		await service.SyncAsync("test", CloudImportTriggerType.Manual);
		await task1;

		await Task.Delay(10, CancellationToken.None);

		// Assert
		Assert.IsTrue(logger.TrackedExceptions.Any(e => e.Item2!.Contains("already in progress")));
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
		serviceCollection.AddScoped<IImport>(_ => new FakeImport());
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var service = new CloudImportService(scopeFactory, logger, appSettings);

		// Assert
		Assert.IsFalse(service.IsSyncInProgress);
	}

	private sealed class FakeCloudImportClient : ICloudImportClient
	{
		public List<CloudFile> FilesToReturn { get; set; } = new();
		public bool ShouldTestConnectionSucceed { get; set; } = true;
		public List<CloudFile> DeletedFiles { get; } = new();
		public Dictionary<string, string> DownloadedFiles { get; } = new();
		public string Name => "FakeProvider";
		public bool Enabled { get; } = true;

		public Task<List<CloudFile>> ListFilesAsync(string remoteFolder)
		{
			return Task.FromResult(FilesToReturn);
		}

		public Task<string> DownloadFileAsync(CloudFile file, string localFolder)
		{
			var localPath = Path.Combine(localFolder, file.Name);
			DownloadedFiles[file.Name] = localPath;
			// Create a dummy file
			Directory.CreateDirectory(localFolder);
			File.WriteAllText(localPath, "fake content");
			return Task.FromResult(localPath);
		}

		public Task<bool> DeleteFileAsync(CloudFile file)
		{
			DeletedFiles.Add(file);
			return Task.FromResult(true);
		}

		public Task<bool> TestConnectionAsync()
		{
			return Task.FromResult(ShouldTestConnectionSucceed);
		}
	}

	private sealed class FakeImport : IImport
	{
		public Func<List<string>, ImportStatus>? ImportStatusFunc { get; set; }

		public Task<List<ImportIndexItem>> Preflight(List<string> fullFilePathsList,
			ImportSettingsModel importSettings)
		{
			return Task.FromResult(new List<ImportIndexItem>());
		}

		public Task<List<ImportIndexItem>> Importer(IEnumerable<string> inputFullPathList,
			ImportSettingsModel importSettings)
		{
			var list = inputFullPathList.ToList();
			var status = ImportStatusFunc?.Invoke(list) ?? ImportStatus.Ok;
			return Task.FromResult(list.Select(path => new ImportIndexItem
			{
				Status = status, FilePath = path
			}).ToList());
		}
	}
}
