using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
	public string? ContentHash { get; set; } = "hash";
}

public class FakeListFolderResult
{
	public List<FakeCloudFileEntry> Entries { get; set; } = new();
	public bool HasMore { get; set; }
	public string Cursor { get; set; } = "cursor";
}

// Fake token client
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
}
