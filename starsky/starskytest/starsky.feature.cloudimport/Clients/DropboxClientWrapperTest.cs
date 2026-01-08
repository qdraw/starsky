using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dropbox.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.cloudimport.Clients;

namespace starskytest.starsky.feature.cloudimport.Clients;

[TestClass]
public class DropboxClientWrapperTest
{
	private const string InvalidAccessToken = "";

	[TestMethod]
	public async Task ListFolderAsync_ShouldThrowException_WhenInvalidToken()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		await Assert.ThrowsExactlyAsync<BadInputException>(async () =>
		{
			await client.ListFolderAsync("/test");
		});
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task ListFolderContinueAsync_ShouldThrowException_WhenInvalidToken()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		await Assert.ThrowsExactlyAsync<BadInputException>(async () =>
		{
			await client.ListFolderContinueAsync("cursor");
		});
	}

	[TestMethod]
	public async Task DownloadAsync_ShouldThrowException_WhenInvalidToken()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		await Assert.ThrowsExactlyAsync<BadInputException>(async () =>
		{
			await client.DownloadAsync("/test.txt");
		});
	}

	[TestMethod]
	public async Task DeleteV2Async_ShouldThrowException_WhenInvalidToken()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		await Assert.ThrowsExactlyAsync<BadInputException>(async () =>
		{
			await client.DeleteV2Async("/test.txt");
		});
	}

	[TestMethod]
	public void Dispose_ShouldNotThrow()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		client.Dispose();
		Assert.IsNotNull(client);
	}

	[TestMethod]
	[SuppressMessage("ReSharper",
		"S3966: Resource 'client' has already been disposed explicitly " +
		"or through a using statement implicitly. " +
		"Remove the redundant disposal.")]
	public void Dispose_TwoTimes_ShouldNotThrow()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		client.Dispose(); // two times
		client.Dispose();
		Assert.IsNotNull(client);
	}
}
