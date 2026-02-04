using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.cloudimport;
using starsky.feature.cloudimport.Clients;
using starsky.feature.cloudimport.Clients.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.cloudimport;

public class FakeCloudFileEntry
{
	public bool IsFile { get; set; }
	public FakeCloudFileMetadata AsFile { get; set; } = new();
}

public class FakeCloudFileMetadata
{
	public string Id { get; set; } = "id";
	public string Name { get; set; } = "file.txt";
	public string? PathDisplay { get; set; } = "/file.txt";
	public string? PathLower { get; set; } = "/file.txt";
	public long Size { get; set; } = 123;
	public DateTimeOffset ServerModified { get; set; } = DateTimeOffset.UtcNow;
}

public class FakeDropboxCloudImportRefreshToken : IDropboxCloudImportRefreshToken
{
	public Task<(string, int)> ExchangeRefreshTokenAsync(string refreshToken, string appKey,
		string appSecret)
	{
		return Task.FromResult(( "fake-access-token", 3600 ));
	}
}

[TestClass]
public class DropboxCloudImportClientTest
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task InitializeClient_UsesFactoryAndSetsClient()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		var fakeClient = new FakeIDropboxClient(fakeFiles);
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);

		var result = await client.InitializeClient("refresh", "key", "secret");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Name_eq_Dropbox()
	{
		var client = new DropboxCloudImportClient(
			null!,
			null!,
			null!,
			_ => null!
		);
		Assert.AreEqual("Dropbox", client.Name);
	}

	[TestMethod]
	public async Task ListFilesAsync_CallsFilesListFolderAsync()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		fakeFiles.Entries.Add(new FakeCloudFileEntry
		{
			IsFile = true, AsFile = new FakeCloudFileMetadata()
		});
		var fakeClient = new FakeIDropboxClient(fakeFiles);
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		await client.InitializeClient("refresh", "key", "secret");

		await client.ListFilesAsync("/test");

		Assert.Contains("/test", fakeFiles.ListFolderCalledWith);
	}

	[TestMethod]
	public async Task EnsureClient_ThrowsIfNotInitialized()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => null!
		);
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
		{
			await client.ListFilesAsync("/test");
		});
	}

	[TestMethod]
	[DataRow("Dropbox", "token", true)]
	[DataRow("Other", "token", false)]
	[DataRow("Dropbox", null, false)]
	public void Enabled_Theory(string provider, string refreshToken, bool expected)
	{
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings
					{
						Provider = provider,
						Credentials = new CloudProviderCredentials
						{
							RefreshToken = refreshToken
						}
					}
				]
			}
		};
		var client = new DropboxCloudImportClient(
			new FakeIWebLogger(),
			appSettings,
			new FakeDropboxCloudImportRefreshToken()
		);
		Assert.AreEqual(expected, client.Enabled);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task ListFilesAsync_ReturnsAllFiles_WhenHasMoreIsTrue()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();

		// Setup fake files and pagination
		var fakeFiles = new FakeFilesUserRoutes();
		// First page
		fakeFiles.Entries.Add(new FakeCloudFileEntry
		{
			IsFile = true, AsFile = new FakeCloudFileMetadata { Id = "1", Name = "file1.txt" }
		});
		fakeFiles.HasMore = true;
		fakeFiles.Cursor = "cursor1";
		// Second page
		var fakeFiles2 = new FakeFilesUserRoutes();
		fakeFiles2.Entries.Add(new FakeCloudFileEntry
		{
			IsFile = true, AsFile = new FakeCloudFileMetadata { Id = "2", Name = "file2.txt" }
		});
		fakeFiles2.HasMore = false;
		fakeFiles2.Cursor = "cursor2";

		// Setup fake client to return first then second page
		var callCount = 0;
		var fakeClient = new FakeIDropboxClient(fakeFiles)
		{
			ListFolderContinueAsyncFunc = _ =>
			{
				callCount++;
				// Return a Dropbox.Api.Files.ListFolderResult with the second page's entries
				var entries = fakeFiles2.Entries.Select(e =>
					new FileMetadata(
						id: e.AsFile.Id,
						name: e.AsFile.Name,
						clientModified: e.AsFile.ServerModified.UtcDateTime,
						serverModified: e.AsFile.ServerModified.UtcDateTime,
						rev: "123456789",
						size: ( ulong ) e.AsFile.Size,
						pathLower: e.AsFile.PathLower,
						pathDisplay: e.AsFile.PathDisplay,
						sharingInfo: null,
						isDownloadable: true,
						contentHash: new string('a', 64)
					)
				).Cast<Metadata>().ToList();
				var result = new ListFolderResult(entries, fakeFiles2.Cursor, fakeFiles2.HasMore);
				return Task.FromResult(result);
			}
		};
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		await client.InitializeClient("refresh", "key", "secret");

		var files = await client.ListFilesAsync("/test");

		Assert.HasCount(2, files);
		Assert.IsTrue(files.Any(f => f.Id == "1" && f.Name == "file1.txt"));
		Assert.IsTrue(files.Any(f => f.Id == "2" && f.Name == "file2.txt"));
		Assert.AreEqual(1, callCount); // ListFolderContinueAsync should be called once
	}

	[TestMethod]
	public async Task DownloadFileAsync_WritesFileAndReturnsPath()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		var fakeClient = new FakeIDropboxClient(fakeFiles);

		// Setup DownloadAsync to return a fake response with known content
		var fileContent = new byte[] { 1, 2, 3, 4, 5 };
		fakeClient.DownloadAsyncFunc = _ =>
			Task.FromResult<IDownloadResponse<FileMetadata>>(
				new FakeDownloadResponse(fileContent)
			);

		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		await client.InitializeClient("refresh", "key", "secret");

		var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempDir);
		try
		{
			var file = new CloudFile { Name = "testfile.bin", Path = "/testfile.bin" };
			var resultPath = await client.DownloadFileAsync(file, tempDir);
			Assert.IsTrue(File.Exists(resultPath));
			var writtenContent =
				await File.ReadAllBytesAsync(resultPath, TestContext.CancellationToken);
			CollectionAssert.AreEqual(fileContent, writtenContent);
		}
		finally
		{
			Directory.Delete(tempDir, true);
		}
	}

	[TestMethod]
	public async Task DownloadFileAsync_Exception()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		var fakeClient = new FakeIDropboxClient(fakeFiles)
		{
			DownloadAsyncFunc = _ => throw new NotSupportedException()
		};
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		await client.InitializeClient("refresh", "key", "secret");
		var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

		var file = new CloudFile { Name = "testfile.bin", Path = "/testfile.bin" };
		await Assert.ThrowsExactlyAsync<NotSupportedException>(async () =>
		{
			await client.DownloadFileAsync(file, tempDir);
		});
	}

	[TestMethod]
	public async Task DeleteFileAsync_ReturnsTrue_WhenDeleteSucceeds()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		var fakeClient = new FakeIDropboxClient(fakeFiles)
		{
			DeleteV2AsyncFunc = _ => Task.FromResult(new DeleteResult(new FileMetadata(
				id: "id",
				name: "file.txt",
				clientModified: DateTime.UtcNow,
				serverModified: DateTime.UtcNow,
				rev: "123456789",
				size: 1,
				pathLower: "/file.txt",
				pathDisplay: "/file.txt",
				sharingInfo: null,
				isDownloadable: true,
				contentHash: new string('a', 64)
			)))
		};
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		await client.InitializeClient("refresh", "key", "secret");
		var file = new CloudFile { Name = "file.txt", Path = "/file.txt" };
		var result = await client.DeleteFileAsync(file);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task DeleteFileAsync_ReturnsFalse_WhenDeleteThrows()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		var fakeClient = new FakeIDropboxClient(fakeFiles)
		{
			DeleteV2AsyncFunc = _ => throw new InvalidOperationException("fail")
		};
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		await client.InitializeClient("refresh", "key", "secret");
		var file = new CloudFile { Name = "file.txt", Path = "/file.txt" };
		var result = await client.DeleteFileAsync(file);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TestConnectionAsync_ReturnsTrue_WhenListFolderAsyncSucceeds()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		fakeFiles.Entries.Add(new FakeCloudFileEntry
		{
			IsFile = true, AsFile = new FakeCloudFileMetadata()
		});
		var fakeClient = new FakeIDropboxClient(fakeFiles);
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		await client.InitializeClient("refresh", "key", "secret");
		var result = await client.TestConnectionAsync();
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task TestConnectionAsync_ReturnsFalse_WhenListFolderAsyncThrows()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		var fakeClient = new FakeIDropboxClient(fakeFiles)
		{
			ListFolderAsyncFunc = _ => throw new InvalidOperationException("fail connect")
		};
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		await client.InitializeClient("refresh", "key", "secret");
		var result = await client.TestConnectionAsync();
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task InitializeClient_ReturnsTrue_WhenFirstInitialization()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		var fakeClient = new FakeIDropboxClient(fakeFiles);
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		var result = await client.InitializeClient("refresh", "key", "secret");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task InitializeClient_ReturnsFalse_WhenAlreadyInitializedAndNotExpired()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		var fakeClient = new FakeIDropboxClient(fakeFiles);
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		// First call initializes
		var result1 = await client.InitializeClient("refresh", "key", "secret");
		Assert.IsTrue(result1);
		// Simulate not expired by calling again immediately
		var result2 = await client.InitializeClient("refresh", "key", "secret");
		Assert.IsFalse(result2);
	}

	[TestMethod]
	public async Task ListFilesAsync_Throws_WhenClientThrowsException()
	{
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var tokenClient = new FakeDropboxCloudImportRefreshToken();
		var fakeFiles = new FakeFilesUserRoutes();
		var fakeClient = new FakeIDropboxClient(fakeFiles)
		{
			ListFolderAsyncFunc = _ => throw new InvalidOperationException("List failed!")
		};
		var client = new DropboxCloudImportClient(
			logger,
			appSettings,
			tokenClient,
			_ => fakeClient
		);
		await client.InitializeClient("refresh", "key", "secret");
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
		{
			await client.ListFilesAsync("/fail");
		});
	}
}
