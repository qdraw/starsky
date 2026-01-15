using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
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
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task ListFolderAsync_ShouldThrowException_WhenInvalidToken()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		await AssertThrowsAnyAsync<HttpRequestException, BadInputException>(
			async () => await client.ListFolderAsync("/test"),
			"Expected either HttpRequestException " +
			"or BadInputException to be thrown " +
			"(HttpRequestException is thrown when offline)");
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task ListFolderContinueAsync_ShouldThrowException_WhenInvalidToken()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		await AssertThrowsAnyAsync<HttpRequestException, BadInputException>(
			async () => await client.ListFolderContinueAsync("cursor"),
			"Expected either HttpRequestException " +
			"or BadInputException to be thrown "+
			"(HttpRequestException is thrown when offline)");
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task DownloadAsync_ShouldThrowException_WhenInvalidToken()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		
		await AssertThrowsAnyAsync<HttpRequestException, BadInputException>(
			async () => await client.DownloadAsync("/test.txt"),
			"Expected either HttpRequestException " +
			"or BadInputException to be thrown " +
			"(HttpRequestException is thrown when offline)");
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task DeleteV2Async_ShouldThrowException_WhenInvalidToken()
	{
		var client = new DropboxClientWrapper(InvalidAccessToken);
		
		await AssertThrowsAnyAsync<HttpRequestException, BadInputException>(
			async () => await client.DeleteV2Async("/test.txt"),
			"Expected either HttpRequestException " +
			"or BadInputException to be thrown "+
			"(HttpRequestException is thrown when offline)");
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
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

	/// <summary>
	/// Helper method to assert that an async action throws one of multiple exception types
	/// </summary>
	private static async Task AssertThrowsAnyAsync<TException1, TException2>(
		Func<Task> action,
		string message = "Expected one of the specified exceptions to be thrown")
		where TException1 : Exception
		where TException2 : Exception
	{
		var exceptionThrown = false;
		try
		{
			await action();
		}
		catch (TException1)
		{
			exceptionThrown = true;
		}
		catch (TException2)
		{
			exceptionThrown = true;
		}

		Assert.IsTrue(exceptionThrown, message);
	}
}
